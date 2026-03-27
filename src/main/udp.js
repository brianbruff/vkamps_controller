const { EventEmitter } = require('events');
const dgram = require('dgram');
const { parseUdpPacket } = require('./protocol');

class UdpTransport extends EventEmitter {
  constructor() {
    super();
    this.socket = null;
    this.connected = false;
    this.remoteIp = null;
    this.remotePort = null;
  }

  connect(config) {
    return new Promise((resolve, reject) => {
      const { lanIp, udpPort } = config;
      this.remoteIp = lanIp;
      this.remotePort = udpPort;

      this.socket = dgram.createSocket('udp4');

      this.socket.on('message', (msg) => {
        const text = msg.toString('utf8');
        const packet = parseUdpPacket(text);
        if (packet) {
          this.emit('data', { type: 'udp', data: packet });
        }
      });

      this.socket.on('error', (err) => {
        this.connected = false;
        this.emit('error', err);
        reject(err);
      });

      this.socket.bind(() => {
        const initMsg = Buffer.from('11', 'utf8');
        this.socket.send(initMsg, 0, initMsg.length, this.remotePort, this.remoteIp, (err) => {
          if (err) return reject(err);
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
    }
  }

  disconnect() {
    if (this.socket) {
      try {
        const buf = Buffer.from('99', 'utf8');
        this.socket.send(buf, 0, buf.length, this.remotePort, this.remoteIp);
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
