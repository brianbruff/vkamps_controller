const { contextBridge, ipcRenderer } = require('electron');

contextBridge.exposeInMainWorld('amp', {
  // Connection
  connect: () => ipcRenderer.invoke('amp:connect'),
  disconnect: () => ipcRenderer.invoke('amp:disconnect'),
  sendCommand: (cmd) => ipcRenderer.invoke('amp:sendCommand', cmd),

  // Real-time data from transport
  onData: (callback) => {
    const handler = (_event, data) => callback(data);
    ipcRenderer.on('amp:data', handler);
    return () => ipcRenderer.removeListener('amp:data', handler);
  },

  onConnectionChange: (callback) => {
    const handler = (_event, connected) => callback(connected);
    ipcRenderer.on('amp:connection', handler);
    return () => ipcRenderer.removeListener('amp:connection', handler);
  },

  onError: (callback) => {
    const handler = (_event, msg) => callback(msg);
    ipcRenderer.on('amp:error', handler);
    return () => ipcRenderer.removeListener('amp:error', handler);
  },

  // Config
  getConfig: () => ipcRenderer.invoke('amp:getConfig'),
  saveConfig: (cfg) => ipcRenderer.invoke('amp:saveConfig', cfg),

  // Serial ports
  listPorts: () => ipcRenderer.invoke('amp:listPorts'),

  // Import/Export
  importSaveTxt: () => ipcRenderer.invoke('amp:importSaveTxt'),
  exportSaveTxt: () => ipcRenderer.invoke('amp:exportSaveTxt'),

  // Window controls
  setAlwaysOnTop: (value) => ipcRenderer.invoke('amp:setAlwaysOnTop', value),

  // Protocol constants
  getProtocol: () => ipcRenderer.invoke('amp:getProtocol'),
});
