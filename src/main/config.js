const Store = require('electron-store');
const fs = require('fs');
const path = require('path');

const DEFAULTS = {
  comPort: '',
  baudRate: 600,
  windowX: 0,
  windowY: 0,
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
};

const store = new Store({
  name: 'vkamp-config',
  defaults: DEFAULTS,
});

function getConfig() {
  const cfg = {};
  for (const key of Object.keys(DEFAULTS)) {
    cfg[key] = store.get(key, DEFAULTS[key]);
  }
  return cfg;
}

function saveConfig(cfg) {
  for (const [key, value] of Object.entries(cfg)) {
    if (key in DEFAULTS) {
      store.set(key, value);
    }
  }
}

function getDefault(key) {
  return DEFAULTS[key];
}

// Import original save.txt format
function importSaveTxt(filePath) {
  const lines = fs.readFileSync(filePath, 'utf-8').split(/\r?\n/);
  if (lines.length < 19) return false;

  store.set('comPort', lines[0] || '');
  store.set('baudRate', parseInt(lines[1]) || 600);
  store.set('windowX', parseInt(lines[2]) || 0);
  store.set('windowY', parseInt(lines[3]) || 0);
  store.set('mode', lines[4] === 'LAN' ? 'LAN' : 'USB');
  store.set('lanIp', lines[5] || '192.168.0.55');
  store.set('tcpPort', parseInt(lines[6]) || 5005);
  store.set('udpPort', parseInt(lines[7]) || 5010);
  store.set('alwaysOnTop', lines[8]?.toLowerCase() === 'true');
  store.set('bypass', lines[9]?.toLowerCase() === 'true');
  store.set('tempUnit', lines[10] === 'F' ? 'F' : 'C');
  store.set('antenna', parseInt(lines[11]) || 1);
  store.set('voltsMode', lines[12]?.toLowerCase() === 'true');
  store.set('koef', parseInt(lines[13]) || 1200);
  store.set('voltage', lines[14] || '48');
  store.set('cat', parseInt(lines[15]) || 0);
  store.set('antennaMap', (lines[16] || '1,1,1,1,1,1,1,1').split(',').map(Number));
  store.set('sound', lines[17] !== 'False');
  store.set('inputIndicator', lines[18] === 'True');

  return true;
}

// Export to original save.txt format
function exportSaveTxt(filePath) {
  const cfg = getConfig();
  const lines = [
    cfg.comPort,
    String(cfg.baudRate),
    String(cfg.windowX),
    String(cfg.windowY),
    cfg.mode,
    cfg.lanIp,
    String(cfg.tcpPort),
    String(cfg.udpPort),
    cfg.alwaysOnTop ? 'True' : 'False',
    cfg.bypass ? 'True' : 'False',
    cfg.tempUnit,
    String(cfg.antenna),
    cfg.voltsMode ? 'True' : 'False',
    String(cfg.koef),
    cfg.voltage,
    String(cfg.cat),
    cfg.antennaMap.join(','),
    cfg.sound ? 'True' : 'False',
    cfg.inputIndicator ? 'True' : 'False',
  ];
  fs.writeFileSync(filePath, lines.join('\n') + '\n', 'utf-8');
}

module.exports = {
  store,
  getConfig,
  saveConfig,
  getDefault,
  importSaveTxt,
  exportSaveTxt,
  DEFAULTS,
};
