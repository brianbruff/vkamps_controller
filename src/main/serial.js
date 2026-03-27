const { EventEmitter } = require('events');
const { parseSerialPacket } = require('./protocol');

class SerialTransport extends EventEmitter {
  constructor() {
    super();
    this.port = null;
    this.parser = null;
    this.connected = false;
    this.SerialPort = null;
    this.ReadlineParser = null;
  }

  async _loadSerialPort() {
    if (!this.SerialPort) {
      const sp = require('serialport');
      this.SerialPort = sp.SerialPort;
      this.ReadlineParser = sp.ReadlineParser;
    }
  }

  async listPorts() {
    await this._loadSerialPort();
    const ports = await this.SerialPort.list();
    return ports.map(p => ({
      path: p.path,
      manufacturer: p.manufacturer || '',
      vendorId: p.vendorId || '',
      productId: p.productId || '',
    }));
  }

  async connect(config) {
    await this._loadSerialPort();

    if (this.port && this.port.isOpen) {
      this.port.close();
    }

    const { comPort, baudRate } = config;
    if (!comPort) throw new Error('No COM port specified');

    // Handshake: open → send "11" → wait 500ms → send "99" → close → reopen → send "11"
    await this._openAndHandshake(comPort, baudRate);
  }

  async _openAndHandshake(portPath, baudRate) {
    return new Promise((resolve, reject) => {
      this.port = new this.SerialPort({
        path: portPath,
        baudRate: baudRate,
        dataBits: 8,
        parity: 'none',
        stopBits: 1,
        autoOpen: false,
      });

      this.port.open((err) => {
        if (err) return reject(new Error(`Failed to open port: ${err.message}`));

        // Send init
        this.port.write('11\n', (err) => {
          if (err) return reject(err);

          setTimeout(() => {
            // Send reset
            this.port.write('99\n', () => {
              this.port.close(() => {
                // Reopen
                this.port.open((err) => {
                  if (err) return reject(new Error(`Failed to reopen port: ${err.message}`));

                  this.port.write('11\n', (err) => {
                    if (err) return reject(err);

                    this.connected = true;
                    this._setupParser();
                    this.emit('connected');
                    resolve();
                  });
                });
              });
            });
          }, 500);
        });
      });

      this.port.on('error', (err) => {
        this.connected = false;
        this.emit('error', err);
      });

      this.port.on('close', () => {
        if (this.connected) {
          this.connected = false;
          this.emit('disconnected');
        }
      });
    });
  }

  _setupParser() {
    this.parser = this.port.pipe(new this.ReadlineParser({ delimiter: '\n' }));
    this.parser.on('data', (line) => {
      const result = parseSerialPacket(line);
      if (result) {
        this.emit('data', result);
      }
    });
  }

  sendCommand(cmd) {
    if (this.port && this.port.isOpen) {
      this.port.write(cmd + '\n');
    }
  }

  disconnect() {
    if (this.port && this.port.isOpen) {
      try {
        this.port.write('99\n');
      } catch (e) { /* ignore */ }
      setTimeout(() => {
        this.connected = false;
        try { this.port.close(); } catch (e) { /* ignore */ }
      }, 100);
    }
  }

  destroy() {
    this.disconnect();
    this.removeAllListeners();
  }
}

module.exports = SerialTransport;
