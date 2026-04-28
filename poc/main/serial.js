const { EventEmitter } = require('events');
const { parseSerialPacket } = require('./protocol');
const diag = require('./diagnostics');
const cfgModule = require('./config');

class SerialTransport extends EventEmitter {
  constructor() {
    super();
    this.port = null;
    this.parser = null;
    this.connected = false;
    this.SerialPort = null;
    this.ReadlineParser = null;
    this._sawFirstRx = false;
    this._rxCount = 0;
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
    if (!comPort) {
      const err = new Error('No COM port specified');
      diag.error('transport', err.message, { baudRate });
      throw err;
    }
    diag.info('transport', `Serial connecting to ${comPort} @ ${baudRate}`, { comPort, baudRate });

    // Handshake: open → send "11" → wait 500ms → send "99" → close → reopen → send "11"
    try {
      await this._openAndHandshake(comPort, baudRate);
      diag.info('transport', `Serial connected to ${comPort}`, { comPort, baudRate });
    } catch (err) {
      diag.error('transport', `Serial connect failed: ${err && err.message}`, {
        comPort, baudRate, message: err && err.message, stack: err && err.stack,
      });
      throw err;
    }
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
          diag.info('tx', `→ "11\\n"`, { bytes: '11\n', length: 3, transport: 'serial', reason: 'handshake init' });

          setTimeout(() => {
            // Send reset
            this.port.write('99\n', () => {
              diag.info('tx', `→ "99\\n"`, { bytes: '99\n', length: 3, transport: 'serial', reason: 'handshake reset' });
              this.port.close(() => {
                // Reopen
                this.port.open((err) => {
                  if (err) return reject(new Error(`Failed to reopen port: ${err.message}`));

                  this.port.write('11\n', (err) => {
                    if (err) return reject(err);
                    diag.info('tx', `→ "11\\n"`, { bytes: '11\n', length: 3, transport: 'serial', reason: 'handshake reopen' });

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
        diag.error('transport', `Serial error: ${err && err.message}`, {
          message: err && err.message, stack: err && err.stack,
        });
        this.connected = false;
        this.emit('error', err);
      });

      this.port.on('close', () => {
        diag.info('transport', 'Serial port closed', {});
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
      if (!result) {
        diag.warn('rx', `← (unparseable serial) "${line.trim()}"`, { raw: line, transport: 'serial' });
        return;
      }
      this._rxCount += 1;
      const verbose = (() => {
        try { return !!cfgModule.getConfig().verboseRxLogging; } catch { return false; }
      })();
      const isFirst = !this._sawFirstRx;
      if (isFirst) this._sawFirstRx = true;
      if (verbose || isFirst) {
        diag.info('rx', `← "${line.trim()}"`, {
          bytes: line, parsed: result.data, transport: 'serial',
          shape: result.type, first: isFirst || undefined,
        });
      } else if (this._rxCount % 100 === 0) {
        diag.debug('rx', `← (sample #${this._rxCount}) "${line.trim()}"`, {
          bytes: line, parsed: result.data, transport: 'serial', shape: result.type,
        });
      }
      this.emit('data', result);
    });
  }

  sendCommand(cmd) {
    if (this.port && this.port.isOpen) {
      this.port.write(cmd + '\n');
      diag.info('tx', `→ "${cmd}\\n"`, { bytes: cmd + '\n', length: String(cmd).length + 1, transport: 'serial' });
    }
  }

  disconnect() {
    if (this.port && this.port.isOpen) {
      try {
        this.port.write('99\n');
        diag.info('tx', `→ "99\\n"`, { bytes: '99\n', length: 3, transport: 'serial', reason: 'disconnect' });
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
