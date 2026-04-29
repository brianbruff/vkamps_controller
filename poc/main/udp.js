const { EventEmitter } = require('events');
const dgram = require('dgram');
const { parseUdpPacket } = require('./protocol');
const diag = require('./diagnostics');
const cfgModule = require('./config');

class UdpTransport extends EventEmitter {
  constructor() {
    super();
    this.socket = null;
    this.connected = false;
    this.remoteIp = null;
    this.remotePort = null;
    this._sawFirstRx = false;
    this._rxCount = 0;
  }

  connect(config) {
    return new Promise((resolve, reject) => {
      const { lanIp, udpPort } = config;
      this.remoteIp = lanIp;
      this.remotePort = udpPort;
      diag.info('transport', `UDP binding (target ${lanIp}:${udpPort})`, { host: lanIp, port: udpPort });

      this.socket = dgram.createSocket({ type: 'udp4', reuseAddr: true });

      this.socket.on('message', (msg) => {
        const text = msg.toString('utf8');
        const packet = parseUdpPacket(text);
        if (!packet) {
          diag.warn('rx', `← (unparseable UDP) "${text.trim()}"`, { raw: text, transport: 'udp' });
          return;
        }
        this._rxCount += 1;
        const verbose = (() => {
          try { return !!cfgModule.getConfig().verboseRxLogging; } catch { return false; }
        })();
        const isFirst = !this._sawFirstRx;
        if (isFirst) this._sawFirstRx = true;
        if (verbose || isFirst) {
          diag.info('rx', `← "${text.trim()}"`, {
            bytes: text, parsed: packet, transport: 'udp', first: isFirst || undefined,
          });
        } else if (this._rxCount % 100 === 0) {
          diag.debug('rx', `← (sample #${this._rxCount}) "${text.trim()}"`, {
            bytes: text, parsed: packet, transport: 'udp',
          });
        }
        this.emit('data', { type: 'udp', data: packet });
      });

      this.socket.on('error', (err) => {
        diag.error('transport', `UDP error: ${err && err.message}`, {
          code: err && err.code,
          host: lanIp, port: udpPort,
          message: err && err.message,
          stack: err && err.stack,
        });
        this.connected = false;
        this.emit('error', err);
        reject(err);
      });

      // Bind locally to the same UDP port the amp listens on. The amp firmware
      // streams real-time telemetry to a fixed destination port (not the
      // ephemeral source of our init packet); without an explicit local-port
      // bind we silently miss every packet after the first reply.
      this.socket.bind(udpPort, () => {
        const initMsg = Buffer.from('11', 'utf8');
        this.socket.send(initMsg, 0, initMsg.length, this.remotePort, this.remoteIp, (err) => {
          if (err) {
            diag.error('transport', `UDP init send failed: ${err && err.message}`, {
              code: err && err.code, message: err && err.message, stack: err && err.stack,
            });
            return reject(err);
          }
          diag.info('tx', `→ "11"`, { bytes: '11', length: 2, transport: 'udp', reason: 'init' });
          const localPort = (() => { try { return this.socket.address().port; } catch { return udpPort; } })();
          diag.info('transport', `UDP bound (local :${localPort}, sending to ${this.remoteIp}:${this.remotePort})`, {
            localPort, host: this.remoteIp, port: this.remotePort,
          });
          this.connected = true;
          this.emit('connected');
          resolve();
        });
      });
    });
  }

  sendCommand(cmd) {
    if (this.socket && this.connected) {
      const buf = Buffer.from(cmd, 'utf8');
      this.socket.send(buf, 0, buf.length, this.remotePort, this.remoteIp);
      diag.info('tx', `→ "${cmd}"`, { bytes: cmd, length: String(cmd).length, transport: 'udp' });
    }
  }

  disconnect() {
    if (this.socket) {
      try {
        const buf = Buffer.from('99', 'utf8');
        this.socket.send(buf, 0, buf.length, this.remotePort, this.remoteIp);
        diag.info('tx', `→ "99"`, { bytes: '99', length: 2, transport: 'udp', reason: 'disconnect' });
      } catch (e) { /* ignore */ }
      setTimeout(() => {
        this.connected = false;
        try { this.socket.close(); } catch (e) { /* ignore */ }
      }, 100);
    }
  }

  destroy() {
    this.disconnect();
    this.removeAllListeners();
  }
}

module.exports = UdpTransport;
