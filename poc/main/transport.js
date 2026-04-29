const { EventEmitter } = require('events');
const SerialTransport = require('./serial');
const TcpTransport = require('./tcp');
const UdpTransport = require('./udp');
const diag = require('./diagnostics');

class Transport extends EventEmitter {
  constructor() {
    super();
    this.serial = null;
    this.tcp = null;
    this.udp = null;
    this.mode = null;
    this.connected = false;
    this._lastP9 = 0;
  }

  async connect(config) {
    this.disconnect();
    this.mode = config.mode;

    diag.info('transport', `connect() requested (mode=${config.mode})`, {
      mode: config.mode,
      target: config.mode === 'USB'
        ? `${config.comPort || '(unset)'} @ ${config.baudRate}`
        : `${config.lanIp}:${config.tcpPort} (TCP) / :${config.udpPort} (UDP)`,
    });

    if (config.mode === 'USB') {
      this.serial = new SerialTransport();
      this._bindEvents(this.serial);
      await this.serial.connect(config);
    } else {
      // LAN mode — both TCP and UDP simultaneously
      this.tcp = new TcpTransport();
      this.udp = new UdpTransport();

      this.tcp.on('data', (pkt) => {
        // Match legacy Helios DX behavior (Main.cs:1371-1376): on every p9
        // transition 0→1 (TX idle→active) re-kick the UDP stream by re-sending
        // "11". The amp's UDP firmware only streams p1/p2/p4/p12 telemetry
        // while TX is keyed and stops between bursts; without this re-kick
        // we never see output/reflected/current/input power.
        if (pkt && pkt.type === 'tcp' && pkt.data && typeof pkt.data.p9 === 'number') {
          const p9 = pkt.data.p9;
          if (this._lastP9 === 0 && p9 !== 0 && this.udp && this.udp.connected) {
            diag.info('transport', 'TX active (p9 0→1) — re-kicking UDP stream', { p9 });
            this.udp.sendCommand('11');
          }
          this._lastP9 = p9;
        }
        this.emit('data', pkt);
      });
      this.udp.on('data', (pkt) => this.emit('data', pkt));

      this.tcp.on('error', (err) => this.emit('error', err));
      this.udp.on('error', (err) => this.emit('error', err));

      this.tcp.on('disconnected', () => {
        this.connected = false;
        this.emit('disconnected');
      });

      await this.tcp.connect(config);
      await this.udp.connect(config);

      this.connected = true;
      this.emit('connected');
    }
  }

  _bindEvents(transport) {
    transport.on('data', (pkt) => this.emit('data', pkt));
    transport.on('connected', () => {
      this.connected = true;
      this.emit('connected');
    });
    transport.on('disconnected', () => {
      this.connected = false;
      this.emit('disconnected');
    });
    transport.on('error', (err) => this.emit('error', err));
  }

  sendCommand(cmd) {
    if (this.mode === 'USB' && this.serial) {
      this.serial.sendCommand(cmd);
    } else {
      // In LAN mode, commands go via TCP
      if (this.tcp) this.tcp.sendCommand(cmd);
    }
  }

  disconnect() {
    const wasConnected = this.connected;
    if (this.serial || this.tcp || this.udp) {
      diag.info('transport', `disconnect() requested (mode=${this.mode || 'none'})`, { wasConnected });
    }
    if (this.serial) {
      this.serial.destroy();
      this.serial = null;
    }
    if (this.tcp) {
      this.tcp.destroy();
      this.tcp = null;
    }
    if (this.udp) {
      this.udp.destroy();
      this.udp = null;
    }
    this.connected = false;
    this.mode = null;
    this._lastP9 = 0;
  }

  async listPorts() {
    const serial = new SerialTransport();
    try {
      return await serial.listPorts();
    } catch {
      return [];
    }
  }
}

module.exports = Transport;
