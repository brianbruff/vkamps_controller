const { EventEmitter } = require('events');
const net = require('net');
const { parseTcpPacket } = require('./protocol');
const diag = require('./diagnostics');
const cfgModule = require('./config');

class TcpTransport extends EventEmitter {
  constructor() {
    super();
    this.socket = null;
    this.connected = false;
    this.buffer = '';
    this._sawFirstRx = false;
    this._rxCount = 0;
  }

  connect(config) {
    return new Promise((resolve, reject) => {
      const { lanIp, tcpPort } = config;
      diag.info('transport', `TCP connecting to ${lanIp}:${tcpPort}`, { host: lanIp, port: tcpPort });

      this.socket = new net.Socket();
      this.socket.setEncoding('utf8');

      this.socket.connect(tcpPort, lanIp, () => {
        diag.info('transport', `TCP connected to ${lanIp}:${tcpPort}`, { host: lanIp, port: tcpPort });
        // Send the init handshake.
        const initMsg = '11';
        try {
          this.socket.write(initMsg);
          diag.info('tx', `→ "${initMsg}"`, { bytes: initMsg, length: initMsg.length, transport: 'tcp' });
        } catch (e) {
          diag.warn('tx', `TCP init write failed: ${e && e.message}`, { error: e && e.message });
        }
        this.connected = true;
        this.emit('connected');
        resolve();
      });

      this.socket.on('data', (data) => {
        this.buffer += data;
        // Process complete lines
        const lines = this.buffer.split(/[\r\n]+/);
        this.buffer = lines.pop() || '';
        for (const line of lines) {
          if (line.trim()) {
            this._handleLine(line);
          }
        }
        // Also try to parse remaining buffer as a complete packet
        if (this.buffer.trim() && this.buffer.includes(',')) {
          const parts = this.buffer.trim().split(',');
          if (parts.length >= 8) {
            const handled = this._handleLine(this.buffer);
            if (handled) this.buffer = '';
          }
        }
      });

      this.socket.on('error', (err) => {
        diag.error('transport', `TCP error: ${err && err.message}`, {
          code: err && err.code,
          host: lanIp, port: tcpPort,
          message: err && err.message,
          stack: err && err.stack,
        });
        this.connected = false;
        this.emit('error', err);
        reject(err);
      });

      this.socket.on('close', (hadError) => {
        diag.info('transport', `TCP socket closed${hadError ? ' (with error)' : ''}`, { hadError: !!hadError });
        if (this.connected) {
          this.connected = false;
          this.emit('disconnected');
        }
      });
    });
  }

  _handleLine(line) {
    const packet = parseTcpPacket(line);
    if (!packet) {
      diag.warn('rx', `← (unparseable) "${line}"`, { raw: line, transport: 'tcp' });
      return false;
    }
    this._rxCount += 1;
    const verbose = (() => {
      try { return !!cfgModule.getConfig().verboseRxLogging; } catch { return false; }
    })();
    const isFirst = !this._sawFirstRx;
    if (isFirst) this._sawFirstRx = true;
    if (verbose || isFirst) {
      diag.log(verbose ? 'info' : 'info', 'rx', `← "${line.trim()}"`, {
        bytes: line, parsed: packet, transport: 'tcp', first: isFirst || undefined,
      });
    } else if (this._rxCount % 100 === 0) {
      diag.log('debug', 'rx', `← (sample #${this._rxCount}) "${line.trim()}"`, {
        bytes: line, parsed: packet, transport: 'tcp',
      });
    }
    this.emit('data', { type: 'tcp', data: packet });
    return true;
  }

  sendCommand(cmd) {
    if (this.socket && this.connected) {
      this.socket.write(cmd);
      diag.info('tx', `→ "${cmd}"`, { bytes: cmd, length: String(cmd).length, transport: 'tcp' });
    }
  }

  disconnect() {
    if (this.socket) {
      try {
        this.socket.write('99');
        diag.info('tx', `→ "99"`, { bytes: '99', length: 2, transport: 'tcp', reason: 'disconnect' });
      } catch (e) { /* ignore */ }
      setTimeout(() => {
        this.connected = false;
        try { this.socket.destroy(); } catch (e) { /* ignore */ }
      }, 100);
    }
  }

  destroy() {
    this.disconnect();
    this.removeAllListeners();
  }
}

module.exports = TcpTransport;
