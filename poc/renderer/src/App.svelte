<script>
  import { onMount } from 'svelte';

  import HeaderBar from './lib/chrome/HeaderBar.svelte';
  import OutputMeter from './lib/meters/OutputMeter.svelte';
  import MeterBar from './lib/meters/MeterBar.svelte';
  import StatusPill from './lib/status/StatusPill.svelte';
  import AntennaTile from './lib/cards/AntennaTile.svelte';
  import BandTile from './lib/cards/BandTile.svelte';
  import SwrTile from './lib/cards/SwrTile.svelte';
  import PowerThermalTile from './lib/cards/PowerThermalTile.svelte';
  import CompactPanel from './lib/cards/CompactPanel.svelte';
  import ControlButton from './lib/controls/ControlButton.svelte';
  import OperateToggle from './lib/controls/OperateToggle.svelte';
  import SettingsModal from './lib/settings/SettingsModal.svelte';
  import DiagnosticsView from './lib/diagnostics/DiagnosticsView.svelte';

  import { settings, loadSettings, saveSettings } from './lib/stores/settings.svelte.js';
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

  // Display buttons (7) mapped to firmware band indices (0..7).
  // 60-40 share one relay → firmware idx 2. 30-20 share one relay → firmware idx 3
  // (we also accept idx 4 = legacy 20m reports and highlight the same button).
  // cmdIdx is the firmware index sent on click; matchIdx are the firmware indices
  // that highlight this button when reported by the device.
  const BAND_BUTTONS = [
    { label: '160',   freq: '1.84 MHz',  cmdIdx: 0, matchIdx: [0] },
    { label: '80',    freq: '3.65 MHz',  cmdIdx: 1, matchIdx: [1] },
    { label: '60-40', freq: '7.10 MHz',  cmdIdx: 2, matchIdx: [2] },
    { label: '30-20', freq: '10.10 MHz', cmdIdx: 3, matchIdx: [3, 4] },
    { label: '17-15', freq: '18.10 MHz', cmdIdx: 5, matchIdx: [5] },
    { label: '12-10', freq: '24.94 MHz', cmdIdx: 6, matchIdx: [6] },
    { label: '6',     freq: '50.10 MHz', cmdIdx: 7, matchIdx: [7] },
  ];
  const BAND_LABELS = BAND_BUTTONS.map(b => b.label);
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

  // ---- Canvas scaling ----
  // The whole UI is laid out in a fixed 1280×768 design canvas (5:3, matches
  // the Nextion 7" target). On bigger viewports we scale the canvas up via
  // `transform: scale()`; on smaller ones it scales down. Aspect mismatches
  // letterbox — we never reflow except for portrait phones.
  const DESIGN_W = 1280;
  const DESIGN_H = 768;
  function updateCanvasScale() {
    const scale = Math.min(window.innerWidth / DESIGN_W, window.innerHeight / DESIGN_H);
    const tx = (window.innerWidth  - DESIGN_W * scale) / 2;
    const ty = (window.innerHeight - DESIGN_H * scale) / 2;
    const root = document.documentElement.style;
    root.setProperty('--canvas-scale', String(scale));
    root.setProperty('--canvas-tx', `${tx}px`);
    root.setProperty('--canvas-ty', `${ty}px`);
  }

  // ---- Wire up IPC events ----
  let unsubPacket;
  let unsubConn;
  let unsubErr;

  onMount(async () => {
    updateCanvasScale();
    window.addEventListener('resize', updateCanvasScale);

    if (!window.api) {
      console.warn('window.api missing — preload not loaded');
      return () => window.removeEventListener('resize', updateCanvasScale);
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
      window.removeEventListener('resize', updateCanvasScale);
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
  // Compact view uses one button that cycles through the antenna ports.
  const ANTENNA_PORTS = [1, 2, 3];
  function onAntennaCycle() {
    const i = ANTENNA_PORTS.indexOf(deviceState.antenna || 1);
    const next = ANTENNA_PORTS[(i + 1) % ANTENNA_PORTS.length];
    onAntennaSelect(next);
  }

  function onViewModeChange(next) {
    if (next !== 'full' && next !== 'compact') return;
    if (settings.viewMode === next) return;
    saveSettings({ viewMode: next });
  }

  const catManual = $derived(settings.cat === 5);
  // Active display-button index derived from the firmware band index.
  const activeBandButton = $derived(
    BAND_BUTTONS.findIndex(b => b.matchIdx.includes(deviceState.band))
  );
  function onBandSelect(displayIdx) {
    if (!catManual) return;
    const btn = BAND_BUTTONS[displayIdx];
    if (!btn) return;
    send(String(71 + btn.cmdIdx));
    deviceState.band = btn.cmdIdx;
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

<div class="fit-to-screen">
<div class="app-shell">
  <HeaderBar
    viewMode={settings.viewMode}
    onviewmode={onViewModeChange}
    onneedsetup={onNeedSetup}
    ondiagnostics={onDiagnostics} />

  <main class="main">
    {#if settings.viewMode === 'compact'}
      <div class="panel compact-panel">
        <CompactPanel
          outW={outW}
          outMax={outputScale.max}
          outTicks={outputScale.ticks}
          swr={swrVal}
          bandLabels={BAND_LABELS}
          bandActive={activeBandButton}
          bandFreq={BAND_BUTTONS[activeBandButton]?.freq || ''}
          bandEnabled={catManual}
          onbandselect={onBandSelect}
          antenna={deviceState.antenna || 1}
          antennaPorts={ANTENNA_PORTS}
          onantennacycle={onAntennaCycle}
          fanFull={deviceState.fanFull}
          onfan={onFanPill}
          operate={operating}
          onoperate={onOperateChange}
          bypass={deviceState.bypass}
          onbypass={onBypass}
          txActive={deviceState.txActive} />
      </div>
    {:else}
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
          bands={BAND_LABELS}
          activeIndex={activeBandButton}
          freq={BAND_BUTTONS[activeBandButton]?.freq || ''}
          enabled={catManual}
          onselect={onBandSelect} />

        <SwrTile value={swrVal} />

        <PowerThermalTile
          volts={voltage}
          plus={deviceState.voltsPlus}
          onvolts={onVoltsToggle}
          temp={settings.showFahrenheit ? tempF : tempC}
          tempUnit={settings.showFahrenheit ? '°F' : '°C'}
          tempWarnAt={settings.showFahrenheit ? 130 : 55}
          tempDangerAt={settings.showFahrenheit ? 165 : 75}
          tempMax={settings.showFahrenheit ? 250 : 120} />
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
    {/if}
  </main>

  <SettingsModal bind:open={settingsOpen} onclose={() => settingsOpen = false} />
  <DiagnosticsView bind:open={diagOpen} onclose={() => diagOpen = false} />
</div>
</div>

<style>
  /* Letterbox wrapper — fills viewport, never scrolls. The dark backdrop
     fills any aspect-ratio mismatch around the centered canvas. */
  .fit-to-screen {
    position: fixed;
    inset: 0;
    background: #0a0e1f;
    overflow: hidden;
  }

  /* Fixed 1280×768 design canvas. Positioned at viewport (0,0); JS computes
     the letterbox offsets and scale on resize and exposes them as CSS vars.
     transform-origin: 0 0 keeps the math simple — translate first, then
     scale, with no percentage gymnastics. */
  .app-shell {
    position: absolute;
    top: 0;
    left: 0;
    width: 1280px;
    height: 768px;
    display: grid;
    grid-template-rows: 64px 1fr;
    background: var(--bg);
    transform-origin: 0 0;
    transform:
      translate(var(--canvas-tx, 0px), var(--canvas-ty, 0px))
      scale(var(--canvas-scale, 1));
    border-radius: 6px;
    overflow: hidden;
    box-shadow: 0 20px 60px rgba(0, 0, 0, 0.45);
  }

  .main {
    /* No top padding — the panel butts up against the header so the active
       view-mode tab (white) merges seamlessly with the panel below. */
    padding: 0 18px 16px;
    display: flex;
    flex-direction: column;
    min-height: 0;
  }

  /* Panel rows are sized to fill exactly the available height
     (768 - 64 header - 32 main pad - 28 panel pad = 644px content).
     Hero 200 + Sub 130 + Status 44 + Secondary 200 + Controls 52
     + 4 × 12px gap = 674. We give the secondary row 1fr so it
     absorbs any rounding slack without overflow. */
  .panel {
    background: var(--paper);
    border: 1px solid var(--hairline);
    border-top: none;             /* the tabs above own this edge */
    border-radius: 0 0 14px 14px; /* rounded only at the bottom */
    box-shadow: var(--shadow-panel);
    padding: 14px 14px;
    display: grid;
    grid-template-rows:
      200px   /* hero output meter */
      130px   /* reflected / input / current */
      44px    /* status bar */
      1fr     /* antenna · band · swr · power+thermal */
      52px;   /* controls */
    gap: 12px;
    flex: 1 1 auto;
    min-height: 0;
    overflow: hidden;
  }

  /* Compact view drives its own internal grid — opt out of the 5-row
     panel template and let CompactPanel fill the panel box. */
  .panel.compact-panel {
    display: flex;
    flex-direction: column;
    padding: 16px;
  }
  .panel.compact-panel > :global(*) {
    flex: 1 1 auto;
    min-height: 0;
  }

  .sub-meters {
    display: grid;
    grid-template-columns: repeat(3, minmax(0, 1fr));
    gap: 12px;
    min-height: 0;
  }

  .statusbar {
    display: flex;
    gap: 10px;
    align-items: stretch;
    padding: 6px 10px;
    background: var(--paper-2);
    border: 1px solid var(--hairline);
    border-radius: 12px;
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

  /* antenna · band · swr · power+thermal — single row, fixed proportions. */
  .secondary {
    display: grid;
    grid-template-columns: 1fr 2.4fr 1fr 1.5fr;
    grid-template-rows: minmax(0, 1fr);
    gap: 12px;
    min-height: 0;
  }

  .controls {
    display: grid;
    grid-template-columns: 2fr repeat(4, 1fr);
    gap: 10px;
  }

  /* Portrait reflow stub — when (and if) we build a phone layout, this is
     where the alternative grid lives. For now it intentionally does nothing
     so portrait viewports just letterbox like everything else. */
  @media (orientation: portrait) and (max-aspect-ratio: 4/5) {
    /* TODO: phone-specific stacked layout */
  }
</style>
