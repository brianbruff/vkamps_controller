<script>
  import { onMount } from 'svelte';

  import TesterBanner from './lib/chrome/TesterBanner.svelte';
  import HeaderBar from './lib/chrome/HeaderBar.svelte';
  import OutputMeter from './lib/meters/OutputMeter.svelte';
  import MeterBar from './lib/meters/MeterBar.svelte';
  import StatusPill from './lib/status/StatusPill.svelte';
  import StatCard from './lib/cards/StatCard.svelte';
  import ControlButton from './lib/controls/ControlButton.svelte';
  import SettingsModal from './lib/settings/SettingsModal.svelte';
  import DiagnosticsView from './lib/diagnostics/DiagnosticsView.svelte';

  import { settings, loadSettings } from './lib/stores/settings.svelte.js';
  import { transportState, setStatus, setError } from './lib/stores/transport.svelte.js';
  import { deviceState, applyStatePacket } from './lib/stores/deviceState.svelte.js';
  import {
    telemetry, applyMeterPacket, applyStateMeters, stateMeters,
    watts, reflectedW, inputW, currentA, efficiency, swr,
  } from './lib/stores/telemetry.svelte.js';

  // Local UI state
  let settingsOpen = $state(false);
  let diagOpen = $state(false);
  let isMockBuild = $state(false);
  let sleeping = $state(false);
  let sleepResetTimer = null;

  // ---- Scale config ----
  const OUTPUT_SCALES = {
    600:  { max: 600,  ticks: [100, 200, 300, 400, 500, 600] },
    1200: { max: 1200, ticks: [200, 400, 600, 800, 1000, 1200] },
    2400: { max: 2400, ticks: [400, 800, 1200, 1600, 2000, 2400] },
  };
  const REFLECTED_SCALES = { 600: 50, 1200: 100, 2400: 200 };
  const CURRENT_SCALES = { 600: 20, 1200: 40, 2400: 80 };
  const INPUT_MAX = 100;

  const BANDS = ['160m', '80m', '40m', '30m', '20m', '17–15m', '12–10m', '6m'];
  const ERRORS = {
    0: 'STATUS OK',
    1: 'ERR INPUT',
    2: 'ERR POWER',
    3: 'ERR REF',
    4: 'ERR LPF',
    5: 'ERR CURRENT',
    6: 'ERR VOLTAGE',
    7: 'ERR TEMP',
  };

  const outputScale = $derived(OUTPUT_SCALES[settings.koef] || OUTPUT_SCALES[1200]);
  const reflectedMax = $derived(REFLECTED_SCALES[settings.koef] || 100);
  const currentMax = $derived(CURRENT_SCALES[settings.koef] || 40);

  // Derived display values
  const outW = $derived(watts(telemetry.p1));
  const refW = $derived(reflectedW(telemetry.p2));
  const inW  = $derived(inputW(telemetry.p12));
  const curA = $derived(currentA(telemetry.p4));
  const peakOutW = $derived(watts(telemetry.peakP1));
  const peakRefW = $derived(reflectedW(telemetry.peakP2));
  const eff = $derived(efficiency(telemetry.p1, telemetry.p4, stateMeters.p5));
  const swrVal = $derived(swr(telemetry.peakP1, telemetry.peakP2));
  const tempC = $derived(stateMeters.p3);
  const tempF = $derived(Math.round(tempC * 18 / 10 + 32));
  const voltage = $derived(stateMeters.p5 / 10);

  // ---- Wire up IPC events ----
  let unsubPacket;
  let unsubConn;
  let unsubErr;

  onMount(async () => {
    if (!window.api) {
      console.warn('window.api missing — preload not loaded');
      return;
    }

    await loadSettings();
    isMockBuild = await window.api.isMock();
    transportState.isMock = isMockBuild;

    unsubPacket = window.api.onPacket((pkt) => {
      if (!pkt) return;
      if (pkt.type === 'udp' && pkt.data) {
        applyMeterPacket(pkt.data);
      } else if (pkt.type === 'tcp' && pkt.data) {
        applyStatePacket(pkt.data);
        applyStateMeters(pkt.data);
        // Any inbound state packet clears the local "sleeping" latch
        if (sleeping) sleeping = false;
      }
    });

    unsubConn = window.api.onConnection((open) => {
      setStatus(open ? 'open' : 'closed');
    });

    unsubErr = window.api.onError((msg) => {
      setError(msg || 'Unknown transport error');
    });

    // No auto-connect — the user clicks Connect in the header. This makes the
    // POC ship-safe: a misconfigured tester won't get a confusing failure
    // dialog before they've even seen the UI.
    setStatus('closed');

    // Apply always-on-top from settings
    if (settings.alwaysOnTop) {
      try { window.api.setAlwaysOnTop(true); } catch {}
    }

    return () => {
      unsubPacket?.();
      unsubConn?.();
      unsubErr?.();
    };
  });

  // ---- Command helpers ----
  function send(cmd) { if (window.api) window.api.send(String(cmd)); }

  // Footer buttons
  function onReset() {
    send('23');
    sleeping = false;
  }
  function onSleep() {
    send('44');
    sleeping = true;
    clearTimeout(sleepResetTimer);
    sleepResetTimer = setTimeout(() => { sleeping = false; }, 6000);
  }
  function onBypass() {
    const next = !deviceState.bypass;
    send(next ? '21' : '22');
    deviceState.bypass = next; // optimistic
  }
  function onSetup() { settingsOpen = true; }
  function onDiagnostics() { diagOpen = true; }
  function onNeedSetup() { settingsOpen = true; }

  // Status-pill click handlers
  function onTxPill() {
    // Mirror of footer Bypass for parity per DESIGN.md §6
    onBypass();
  }
  function onFanPill() {
    const next = !deviceState.fanFull;
    send(next ? '45' : '46');
    deviceState.fanFull = next;
  }
  function onErrorPill() {
    if (deviceState.errorCode) onReset();
  }

  // Stat-card click handlers
  function onAntennaCycle() {
    const max = 4;
    const next = (deviceState.antenna % max) + 1;
    send(String(30 + next));
    deviceState.antenna = next;
  }

  const catManual = $derived(settings.cat === 5);
  function onBandCycle() {
    if (!catManual) return;
    const next = (deviceState.band + 1) % 8;
    send(String(71 + next));
    deviceState.band = next;
  }
  function onVoltsToggle() {
    const next = !deviceState.voltsPlus;
    send(next ? '42' : '41');
    deviceState.voltsPlus = next;
  }

  // ---- Pill texts ----
  const errorText = $derived(deviceState.errorCode ? (ERRORS[deviceState.errorCode] || 'ERROR') : 'STATUS OK');
  const errorTone = $derived(deviceState.errorCode ? 'error' : 'ok');
  const txPillTone = $derived(deviceState.bypass ? 'bypass' : (deviceState.txActive ? 'tx' : 'idle'));
  const txPillLabel = $derived(deviceState.bypass ? 'BYPASS' : (deviceState.txActive ? 'ON AIR' : 'STANDBY'));
  const fanPillLabel = $derived(deviceState.fanFull ? 'FAN 100%' : 'FAN AUTO');
