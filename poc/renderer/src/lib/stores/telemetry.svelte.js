// Telemetry — latest p1/p2/p4/p12, peak holds, derived watts/eff/swr.
import { settings } from './settings.svelte.js';
import { deviceState } from './deviceState.svelte.js';

export const telemetry = $state({
  // raw from latest 4-field packet
  p1: 0,
  p2: 0,
  p4: 0,
  p12: 0,
  // derived (recomputed via $derived in components, but kept here for peak hold)
  peakP1: 0,
  peakP2: 0,
  peakDecayAt: 0,
});

export function applyMeterPacket(d) {
  telemetry.p1 = d.p1 ?? 0;
  telemetry.p2 = d.p2 ?? 0;
  telemetry.p4 = d.p4 ?? 0;
  telemetry.p12 = d.p12 ?? 0;

  const now = performance.now();
  const peakHoldMs = settings.peakHoldDuration || 2000;
  if (telemetry.p1 >= telemetry.peakP1) {
    telemetry.peakP1 = telemetry.p1;
    telemetry.peakDecayAt = now + peakHoldMs;
  } else if (now > telemetry.peakDecayAt) {
    telemetry.peakP1 = Math.max(telemetry.p1, telemetry.peakP1 * 0.985);
  }
  if (telemetry.p2 >= telemetry.peakP2) {
    telemetry.peakP2 = telemetry.p2;
  } else if (now > telemetry.peakDecayAt) {
    telemetry.peakP2 = Math.max(telemetry.p2, telemetry.peakP2 * 0.985);
  }

  deviceState.txActive = !deviceState.bypass && telemetry.p1 > 5;
}

// ---------- Derived helpers (pure functions) ----------
const DIVISOR = { 600: 952, 1200: 476, 2400: 238 };
const CURRENT_MULT = {
  600:  { false: 66,  true: 102 },
  1200: { false: 132, true: 204 },
  2400: { false: 264, true: 408 },
};

export function watts(rawP1, koef = settings.koef) {
  return Math.floor((rawP1 * rawP1) / (DIVISOR[koef] || 476));
}

export function reflectedW(rawP2, koef = settings.koef) {
  // Reflected uses the same divisor / shape (legacy app maps 0..50 → display)
  return Math.floor((rawP2 * rawP2) / (DIVISOR[koef] || 476));
}

export function inputW(p12, mode = settings.mode) {
  return Math.floor((p12 * p12) / (mode === 'USB' ? 476 : 540));
}

export function currentA(rawP4, koef = settings.koef, inputIndicator = settings.inputIndicator) {
  const mult = CURRENT_MULT[koef]?.[inputIndicator] ?? 132;
  return (rawP4 * mult) / 1000;
}

export function efficiency(p1, p4, p5, koef = settings.koef, inputIndicator = settings.inputIndicator) {
  const d = DIVISOR[koef] || 476;
  const mult = CURRENT_MULT[koef]?.[inputIndicator] ?? 132;
  const voltageV = p5 / 10;
  const cur = (p4 * mult / 100) / 10;
  const inputWatts = voltageV * cur;
  if (inputWatts === 0) return 0;
  const outputW = (p1 * p1) / d;
  return Math.min(Math.floor((outputW * 100) / inputWatts), 99);
}

export function swr(fwdPeak, refPeak) {
  if (fwdPeak === 0 || refPeak === 0 || fwdPeak <= refPeak) return 1.0;
  const v = (fwdPeak + refPeak) / (fwdPeak - refPeak);
  return v < 10 ? Math.round(v * 100) / 100 : 9.99;
}

// Voltage and temp from latest 8-field packet
export const stateMeters = $state({
  p3: 0, // temp raw
  p5: 480, // voltage * 10
});

export function applyStateMeters(d) {
  if (typeof d.p3 === 'number') stateMeters.p3 = d.p3;
  if (typeof d.p5 === 'number') stateMeters.p5 = d.p5;
}
