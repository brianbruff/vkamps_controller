const { ipcMain, dialog } = require('electron');
const fs = require('fs');
const path = require('path');
const os = require('os');
const Transport = require('./transport');
const MockTransport = require('./mock');
const config = require('./config');
const diag = require('./diagnostics');
const { CMD, VOLTAGE_CMDS, CAT_CMDS } = require('./protocol');

const USE_MOCK = process.env.VKAMP_MOCK === '1';

// Hard cap on a single connect attempt before we give up and tear down.
// TCP/serial otherwise inherit OS-level timeouts that can be ~75 s.
const CONNECT_TIMEOUT_MS = Number(process.env.VKAMP_CONNECT_TIMEOUT_MS) || 15000;

let transport = USE_MOCK ? new MockTransport() : new Transport();
let mainWindow = null;

// Reverse-lookup table for command names — for diagnostic messages.
function commandName(cmd) {
  const c = String(cmd);
  for (const [name, val] of Object.entries(CMD)) {
    if (val === c) return name;
  }
  return null;
}

function setupIPC(win) {
  mainWindow = win;

  diag.info('lifecycle', `IPC setup (transport=${USE_MOCK ? 'MOCK' : 'real'})`, { mock: USE_MOCK });

  // Forward every diagnostic entry to the renderer.
  diag.on('entry', (entry) => {
    if (mainWindow && !mainWindow.isDestroyed()) {
      try { mainWindow.webContents.send('diag:entry', entry); } catch {}
    }
  });
  diag.on('cleared', () => {
    if (mainWindow && !mainWindow.isDestroyed()) {
      try { mainWindow.webContents.send('diag:cleared'); } catch {}
    }
  });

  transport.on('data', (pkt) => {
    if (mainWindow && !mainWindow.isDestroyed()) {
      mainWindow.webContents.send('amp:data', pkt);
    }
  });

  transport.on('connected', () => {
    if (mainWindow && !mainWindow.isDestroyed()) {
      mainWindow.webContents.send('amp:connection', true);
    }
  });

  transport.on('disconnected', () => {
    if (mainWindow && !mainWindow.isDestroyed()) {
      mainWindow.webContents.send('amp:connection', false);
    }
  });

  transport.on('error', (err) => {
    if (mainWindow && !mainWindow.isDestroyed()) {
      mainWindow.webContents.send('amp:error', err && err.message ? err.message : String(err));
    }
  });

  ipcMain.handle('amp:connect', async () => {
    const cfg = config.getConfig();
    const target = cfg.mode === 'USB'
      ? `${cfg.comPort || '(unset)'} @ ${cfg.baudRate}`
      : `${cfg.lanIp}:${cfg.tcpPort}`;

    let timeoutHandle = null;
    let timedOut = false;
    const timeoutPromise = new Promise((_, reject) => {
      timeoutHandle = setTimeout(() => {
        timedOut = true;
        const err = new Error(`Connection timed out after ${CONNECT_TIMEOUT_MS / 1000}s`);
        err.code = 'EVKAMPCONNECTTIMEOUT';
        reject(err);
      }, CONNECT_TIMEOUT_MS);
    });

    const startedAt = Date.now();
    try {
      await Promise.race([transport.connect(cfg), timeoutPromise]);
      clearTimeout(timeoutHandle);
    } catch (err) {
      clearTimeout(timeoutHandle);
      const elapsedMs = Date.now() - startedAt;
      if (timedOut) {
        diag.error('transport', `Connect timed out after ${CONNECT_TIMEOUT_MS / 1000}s — abandoning attempt`, {
          mode: cfg.mode,
          target,
          timeoutMs: CONNECT_TIMEOUT_MS,
          elapsedMs,
        });
        // Tear down any half-open sockets/ports before surfacing the error.
        try { transport.disconnect(); } catch {}
      }
      // Re-throw so the renderer's invoke() rejects.
      throw err;
    }

    // Send post-connect config commands (CAT, voltage)
    try {
      const catCmd = CAT_CMDS[cfg.cat] || CMD.CAT_RF;
      diag.info('command', `cmd ${catCmd} (${commandName(catCmd) || 'CAT'})`, { cmd: catCmd, source: 'post-connect' });
      transport.sendCommand(catCmd);
      const voltCmd = VOLTAGE_CMDS[cfg.voltage] || CMD.VOLTAGE_48;
      diag.info('command', `cmd ${voltCmd} (${commandName(voltCmd) || 'VOLTAGE'})`, { cmd: voltCmd, source: 'post-connect' });
      transport.sendCommand(voltCmd);
    } catch (e) { /* non-fatal */ }

    return true;
  });

  ipcMain.handle('amp:disconnect', () => {
    diag.info('command', 'disconnect requested', {});
    transport.disconnect();
    return true;
  });

  ipcMain.handle('amp:sendCommand', (_event, cmd) => {
    const name = commandName(cmd);
    diag.info('command', `cmd ${cmd}${name ? ` (${name})` : ''}`, { cmd, name });
    transport.sendCommand(cmd);
    return true;
  });

  ipcMain.handle('amp:getConfig', () => config.getConfig());

  ipcMain.handle('amp:saveConfig', (_event, cfg) => {
    config.saveConfig(cfg);
    return true;
  });

  ipcMain.handle('amp:listPorts', async () => {
    if (typeof transport.listPorts === 'function') {
      return await transport.listPorts();
    }
    return [];
  });

  ipcMain.handle('amp:importSaveTxt', async () => {
    const result = await dialog.showOpenDialog(mainWindow, {
      title: 'Import save.txt',
      filters: [{ name: 'Text Files', extensions: ['txt'] }],
      properties: ['openFile'],
    });
    if (result.canceled || !result.filePaths.length) return false;
    return config.importSaveTxt(result.filePaths[0]);
  });

  ipcMain.handle('amp:exportSaveTxt', async () => {
    const result = await dialog.showSaveDialog(mainWindow, {
      title: 'Export save.txt',
      defaultPath: 'save.txt',
      filters: [{ name: 'Text Files', extensions: ['txt'] }],
    });
    if (result.canceled || !result.filePath) return false;
    config.exportSaveTxt(result.filePath);
    return true;
  });

  ipcMain.handle('amp:setAlwaysOnTop', (_event, value) => {
    if (mainWindow) mainWindow.setAlwaysOnTop(!!value);
    return true;
  });

  ipcMain.handle('amp:getProtocol', () => {
    const { CMD: C, BANDS, ERROR_CODES, SCALE_LABELS, ANTENNA_CMDS, BAND_CMDS } = require('./protocol');
    return { CMD: C, BANDS, ERROR_CODES, SCALE_LABELS, ANTENNA_CMDS, BAND_CMDS };
  });

  // Mock / dev knobs (no-op when not running mock)
  ipcMain.handle('amp:isMock', () => USE_MOCK);
  ipcMain.handle('amp:mock', (_event, action, payload) => {
    if (!USE_MOCK || !transport) return false;
    switch (action) {
      case 'tx':            transport.setMockTx(payload); break;
      case 'drive':         transport.setDriveOverride(payload); break;
      case 'voltageDrift':  transport.setVoltageDrift(payload); break;
      case 'injectError':   transport.setInjectError(payload); break;
      case 'forceBypass':   transport.setForceBypass(payload); break;
      case 'forceFanFull':  transport.setForceFanFull(payload); break;
      case 'dropConnection': transport.dropConnection(payload || 3000); break;
      default: return false;
    }
    return true;
  });

  // ---- Diagnostics IPC ----

  ipcMain.handle('diag:getAll', () => diag.getAll());
  ipcMain.handle('diag:clear', () => { diag.clear(); return true; });
  ipcMain.handle('diag:logUi', (_event, level, message, detail) => {
    diag.log(level || 'info', 'ui', String(message || ''), detail);
    return true;
  });
  ipcMain.handle('diag:save', async () => {
    // Default location: Desktop if writable, else Documents, else home.
    const stamp = (() => {
      const d = new Date();
      const pad = (n, w = 2) => String(n).padStart(w, '0');
      return `${d.getFullYear()}${pad(d.getMonth() + 1)}${pad(d.getDate())}-` +
             `${pad(d.getHours())}${pad(d.getMinutes())}${pad(d.getSeconds())}`;
    })();
    const home = os.homedir();
    const candidates = [
      path.join(home, 'Desktop'),
      path.join(home, 'Documents'),
      home,
    ];
    let baseDir = home;
    for (const c of candidates) {
      try { fs.accessSync(c, fs.constants.W_OK); baseDir = c; break; } catch {}
    }
    const defaultFilename = `vkamp-poc-diag-${stamp}.txt`;
    const result = await dialog.showSaveDialog(mainWindow, {
      title: 'Save diagnostic log',
      defaultPath: path.join(baseDir, defaultFilename),
      filters: [{ name: 'Text Files', extensions: ['txt'] }],
    });
    if (result.canceled || !result.filePath) {
      diag.info('ui', 'Diagnostic log save cancelled', {});
      return null;
    }
    try {
      const text = diag.dumpToText({ settings: config.getConfig() });
      fs.writeFileSync(result.filePath, text, 'utf-8');
      diag.info('ui', `Diagnostic log saved`, { path: result.filePath, bytes: Buffer.byteLength(text, 'utf-8') });
      return result.filePath;
    } catch (e) {
      diag.error('error', `Diagnostic log save failed: ${e && e.message}`, {
        message: e && e.message, stack: e && e.stack, path: result.filePath,
      });
      throw e;
    }
  });
}

function cleanup() {
  if (transport) transport.disconnect();
}

module.exports = { setupIPC, cleanup };
