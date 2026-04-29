// Persisted settings, backed by electron-store via IPC.
// Loaded once at startup and held in memory; saveConfig() round-trips writes.

const DEFAULTS = {
  comPort: '',
  baudRate: 600,
  windowX: 0,
  windowY: 0,
  mode: 'TCP',
  lanIp: '192.168.0.55',
  tcpPort: 5005,
  udpPort: 5010,
  alwaysOnTop: false,
  bypass: false,
  tempUnit: 'C',
  antenna: 1,
  voltsMode: false,
  koef: 1200,
  voltage: '48',
  cat: 0,
  antennaMap: [1, 1, 1, 1, 1, 1, 1, 1],
  sound: true,
  inputIndicator: false,
  // POC-only display option
  showFahrenheit: false,
  // POC-only: verbose RX logging (logs every received packet to diagnostics)
  verboseRxLogging: false,
};

export const settings = $state({ ...DEFAULTS });
export let loaded = $state({ value: false });

export async function loadSettings() {
  if (!window.api) return;
  try {
    const fromMain = await window.api.getConfig();
    if (fromMain && typeof fromMain === 'object') {
      Object.assign(settings, DEFAULTS, fromMain);
    }
  } catch (e) {
    console.warn('loadSettings failed', e);
  }
  loaded.value = true;
}

export async function saveSettings(patch = {}) {
  Object.assign(settings, patch);
  if (window.api) {
    // Strip Svelte $state proxies — Electron IPC's structured clone can't
    // serialize them, which silently rejects the call and leaves the store
    // un-persisted.
    const payload = $state.snapshot(settings);
    try { await window.api.saveConfig(payload); }
    catch (e) { console.warn('saveSettings failed', e); }
  }
}
