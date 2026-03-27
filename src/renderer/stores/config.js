import { writable } from 'svelte/store';

export const appConfig = writable({
  comPort: '',
  baudRate: 600,
  mode: 'USB',
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
});

export async function loadConfig() {
  if (window.amp) {
    const cfg = await window.amp.getConfig();
    appConfig.set(cfg);
  }
}

export async function saveAppConfig(cfg) {
  if (window.amp) {
    await window.amp.saveConfig(cfg);
    appConfig.set(cfg);
  }
}
