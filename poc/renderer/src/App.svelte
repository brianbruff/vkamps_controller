<script>
  import { onMount } from 'svelte';

  import HeaderBar from './lib/chrome/HeaderBar.svelte';
  import OutputMeter from './lib/meters/OutputMeter.svelte';
  import MeterBar from './lib/meters/MeterBar.svelte';
  import StatusPill from './lib/status/StatusPill.svelte';
  import AntennaTile from './lib/cards/AntennaTile.svelte';
  import BandTile from './lib/cards/BandTile.svelte';
  import SwrTile from './lib/cards/SwrTile.svelte';
  import VoltTile from './lib/cards/VoltTile.svelte';
  import TempTile from './lib/cards/TempTile.svelte';
  import ControlButton from './lib/controls/ControlButton.svelte';
  import OperateToggle from './lib/controls/OperateToggle.svelte';
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
  const BAND_FREQ = ['1.84 MHz', '3.65 MHz', '7.10 MHz', '10.10 MHz', '14.20 MHz', '18.10 MHz', '24.94 MHz', '50.10 MHz'];
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
        if (sleeping) sleeping = false;
      }
    });

    unsubConn = window.api.onConnection((open) => {
      setStatus(open ? 'open' : 'closed');
    });

    unsubErr = window.api.onError((msg) => {
      setError(msg || 'Unknown transport error');
    });

    setStatus('closed');

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
    deviceState.bypass = next;
  }
  function onSetup() { settingsOpen = true; }
  function onDiagnostics() { diagOpen = true; }
  function onNeedSetup() { settingsOpen = true; }

  function onOperateChange(next) {
    // operate=true means NOT bypass (RF active path)
    if (next === !deviceState.bypass) return;
    onBypass();
  }

  function onErrorPill() {
    if (deviceState.errorCode) onReset();
  }
  function onFanPill() {
    const next = !deviceState.fanFull;
    send(next ? '45' : '46');
    deviceState.fanFull = next;
  }

  function onAntennaSelect(n) {
    send(String(30 + n));
    deviceState.antenna = n;
  }

  const catManual = $derived(settings.cat === 5);
  function onBandSelect(i) {
    if (!catManual) return;
    send(String(71 + i));
    deviceState.band = i;
  }
  function onVoltsToggle() {
    const next = !deviceState.voltsPlus;
    send(next ? '42' : '41');
    deviceState.voltsPlus = next;
  }

  // ---- Status chip strings ----
  const operating = $derived(!deviceState.bypass);
  const errorText = $derived(deviceState.errorCode ? (ERRORS[deviceState.errorCode] || 'ERROR') : 'Status OK');
  const errorTone = $derived(deviceState.errorCode ? 'error' : 'ok');
  const onAirText = $derived(
    deviceState.bypass ? 'Bypass' :
    deviceState.txActive ? 'On Air' : 'Standby'
  );
  const onAirTone = $derived(
    deviceState.bypass ? 'bypass' :
    deviceState.txActive ? 'tx' : 'idle'
  );
  const fanText = $derived(deviceState.fanFull ? 'Fan 100%' : 'Fan Auto');
  const fanTone = $derived(deviceState.fanFull ? 'fan' : 'idle');
</script>

<div class="app-shell">
  <HeaderBar
    onneedsetup={onNeedSetup}
    ondiagnostics={onDiagnostics} />

  <main class="main">
    <div class="panel">
      <!-- Hero output meter -->
      <OutputMeter
        value={outW}
        max={outputScale.max}
        ticks={outputScale.ticks}
        peakHold={peakOutW}
        avg={peakOutW ? Math.round(peakOutW * 0.96) : 0}
        label="Output Power" />

      <!-- Sub-meters row -->
      <div class="sub-meters">
        <MeterBar
          label="Reflected"
          icon="arrowDown"
          sub={`RFL · max ${reflectedMax} W`}
          value={refW}
          max={reflectedMax}
          unit="W"
          peakHold={peakRefW}
          ticks={[Math.round(reflectedMax / 4), Math.round(reflectedMax / 2), Math.round(reflectedMax * 3 / 4), reflectedMax]} />

        <MeterBar
          label="Input"
          icon="arrowUp"
          sub={`DRV · max ${INPUT_MAX} W`}
          value={inW}
          max={INPUT_MAX}
          unit="W"
          ticks={[25, 50, 75, 100]} />

        <MeterBar
          label="Current"
          icon="bolt"
          spark={operating ? 1 : 0.05}
          value={curA}
          max={currentMax}
          unit="A"
          decimals={1}
          ticks={[Math.round(currentMax / 4), Math.round(currentMax / 2), Math.round(currentMax * 3 / 4), currentMax]} />
      </div>

      <!-- Status chip bar -->
      <div class="statusbar">
        <StatusPill
          label={onAirText}
          tone={onAirTone}
          active={deviceState.bypass || deviceState.txActive}
          onclick={onBypass} />
        <StatusPill
          label={errorText}
          tone={errorTone}
          active={deviceState.errorCode > 0}
          onclick={deviceState.errorCode ? onErrorPill : null} />
        <StatusPill
          label={fanText}
          tone={fanTone}
          active={deviceState.fanFull}
          onclick={onFanPill} />
        <div class="stat-rest">
          <div class="stat"><span class="l">Efficiency</span><span class="v">{eff}%</span></div>
          <div class="stat"><span class="l">SWR</span><span class="v">{swrVal.toFixed(2)}</span></div>
        </div>
      </div>

      <!-- Secondary tile row -->
      <div class="secondary">
        <AntennaTile
          active={deviceState.antenna}
          ports={[1, 2, 3]}
          showLabels={true}
          onselect={onAntennaSelect} />

        <BandTile
          bands={BANDS}
          activeIndex={deviceState.band}
          freq={BAND_FREQ[deviceState.band] || ''}
          enabled={catManual}
          onselect={onBandSelect} />

        <SwrTile value={swrVal} />

        <VoltTile
          value={voltage}
          plus={deviceState.voltsPlus}
          ontoggle={onVoltsToggle} />

        <TempTile
          value={settings.showFahrenheit ? tempF : tempC}
          unit={settings.showFahrenheit ? '°F' : '°C'}
          warnAt={settings.showFahrenheit ? 130 : 55}
          dangerAt={settings.showFahrenheit ? 165 : 75}
          max={settings.showFahrenheit ? 250 : 120} />
      </div>

      <!-- Controls -->
      <div class="controls">
        <OperateToggle operate={operating} onchange={onOperateChange} />
        <ControlButton icon="bypass" label="Bypass" kind="toggle" active={deviceState.bypass} onclick={onBypass} />
        <ControlButton icon="reset"  label="Reset"  onclick={onReset} />
        <ControlButton icon="moon"   label={sleeping ? 'Asleep' : 'Sleep'} active={sleeping} onclick={onSleep} kind="momentary" tone="warn" />
        <ControlButton icon="setup"  label="Setup"  kind="modal" onclick={onSetup} />
      </div>
    </div>
  </main>

  <SettingsModal bind:open={settingsOpen} onclose={() => settingsOpen = false} />
  <DiagnosticsView bind:open={diagOpen} onclose={() => diagOpen = false} />