</script>

<div class="app-shell">
  <TesterBanner />

  <HeaderBar
    isMock={isMockBuild}
    onneedsetup={onNeedSetup}
    ondiagnostics={onDiagnostics} />

  <main class="main">
    <!-- Output meter (full width) -->
    <OutputMeter
      value={outW}
      max={outputScale.max}
      ticks={outputScale.ticks}
      peakHold={peakOutW}
      label="OUTPUT POWER" />

    <!-- Sub-meters row -->
    <div class="sub-meters">
      <MeterBar
        label="REFLECTED"
        value={refW}
        max={reflectedMax}
        unit="W"
        peakHold={peakRefW}
        ticks={[Math.round(reflectedMax/4), Math.round(reflectedMax/2), Math.round(reflectedMax*3/4), reflectedMax]} />

      <MeterBar
        label="INPUT"
        value={inW}
        max={INPUT_MAX}
        unit="W"
        ticks={[25, 50, 75, 100]} />

      <MeterBar
        label="CURRENT"
        value={curA}
        max={currentMax}
        unit="A"
        ticks={[Math.round(currentMax/4), Math.round(currentMax/2), Math.round(currentMax*3/4), currentMax]} />
    </div>

    <!-- Status pills -->
    <div class="status-pills">
      <StatusPill
        label={errorText}
        tone={errorTone}
        active={deviceState.errorCode > 0}
        onclick={deviceState.errorCode ? onErrorPill : null} />
      <StatusPill
        label={txPillLabel}
        tone={txPillTone}
        active={deviceState.bypass || deviceState.txActive}
        onclick={onTxPill} />
      <StatusPill
        label={fanPillLabel}
        tone="fan"
        active={deviceState.fanFull}
        onclick={onFanPill} />
    </div>

    <!-- Stat cards -->
    <div class="stat-cards">
      <StatCard
        icon="antenna"
        value={`ANT ${deviceState.antenna}`}
        label="Antenna"
        mode="cycle"
        onactivate={onAntennaCycle} />

      <StatCard
        icon=""
        value={BANDS[deviceState.band] || '—'}
        label="Band"
        mode={catManual ? 'cycle' : 'readonly'}
        enabled={catManual}
        onactivate={onBandCycle} />

      <StatCard
        value={swrVal.toFixed(2)}
        label="SWR"
        mode="readonly" />

      <StatCard
        value={`${eff}%`}
        label="Efficiency"
        mode="readonly" />

      <StatCard
        icon="bolt"
        value={voltage.toFixed(1)}
        unit={deviceState.voltsPlus ? 'V+' : 'V'}
        label="Volts"
        mode="toggle"
        active={deviceState.voltsPlus}
        onactivate={onVoltsToggle} />

      <StatCard
        icon="thermo"
        value={settings.showFahrenheit ? tempF : tempC}
        unit={settings.showFahrenheit ? '°F' : '°C'}
        label="Temp"
        mode="readonly" />
    </div>

    <!-- Footer controls -->
    <div class="footer-controls">
      <ControlButton icon="reset" label="Reset" onclick={onReset} />
      <ControlButton icon="sleep" label={sleeping ? 'Asleep' : 'Sleep'} active={sleeping} onclick={onSleep} kind="momentary" tone="warn" />
      <ControlButton icon="bypass" label="ByPass" kind="toggle" active={deviceState.bypass} onclick={onBypass} />
      <ControlButton icon="setup" label="Setup" kind="modal" onclick={onSetup} />
    </div>
  </main>

  <SettingsModal bind:open={settingsOpen} onclose={() => settingsOpen = false} />
  <DiagnosticsView bind:open={diagOpen} onclose={() => diagOpen = false} />
</div>

<style>
  .app-shell {
    display: grid;
    grid-template-rows: auto 56px 1fr;
    height: 100vh;
    background: var(--color-bg);
  }

  .main {
    display: grid;
    grid-template-rows:
      minmax(120px, 1.4fr)
      minmax(110px, 1fr)
      44px
      72px
      48px;
    gap: 12px;
    padding: 12px 16px;
    background: var(--color-bg);
    overflow: hidden;
  }

  .sub-meters {
    display: grid;
    grid-template-columns: 1fr 1fr 1fr;
    gap: 12px;
  }
  .stat-cards {
    display: grid;
    grid-template-columns: repeat(6, 1fr);
    gap: 8px;
  }
  .status-pills {
    display: flex;
    gap: 12px;
    align-items: stretch;
  }
  .footer-controls {
    display: grid;
    grid-template-columns: repeat(4, 1fr);
    gap: 8px;
    align-items: stretch;
  }
</style>
