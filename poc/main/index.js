const { app, BrowserWindow } = require('electron');
const path = require('path');
const os = require('os');
const { setupIPC, cleanup } = require('./ipc');
const config = require('./config');
const diag = require('./diagnostics');

let mainWindow = null;

// Capture uncaught errors into the diagnostic log so the third-party tester sees them.
process.on('uncaughtException', (err) => {
  try {
    diag.error('error', `uncaughtException: ${err && err.message}`, {
      name: err && err.name, message: err && err.message, stack: err && err.stack,
    });
  } catch {}
});
process.on('unhandledRejection', (reason) => {
  try {
    const msg = reason && reason.message ? reason.message : String(reason);
    diag.error('error', `unhandledRejection: ${msg}`, {
      message: msg, stack: reason && reason.stack,
    });
  } catch {}
});

function createWindow() {
  const cfg = config.getConfig();

  diag.info('lifecycle', 'App launched', {
    appVersion: (() => { try { return app.getVersion(); } catch { return ''; } })(),
    electron: process.versions.electron,
    node:     process.versions.node,
    chrome:   process.versions.chrome,
    platform: `${process.platform} ${process.arch}`,
    osType:   `${os.type()} ${os.release()}`,
    appPath:  app.getAppPath(),
    mock:     process.env.VKAMP_MOCK === '1',
  });

  mainWindow = new BrowserWindow({
    width: 1024,
    height: 640,
    minWidth: 880,
    minHeight: 580,
    x: cfg.windowX || undefined,
    y: cfg.windowY || undefined,
    alwaysOnTop: cfg.alwaysOnTop,
    title: 'VKAmp [POC TESTER] — Helios DX Replacement',
    backgroundColor: '#0e1620',
    frame: true,
    icon: path.join(__dirname, '..', 'assets', 'icon.png'),
    webPreferences: {
      preload: path.join(__dirname, '..', 'preload', 'preload.js'),
      contextIsolation: true,
      nodeIntegration: false,
    },
  });

  // Pin the title even after the page loads (Electron lets pages override title)
  mainWindow.on('page-title-updated', (e) => {
    e.preventDefault();
  });

  setupIPC(mainWindow);

  diag.info('lifecycle', 'Main window created', {
    width: 1024, height: 640, alwaysOnTop: !!cfg.alwaysOnTop,
  });

  const isDev = !app.isPackaged && process.env.VKAMP_DEV_SERVER !== '0';
  const devServerUrl = process.env.VKAMP_DEV_URL || 'http://localhost:5173';

  // Use the dev server only if it's reachable; otherwise fall back to the built file.
  const indexHtml = path.join(__dirname, '..', 'renderer', 'dist', 'index.html');
  const fs = require('fs');

  if (isDev && process.env.VKAMP_USE_DEV_SERVER === '1') {
    mainWindow.loadURL(devServerUrl);
  } else if (fs.existsSync(indexHtml)) {
    mainWindow.loadFile(indexHtml);
  } else if (isDev) {
    mainWindow.loadURL(devServerUrl);
  } else {
    mainWindow.loadFile(indexHtml); // will error meaningfully if missing
  }

  mainWindow.on('moved', () => {
    if (!mainWindow) return;
    const [x, y] = mainWindow.getPosition();
    config.saveConfig({ windowX: x, windowY: y });
  });

  mainWindow.on('closed', () => {
    mainWindow = null;
  });

  // Forward renderer console to main stdout for debugging
  if (process.env.VKAMP_LOG_RENDERER === '1') {
    mainWindow.webContents.on('console-message', (_e, level, msg, line, src) => {
      console.log(`[renderer ${level}] ${msg} (${src}:${line})`);
    });
    mainWindow.webContents.on('render-process-gone', (_e, details) => {
      console.error('[renderer] process-gone', details);
    });
  }

  // Optional: self-test for verification — drives the Connect → fail → save flow
  // headlessly so we can capture a screenshot of the diagnostics view and dump
  // the log to disk without manual interaction.
  // Set VKAMP_VERIFY=1 to enable. Honors:
  //   VKAMP_VERIFY_DIAG_DUMP — path for the diagnostic text dump
  //   VKAMP_VERIFY_SCREENSHOT — path for the diagnostics-view PNG
  //   VKAMP_VERIFY_HOST / VKAMP_VERIFY_PORT — connect target (default 127.0.0.1:5005)
  //   VKAMP_VERIFY_EXIT=1 — quit when done
  if (process.env.VKAMP_VERIFY === '1') {
    const fs = require('fs');
    const ipcMod = require('./ipc');
    const cfg2 = config.getConfig();
    const host = process.env.VKAMP_VERIFY_HOST || '127.0.0.1';
    const port = Number(process.env.VKAMP_VERIFY_PORT || 5005);
    const dumpPath = process.env.VKAMP_VERIFY_DIAG_DUMP || '/tmp/vkamp-poc-diag-test.txt';
    const shotPath = process.env.VKAMP_VERIFY_SCREENSHOT;
    const exitWhenDone = process.env.VKAMP_VERIFY_EXIT === '1';

    // Force a known LAN config so the verification is deterministic.
    config.saveConfig({ mode: 'TCP', lanIp: host, tcpPort: port });

    mainWindow.webContents.on('did-finish-load', () => {
      setTimeout(async () => {
        try {
          // 1) Open the diagnostics view in the renderer for the screenshot.
          await mainWindow.webContents.executeJavaScript(`
            (async () => {
              if (!window.api) return false;
              await window.api.logUi('info', 'Verify mode: simulating Connect button click', { host: '${host}', port: ${port} });
              return true;
            })();
          `);

          // 2) Drive a Connect via the renderer (fires UI log + invokes amp:connect).
          await mainWindow.webContents.executeJavaScript(`
            (async () => {
              try { await window.api.connect(); }
              catch (e) { /* expected to fail */ }
            })();
          `);

          // Give the connect attempt a moment to fail and log.
          await new Promise(r => setTimeout(r, 800));

          // 3) Open the diagnostics screen (click the DIAG button if present).
          await mainWindow.webContents.executeJavaScript(`
            (async () => {
              const btn = document.querySelector('button[aria-label="Diagnostics"]');
              if (btn) btn.click();
            })();
          `);
          await new Promise(r => setTimeout(r, 500));

          // 4) Screenshot the diagnostics view.
          if (shotPath) {
            try {
              const img = await mainWindow.webContents.capturePage();
              fs.writeFileSync(shotPath, img.toPNG());
              console.log('[verify] screenshot ->', shotPath);
            } catch (e) {
              console.error('[verify] screenshot failed', e);
            }
          }

          // 5) Write the diagnostic dump.
          try {
            const text = diag.dumpToText({ settings: config.getConfig() });
            fs.writeFileSync(dumpPath, text, 'utf-8');
            console.log('[verify] diag dump ->', dumpPath);
          } catch (e) {
            console.error('[verify] diag dump failed', e);
          }

          if (exitWhenDone) {
            setTimeout(() => app.quit(), 200);
          }
        } catch (e) {
          console.error('[verify] failed', e);
          if (exitWhenDone) app.quit();
        }
      }, 800);
    });
  }

  // Optional: self-capture for verification (set VKAMP_CAPTURE=path)
  if (process.env.VKAMP_CAPTURE) {
    const fs = require('fs');
    const out = process.env.VKAMP_CAPTURE;
    const delayMs = Number(process.env.VKAMP_CAPTURE_DELAY_MS || 3500);
    const exitAfter = process.env.VKAMP_CAPTURE_EXIT === '1';
    mainWindow.webContents.on('did-finish-load', () => {
      setTimeout(async () => {
        try {
          const img = await mainWindow.webContents.capturePage();
          fs.writeFileSync(out, img.toPNG());
          console.log('[capture] wrote', out);
          if (exitAfter) app.quit();
        } catch (e) {
          console.error('[capture] failed', e);
          if (exitAfter) app.quit();
        }
      }, delayMs);
    });
  }
}

app.whenReady().then(createWindow);

app.on('window-all-closed', () => {
  diag.info('lifecycle', 'window-all-closed; quitting');
  cleanup();
  app.quit();
});

app.on('before-quit', () => {
  diag.info('lifecycle', 'before-quit');
  cleanup();
});

app.on('activate', () => {
  if (BrowserWindow.getAllWindows().length === 0) {
    createWindow();
  }
});
