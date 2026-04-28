const { contextBridge, ipcRenderer } = require('electron');

const api = {
  // Connection
  connect: () => ipcRenderer.invoke('amp:connect'),
  disconnect: () => ipcRenderer.invoke('amp:disconnect'),
  send: (cmd) => ipcRenderer.invoke('amp:sendCommand', cmd),

  // Telemetry / events
  onPacket: (cb) => {
    const handler = (_e, pkt) => cb(pkt);
    ipcRenderer.on('amp:data', handler);
    return () => ipcRenderer.removeListener('amp:data', handler);
  },
  onConnection: (cb) => {
    const handler = (_e, connected) => cb(connected);
    ipcRenderer.on('amp:connection', handler);
    return () => ipcRenderer.removeListener('amp:connection', handler);
  },
  onError: (cb) => {
    const handler = (_e, msg) => cb(msg);
    ipcRenderer.on('amp:error', handler);
    return () => ipcRenderer.removeListener('amp:error', handler);
  },

  // Config
  getConfig: () => ipcRenderer.invoke('amp:getConfig'),
  saveConfig: (cfg) => ipcRenderer.invoke('amp:saveConfig', cfg),

  // Serial ports
  listPorts: () => ipcRenderer.invoke('amp:listPorts'),

  // File dialogs
  importSaveTxt: () => ipcRenderer.invoke('amp:importSaveTxt'),
  exportSaveTxt: () => ipcRenderer.invoke('amp:exportSaveTxt'),

  // Window controls
  setAlwaysOnTop: (v) => ipcRenderer.invoke('amp:setAlwaysOnTop', v),

  // Protocol constants (cached on first call by the renderer)
  getProtocol: () => ipcRenderer.invoke('amp:getProtocol'),

  // Mock / dev hooks
  isMock: () => ipcRenderer.invoke('amp:isMock'),
  mock: (action, payload) => ipcRenderer.invoke('amp:mock', action, payload),

  // ---- Diagnostics ----
  // Subscribe to live diagnostic entries. Returns an unsubscribe function.
  onDiagEntry: (cb) => {
    const handler = (_e, entry) => cb(entry);
    ipcRenderer.on('diag:entry', handler);
    return () => ipcRenderer.removeListener('diag:entry', handler);
  },
  onDiagCleared: (cb) => {
    const handler = () => cb();
    ipcRenderer.on('diag:cleared', handler);
    return () => ipcRenderer.removeListener('diag:cleared', handler);
  },
  getDiagEntries: () => ipcRenderer.invoke('diag:getAll'),
  clearDiag: () => ipcRenderer.invoke('diag:clear'),
  saveDiag: () => ipcRenderer.invoke('diag:save'),
  // Renderer-originated log entry (UI events).
  logUi: (level, message, detail) => ipcRenderer.invoke('diag:logUi', level, message, detail),
};

contextBridge.exposeInMainWorld('api', api);
// Backward alias for any code expecting `window.amp`
contextBridge.exposeInMainWorld('amp', api);
