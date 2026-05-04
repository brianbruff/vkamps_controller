// MockTransport — emits packets in the same shape as transport.js so the
// renderer is identical with or without hardware. Driven by §7b of DESIGN.md.

const { EventEmitter } = require('events');

class MockTransport extends EventEmitter {
  constructor() {
    super();
    this.connected = false;
    this.timer = null;

    // Generator state (DESIGN.md §7b)
    this.tickMs = 20;
    this.t = 0;
    this.tickCount = 0;
    this.txOn = false;
    this.injectError = 0;
    this.band = 4;       // 30m default (device uses 1-8, where 4=30m)
    this.antenna = 1;
    this.bypass = 0;
    this.voltsPlus = false;
    this.fanFull = false;
    this.driveOverride = null; // 0..1 if set, overrides sine drive
    this.voltageDrift = 0;     // tenths of V
  }

  async connect(_config) {
    if (this.connected) return;
    this.connected = true;
    // Tiny async delay to feel like a real socket open
    setTimeout(() => {
      this.emit('connected');
    }, 50);
    this._start();
    // Optional: auto-enable TX for screenshot/demo runs
    if (process.env.VKAMP_MOCK_AUTOTX === '1') this.txOn = true;
  }

  _start() {
    if (this.timer) return;
    this.timer = setInterval(() => this._tick(), this.tickMs);
  }

  _stop() {
    if (this.timer) {
      clearInterval(this.timer);
      this.timer = null;
    }
  }

  _tick() {
    this.t += this.tickMs / 1000;
    this.tickCount += 1;
    const noise = (s = 1) => (Math.random() - 0.5) * s;

    const sine = 0.5 + 0.5 * Math.sin(2 * Math.PI * 0.4 * this.t);
    const drive = this.driveOverride != null ? this.driveOverride : (this.txOn ? sine : 0);

    const p1 = this.bypass ? 0 : Math.max(0, Math.round(720 * drive + noise(8)));
    const p2 = this.bypass ? 0 : Math.max(0, Math.round(p1 * 0.04 + noise(2)));
    const p4 = this.bypass ? 12 : Math.max(0, Math.round((40 + p1 * 0.06) + noise(3)));
    const p12 = this.bypass ? 0 : Math.max(0, Math.round(20 * drive + noise(2)));

    // 4-field meter packet — every tick (UDP shape)
    this.emit('data', { type: 'udp', data: { p1, p2, p4, p12 } });

    // 8-field state packet — every 5 ticks (~100 ms)
    if (this.tickCount % 5 === 0) {
      const baseV = this.voltsPlus ? 535 : 480;
      const p5 = baseV + Math.round(noise(4)) + this.voltageDrift;
      const p3 = Math.max(
        0,
        Math.round(28 + (this.txOn ? 12 * drive : 0) + noise(0.6)),
      );

      this.emit('data', {
        type: 'tcp',
        data: {
          p3,
          p5,
          p6: this.band,
          p7: this.antenna,
          p8: this.injectError,
          p9: 0,
          p10: this.fanFull ? 1 : 0,
          p11: this.bypass,
        },
      });
    }
  }

  sendCommand(cmd) {
    const c = String(cmd).trim();
    if (c === '21') this.bypass = 1;
    else if (c === '22') this.bypass = 0;
    else if (c === '23') this.injectError = 0;
    else if (/^3[1-4]$/.test(c)) this.antenna = Number(c[1]);
    else if (c === '41') this.voltsPlus = false;
    else if (c === '42') this.voltsPlus = true;
    else if (c === '45') this.fanFull = true;
    else if (c === '46') this.fanFull = false;
    else if (/^7[1-8]$/.test(c)) this.band = Number(c[1]);  // Store as 1-8 like device
    // 11, 99, 51-54, 61-66 accepted with no state change
  }

  disconnect() {
    this._stop();
    if (this.connected) {
      this.connected = false;
      // Defer slightly to mirror real-socket teardown timing
      setImmediate(() => this.emit('disconnected'));
    }
  }

  destroy() {
    this._stop();
    this.removeAllListeners();
    this.connected = false;
  }

  async listPorts() {
    return [
      { path: 'COM-MOCK', manufacturer: 'VKAmp Mock', vendorId: '', productId: '' },
    ];
  }

  // Dev-panel hooks (called from ipc.js)
  setMockTx(on) { this.txOn = !!on; }
  setDriveOverride(v) { this.driveOverride = v == null ? null : Math.max(0, Math.min(1, +v)); }
  setVoltageDrift(tenths) { this.voltageDrift = +tenths || 0; }
  setInjectError(code) { this.injectError = (+code | 0) % 8; }
  setForceBypass(on) { this.bypass = on ? 1 : 0; }
  setForceFanFull(on) { this.fanFull = !!on; }
  dropConnection(ms = 3000) {
    if (!this.connected) return;
    this._stop();
    this.connected = false;
    this.emit('disconnected');
    setTimeout(() => {
      this.connected = true;
      this.emit('connected');
      this._start();
    }, ms);
  }
}

module.exports = MockTransport;
