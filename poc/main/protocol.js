// Command constants — all sent as ASCII strings
const CMD = {
  INIT: '11',
  DISCONNECT: '99',
  BYPASS_ON: '21',
  BYPASS_OFF: '22',
  RESET: '23',
  ANTENNA_1: '31',
  ANTENNA_2: '32',
  ANTENNA_3: '33',
  VOLTS_NORMAL: '41',
  VOLTS_PLUS: '42',
  SLEEP: '44',
  FAN_100: '45',
  FAN_AUTO: '46',
  VOLTAGE_48: '51',
  VOLTAGE_50: '52',
  VOLTAGE_53_5: '53',
  VOLTAGE_58_3: '54',
  CAT_RF: '61',
  CAT_ICOM: '62',
  CAT_YAESU: '63',
  CAT_KENWOOD: '64',
  CAT_ANAN: '65',
  CAT_MANUAL: '66',
  BAND_160: '71',
  BAND_80: '72',
  BAND_40: '73',
  BAND_30: '74',
  BAND_20: '75',
  BAND_17_15: '76',
  BAND_12_10: '77',
  BAND_6: '78',
};

const ANTENNA_CMDS = [CMD.ANTENNA_1, CMD.ANTENNA_2, CMD.ANTENNA_3];
const BAND_CMDS = [CMD.BAND_160, CMD.BAND_80, CMD.BAND_40, CMD.BAND_30, CMD.BAND_20, CMD.BAND_17_15, CMD.BAND_12_10, CMD.BAND_6];
const CAT_CMDS = [CMD.CAT_RF, CMD.CAT_ICOM, CMD.CAT_YAESU, CMD.CAT_KENWOOD, CMD.CAT_ANAN, CMD.CAT_MANUAL];
const VOLTAGE_CMDS = { '48': CMD.VOLTAGE_48, '50': CMD.VOLTAGE_50, '53.5': CMD.VOLTAGE_53_5, '58.3': CMD.VOLTAGE_58_3 };

const BANDS = ['160', '80', '40', '30', '20', '17-15', '12-10', '6'];

const ERROR_CODES = {
  0: { text: 'Status OK', color: 'green' },
  1: { text: 'Error Input Power!', color: 'red' },
  2: { text: 'Error Power', color: 'red' },
  3: { text: 'Error REF!', color: 'red' },
  4: { text: 'Error LPF!', color: 'red' },
  5: { text: 'Error Current!', color: 'red' },
  6: { text: 'Error Voltage!', color: 'red' },
  7: { text: 'Error Temperature!', color: 'red' },
};

// Scale labels per koef for gauges
const SCALE_LABELS = {
  output: {
    600:  [25, 100, 200, 300, 400, 500, 600],
    1200: [50, 200, 400, 600, 800, 1000, 1200],
    2400: [100, 400, 800, 1200, 1600, 2000, 2400],
  },
  reflected: {
    600:  [2, 10, 25, 50],
    1200: [5, 20, 50, 100],
    2400: [10, 40, 100, 200],
  },
  current: {
    600:  [5, 10, 15, 20],
    1200: [10, 20, 30, 40],
    2400: [20, 40, 60, 80],
  },
};

// Parse a TCP 8-field packet: p3,p5,p6,p7,p8,p9,p10,p11
function parseTcpPacket(str) {
  const parts = str.trim().split(',').map(Number);
  if (parts.length < 8) return null;
  return {
    p3: parts[0],   // temperature raw
    p5: parts[1],   // voltage (tenths)
    p6: parts[2] - 1,   // band index (device sends 1-8, convert to 0-7)
    p7: parts[3],   // antenna (1/2/3)
    p8: parts[4],   // error code
    p9: parts[5],   // device status (0=OK)
    p10: parts[6],  // fan status (1=100%)
    p11: parts[7],  // bypass state (1=bypassed)
  };
}

// Parse a UDP 4-field packet: p1,p2,p4,p12
function parseUdpPacket(str) {
  const parts = str.trim().split(',').map(Number);
  if (parts.length < 4) return null;
  return {
    p1: parts[0],   // output power (0–776)
    p2: parts[1],   // reflected power
    p4: parts[2],   // current (tenths)
    p12: parts[3],  // input power raw
  };
}

// Parse serial packet — auto-detect 4 vs 8 fields
function parseSerialPacket(str) {
  const parts = str.trim().split(',');
  if (parts.length >= 8) return { type: 'tcp', data: parseTcpPacket(str) };
  if (parts.length >= 4) return { type: 'udp', data: parseUdpPacket(str) };
  return null;
}

// Convert raw output power to watts
function rawToWatts(rawP1, koef) {
  const divisor = { 600: 952, 1200: 476, 2400: 238 };
  return Math.floor((rawP1 * rawP1) / (divisor[koef] || 476));
}

// Convert raw current to display amps (depends on koef + inputIndicator)
function rawToCurrentAmps(rawP4, koef, inputIndicator) {
  const multipliers = {
    600:  { false: 66,  true: 102 },
    1200: { false: 132, true: 204 },
    2400: { false: 264, true: 408 },
  };
  const mult = multipliers[koef]?.[inputIndicator] ?? 132;
  return (rawP4 * mult) / 1000;
}

// Calculate efficiency %
function calcEfficiency(p1, p4, p5, koef, inputIndicator) {
  const divisor = { 600: 952, 1200: 476, 2400: 238 };
  const multipliers = {
    600:  { false: 66,  true: 102 },
    1200: { false: 132, true: 204 },
    2400: { false: 264, true: 408 },
  };
  const d = divisor[koef] || 476;
  const mult = multipliers[koef]?.[inputIndicator] ?? 132;
  const voltageV = p5 / 10;
  const currentA = (p4 * mult / 100) / 10;
  const inputW = voltageV * currentA;
  if (inputW === 0) return 0;
  const outputW = (p1 * p1) / d;
  const eff = (outputW * 100) / inputW;
  return Math.min(Math.floor(eff), 99);
}

// Calculate input power display
function calcInputPower(p12, isLan) {
  return Math.floor((p12 * p12) / (isLan ? 540 : 476));
}

// SWR calculation from peak-hold values
function calcSWR(fwdPeak, refPeak) {
  if (fwdPeak === 0 || refPeak === 0 || fwdPeak <= refPeak) return 1.0;
  const swr = (fwdPeak + refPeak) / (fwdPeak - refPeak);
  return swr < 10 ? Math.round(swr * 100) / 100 : 9.99;
}

// Map value from one range to another (Arduino-style)
function mapRange(x, inMin, inMax, outMin, outMax) {
  return Math.floor((x - inMin) * (outMax - outMin) / (inMax - inMin) + outMin);
}

module.exports = {
  CMD,
  ANTENNA_CMDS,
  BAND_CMDS,
  CAT_CMDS,
  VOLTAGE_CMDS,
  BANDS,
  ERROR_CODES,
  SCALE_LABELS,
  parseTcpPacket,
  parseUdpPacket,
  parseSerialPacket,
  rawToWatts,
  rawToCurrentAmps,
  calcEfficiency,
  calcInputPower,
  calcSWR,
  mapRange,
};
