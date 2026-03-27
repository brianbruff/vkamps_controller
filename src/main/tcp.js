const { EventEmitter } = require('events');
const net = require('net');
const { parseTcpPacket } = require('./protocol');

class TcpTransport extends EventEmitter {
  constructor() {
    super();
    this.socket = null;
    this.connected = false;
    this.buffer = '';
  }

  connect(config) {
    return new Promise((resolve, reject) => {
      const { lanIp, tcpPort } = config;

      this.socket = new net.Socket();
      this.socket.setEncoding('utf8');

      this.socket.connect(tcpPort, lanIp, () => {
        this.socket.write('11');
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
            const packet = parseTcpPacket(line);
            if (packet) {
              this.emit('data', { type: 'tcp', data: packet });
            }
          }
        }
        // Also try to parse remaining buffer as a complete packet
        if (this.buffer.trim() && this.buffer.includes(',')) {
          const parts = this.buffer.trim().split(',');
          if (parts.length >= 8) {
            const packet = parseTcpPacket(this.buffer);
            if (packet) {
              this.emit('data', { type: 'tcp', data: packet });
              this.buffer = '';
            }
          }
        }
      });

      this.socket.on('error', (err) => {
        this.connected = false;
        this.emit('error', err);
        reject(err);
      });

      this.socket.on('close', () => {
        if (this.connected) {
          this.connected = false;
          this.emit('disconnected');
        }
      });
    });
  }

  sendCommand(cmd) {
    if (this.socket && this.connected) {
      this.socket.write(cmd);
    }
  }

  disconnect() {
    if (this.socket) {
      try { this.socket.write('99'); } catch (e) { /* ignore */ }
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
