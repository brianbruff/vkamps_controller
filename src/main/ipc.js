const { ipcMain, dialog } = require('electron');
const Transport = require('./transport');
const config = require('./config');
const { CMD, VOLTAGE_CMDS, CAT_CMDS } = require('./protocol');

let transport = new Transport();
let mainWindow = null;

function setupIPC(win) {
  mainWindow = win;

  // Forward transport events to renderer
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
      mainWindow.webContents.send('amp:error', err.message);
    }
  });

  // Connection
  ipcMain.handle('amp:connect', async () => {
    const cfg = config.getConfig();
    await transport.connect(cfg);

    // After connect, send CAT and voltage config commands
    const catCmd = CAT_CMDS[cfg.cat] || CMD.CAT_RF;
    transport.sendCommand(catCmd);

    const voltCmd = VOLTAGE_CMDS[cfg.voltage] || CMD.VOLTAGE_48;
    transport.sendCommand(voltCmd);

    return true;
  });

  ipcMain.handle('amp:disconnect', () => {
    transport.disconnect();
    return true;
  });

  ipcMain.handle('amp:sendCommand', (_event, cmd) => {
    transport.sendCommand(cmd);
    return true;
  });

  // Config
  ipcMain.handle('amp:getConfig', () => {
    return config.getConfig();
  });

  ipcMain.handle('amp:saveConfig', (_event, cfg) => {
    config.saveConfig(cfg);
    return true;
  });

  // Serial ports
  ipcMain.handle('amp:listPorts', async () => {
    return await transport.listPorts();
  });

  // Import save.txt
  ipcMain.handle('amp:importSaveTxt', async () => {
    const result = await dialog.showOpenDialog(mainWindow, {
      title: 'Import save.txt',
      filters: [{ name: 'Text Files', extensions: ['txt'] }],
      properties: ['openFile'],
    });
    if (result.canceled || !result.filePaths.length) return false;
    return config.importSaveTxt(result.filePaths[0]);
  });

  // Export save.txt
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

  // Window controls
  ipcMain.handle('amp:setAlwaysOnTop', (_event, value) => {
    if (mainWindow) mainWindow.setAlwaysOnTop(value);
    return true;
  });

  ipcMain.handle('amp:getProtocol', () => {
    const { CMD, BANDS, ERROR_CODES, SCALE_LABELS, ANTENNA_CMDS, BAND_CMDS } = require('./protocol');
    return { CMD, BANDS, ERROR_CODES, SCALE_LABELS, ANTENNA_CMDS, BAND_CMDS };
  });
}

function cleanup() {
  if (transport) {
    transport.disconnect();
  }
}

module.exports = { setupIPC, cleanup };