</div>

<style>
  .app-shell {
    display: grid;
    grid-template-rows: 64px 1fr;
    height: 100vh;
    background: var(--bg);
  }

  .main {
    padding: 24px 28px;
    overflow: auto;
    display: flex;
    flex-direction: column;
    min-height: 0;
  }

  .panel {
    background: var(--paper);
    border: 1px solid var(--hairline);
    border-radius: 18px;
    box-shadow: var(--shadow-panel);
    padding: 20px 22px 24px;
    display: grid;
    /* Hero, sub-meters, and secondary tiles all absorb extra vertical space;
       status bar and controls stay at their natural height. */
    grid-template-rows:
      minmax(180px, 1.4fr)   /* hero output meter */
      minmax(160px, 1fr)     /* reflected / input / current */
      auto                    /* status bar */
      minmax(180px, 1.3fr)   /* antenna / band / swr / volts / temp */
      auto;                   /* controls */
    gap: 16px;
    width: 100%;
    margin: 0 auto;
    flex: 1 1 auto;
    min-height: 0;
  }

  .sub-meters {
    display: grid;
    grid-template-columns: repeat(3, minmax(0, 1fr));
    gap: 16px;
  }

  .statusbar {
    display: flex;
    gap: 12px;
    align-items: stretch;
    padding: 12px;
    background: var(--paper-2);
    border: 1px solid var(--hairline);
    border-radius: 14px;
    height: 52px;
  }
  .stat-rest {
    margin-left: auto;
    display: flex;
    align-items: center;
    gap: 18px;
    padding-right: 8px;
  }
  .stat {
    display: flex;
    align-items: baseline;
    gap: 8px;
    font-size: 13px;
  }
  .stat .l {
    color: var(--ink-3);
    font-weight: 500;
  }
  .stat .v {
    color: var(--ink);
    font-weight: 600;
    font-family: var(--font-num);
  }

  .secondary {
    display: grid;
    grid-template-columns: 1.4fr 1.6fr 1fr 1fr 1fr;
    gap: 16px;
  }

  .controls {
    display: grid;
    grid-template-columns: 2fr repeat(4, 1fr);
    gap: 12px;
  }

  /* Narrower laptops — keep all 5 secondary tiles in one row but tighter. */
  @media (max-width: 1280px) {
    .secondary {
      grid-template-columns: 1.3fr 1.5fr repeat(3, minmax(0, 1fr));
      gap: 12px;
    }
  }

  /* Tablet-ish — drop secondary to 3 cols, keep controls in one row. */
  @media (max-width: 1080px) {
    .secondary {
      grid-template-columns: repeat(3, minmax(0, 1fr));
    }
  }

  /* Cramped windows — sub-meters wrap, controls go to 2 rows. */
  @media (max-width: 820px) {
    .sub-meters {
      grid-template-columns: repeat(2, minmax(0, 1fr));
    }
    .secondary {
      grid-template-columns: repeat(2, minmax(0, 1fr));
    }
    .controls {
      grid-template-columns: repeat(2, minmax(0, 1fr));
    }
    .stat-rest { gap: 12px; }
  }

  /* Very narrow — single column stack. */
  @media (max-width: 560px) {
    .main { padding: 14px 14px; }
    .panel { padding: 14px 14px 16px; }
    .sub-meters,
    .secondary,
    .controls {
      grid-template-columns: 1fr;
    }
    .statusbar {
      flex-wrap: wrap;
      height: auto;
      padding: 10px;
    }
    .stat-rest {
      width: 100%;
      justify-content: space-between;
      padding-right: 0;
    }
  }
</style>
