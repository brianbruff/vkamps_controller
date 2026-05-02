import { writable, derived } from 'svelte/store';

// Raw telemetry values
export const p1 = writable(0);   // output power raw (0–776)
export const p2 = writable(0);   // reflected power raw
export const p3 = writable(0);   // temperature raw
export const p4 = writable(0);   // current raw (tenths)
export const p5 = writable(0);   // voltage raw (tenths)
export const p6 = writable(0);   // band index
export const p7 = writable(0);   // antenna from device
export const p8 = writable(0);   // error code
export const p9 = writable(0);   // device status (0=idle, !0=transmitting)
export const p10 = writable(0);  // fan status (1=100%)
export const p11 = writable(0);  // bypass state (1=bypassed)
export const p12 = writable(0);  // input power raw

// Peak-hold values (decayed by timers in components)
export const p1Peak = writable(0);
export const p2Peak = writable(0);
export const p4Peak = writable(0);
export const p12Peak = writable(0);

// Connection state
export const isConnected = writable(false);

// Derived: is transmitting
export const isTransmitting = derived(p9, $p9 => $p9 !== 0);

// Derived: is bypassed
export const isBypassed = derived(p11, $p11 => $p11 === 1);

// Derived: error info
export const errorCode = derived(p8, $p8 => $p8);
export const hasError = derived(p8, $p8 => $p8 !== 0);

const ERROR_TEXTS = {
  0: 'Status OK',
  1: 'Error Input Power!',
  2: 'Error Power',
  3: 'Error REF!',
  4: 'Error LPF!',
  5: 'Error Current!',
  6: 'Error Voltage!',
  7: 'Error Temperature!',
};
export const errorText = derived(p8, $p8 => ERROR_TEXTS[$p8] || 'Status OK');

// Derived: fan mode
export const fanMode = derived(p10, $p10 => $p10 === 1 ? '100%' : 'Auto');

// Band names (0-indexed: p6 values 0-7)
const BAND_NAMES = ['160', '80', '40', '30', '20', '17-15', '12-10', '6'];
export const bandName = derived(p6, $p6 => BAND_NAMES[$p6] || '');

// Apply incoming TCP packet
export function applyTcpData(data) {
  if (!data) return;
  p3.set(data.p3);
  p5.set(data.p5);
  p6.set(data.p6);
  p7.set(data.p7);
  p8.set(data.p8);
  p9.set(data.p9);
  p10.set(data.p10);
  p11.set(data.p11);
}

// Apply incoming UDP packet
export function applyUdpData(data) {
  if (!data) return;
  p1.set(data.p1);
  p2.set(data.p2);
  p4.set(data.p4);
  p12.set(data.p12);
}

// Reset all telemetry
export function resetTelemetry() {
  [p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12].forEach(s => s.set(0));
  [p1Peak, p2Peak, p4Peak, p12Peak].forEach(s => s.set(0));
}
