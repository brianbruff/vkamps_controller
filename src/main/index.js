const { app, BrowserWindow } = require('electron');
const path = require('path');
const { setupIPC, cleanup } = require('./ipc');
const config = require('./config');

let mainWindow = null;

function createWindow() {
  const cfg = config.getConfig();

  mainWindow = new BrowserWindow({
    width: 900,
    height: 500,
    minWidth: 900,
    minHeight: 460,
    x: cfg.windowX || undefined,
    y: cfg.windowY || undefined,
    alwaysOnTop: cfg.alwaysOnTop,
    backgroundColor: '#0a0a0a',
    frame: true,
    titleBarStyle: 'hidden',
    titleBarOverlay: false,
    icon: path.join(__dirname, '..', 'assets', 'icon.png'),
    webPreferences: {
      preload: path.join(__dirname, '..', 'preload', 'preload.js'),
      contextIsolation: true,
      nodeIntegration: false,
    },
  });

  setupIPC(mainWindow);

  // In development, load from Vite dev server; in production, load built file
  const isDev = !app.isPackaged;
  if (isDev) {
    mainWindow.loadURL('http://localhost:5173');
  } else {
    mainWindow.loadFile(path.join(__dirname, '..', 'renderer', 'dist', 'index.html'));
  }

  // Save window position on move
  mainWindow.on('moved', () => {
    const [x, y] = mainWindow.getPosition();
    config.saveConfig({ windowX: x, windowY: y });
  });

  mainWindow.on('closed', () => {
    mainWindow = null;
  });
}

app.whenReady().then(createWindow);

app.on('window-all-closed', () => {
  cleanup();
  app.quit();
});

app.on('before-quit', () => {
  cleanup();
});

app.on('activate', () => {
  if (BrowserWindow.getAllWindows().length === 0) {
    createWindow();
  }
});
