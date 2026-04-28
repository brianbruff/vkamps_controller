// Mirror of device-side state derived from incoming 8-field packets + local toggles.
export const deviceState = $state({
  band: 3,         // 0..7 — 30m default
  antenna: 1,      // 1..4
  bypass: false,
  voltsPlus: false,
  fanFull: false,
  errorCode: 0,    // p8
  status: 0,       // p9
  txActive: false, // derived from p1>0 && !bypass (set in telemetry)
});

export function applyStatePacket(d) {
  deviceState.band = d.p6 ?? deviceState.band;
  deviceState.antenna = d.p7 ?? deviceState.antenna;
  deviceState.bypass = !!d.p11;
  deviceState.errorCode = d.p8 ?? 0;
  deviceState.status = d.p9 ?? 0;
  // p10 is fan in legacy code; we treat it as informational
  if (typeof d.p10 === 'number') deviceState.fanFull = d.p10 === 1;
}
