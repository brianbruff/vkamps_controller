<script>
  import { onMount } from 'svelte';
  import { get } from 'svelte/store';
  import BarGauge from './components/BarGauge.svelte';
  import InfoPanel from './components/InfoPanel.svelte';
  import ControlButton from './components/ControlButton.svelte';
  import Setup from './components/Setup.svelte';
  import {
    p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12,
    p1Peak, p2Peak, p4Peak, p12Peak,
    isConnected, isTransmitting, isBypassed,
    bandName, errorText, hasError, fanMode,
    applyTcpData, applyUdpData, resetTelemetry,
  } from './stores/telemetry.js';
  import { appConfig, loadConfig } from './stores/config.js';

  let showSetup = $state(false);
  let connected = $state(false);
  let connecting = $state(false);

  // Peak hold timers
  let opTimer = null;
  let rpTimer = null;
  let crTimer = null;
  let inTimer = null;

  // Computed display values
  let outputWatts = $derived.by(() => {
    const raw = $p1;
    const koef = $appConfig.koef;
    const div = { 600: 952, 1200: 476, 2400: 238 }[koef] || 476;
    return Math.floor((raw * raw) / div);
  });

  let reflectedWatts = $derived.by(() => {
    const raw = $p2;
    const koef = $appConfig.koef;
    const div = { 600: 952, 1200: 476, 2400: 238 }[koef] || 476;
    return Math.floor((raw * raw) / div);
  });

  let currentAmps = $derived.by(() => {
    const raw = $p4;
    const koef = $appConfig.koef;
    const inp = $appConfig.inputIndicator;
    const mult = {
      600:  { false: 66,  true: 102 },
      1200: { false: 132, true: 204 },
      2400: { false: 264, true: 408 },
    }[koef]?.[inp] ?? 132;
    return (raw * mult / 1000).toFixed(0);
  });

  let inputPowerW = $derived.by(() => {
    const raw = $p12;
    const isLan = $appConfig.mode === 'LAN';
    return Math.floor((raw * raw) / (isLan ? 540 : 476));
  });

  let voltageV = $derived(($p5 / 10).toFixed(1));
  let tempDisplay = $derived.by(() => {
    const raw = $p3;
    if ($appConfig.tempUnit === 'F') return Math.floor(18 * raw / 10 + 32);
    return raw;
  });

  let swrDisplay = $derived.by(() => {
    const fwd = $p1Peak;
    const ref = $p2Peak;
    if (fwd === 0 || ref === 0 || fwd <= ref) return '1.00';
    const swr = (fwd + ref) / (fwd - ref);
    return swr < 10 ? swr.toFixed(2) : '9.99';
  });

  let efficiencyPct = $derived.by(() => {
    const koef = $appConfig.koef;
    const inp = $appConfig.inputIndicator;
    const div = { 600: 952, 1200: 476, 2400: 238 }[koef] || 476;
    const mult = {
      600:  { false: 66,  true: 102 },
      1200: { false: 132, true: 204 },
      2400: { false: 264, true: 408 },
    }[koef]?.[inp] ?? 132;
    const vV = $p5 / 10;
    const cA = ($p4 * mult / 100) / 10;
    const inputW = vV * cA;
    if (inputW === 0) return 0;
    const outputW = ($p1 * $p1) / div;
    const eff = (outputW * 100) / inputW;
    return Math.min(Math.floor(eff), 99);
  });

  let antenna = $derived($p7 || $appConfig.antenna);
  let sizeind1 = $derived($appConfig.inputIndicator ? 247 : 380);

  const scaleOutput = $derived(({
    600:  [5, 10, 20, 50, 100, 200, 300, 400, 500, 600],
    1200: [5, 10, 20, 50, 100, 200, 400, 600, 800, 1000, 1200],
    2400: [10, 50, 100, 200, 400, 800, 1200, 1600, 2000, 2400],
  })[$appConfig.koef] || [5, 10, 20, 50, 100, 200, 400, 600, 800, 1000, 1200]);

  const scaleReflected = $derived(({
    600:  [2, 10, 25, 50],
    1200: [5, 20, 50, 100],
    2400: [10, 40, 100, 200],
  })[$appConfig.koef] || [5, 20, 50, 100]);

  const scaleCurrent = $derived(({
    600:  [5, 10, 15, 20],
    1200: [10, 20, 30, 40],
    2400: [20, 40, 60, 80],
  })[$appConfig.koef] || [10, 20, 30, 40]);

  let alertAudio = null;
  let errorAudio = null;

  onMount(async () => {
    await loadConfig();

    try {
      alertAudio = new Audio('/assets/alert.wav');
      errorAudio = new Audio('/assets/error.wav');
    } catch { /* sounds not available */ }

    if (window.amp) {
      window.amp.onData((pkt) => {
        if (pkt.type === 'tcp') applyTcpData(pkt.data);
        else if (pkt.type === 'udp') { applyUdpData(pkt.data); updatePeaks(); }
      });
      window.amp.onConnectionChange((conn) => {
        connected = conn;
        isConnected.set(conn);
        if (!conn) resetTelemetry();
        playAlert();
      });
      window.amp.onError((msg) => console.error('Transport error:', msg));
    }
  });

  function updatePeaks() {
    const raw1 = get(p1), raw2 = get(p2), raw4 = get(p4), raw12 = get(p12);
    const peakDecayMs = $appConfig.peakHoldDuration || 2000;
    if (raw1 >= get(p1Peak)) { p1Peak.set(raw1); clearTimeout(opTimer); opTimer = setTimeout(() => p1Peak.set(0), peakDecayMs); }
    if (raw2 >= get(p2Peak)) { p2Peak.set(raw2); clearTimeout(rpTimer); rpTimer = setTimeout(() => p2Peak.set(0), peakDecayMs); }
    if (raw4 >= get(p4Peak)) { p4Peak.set(raw4); clearTimeout(crTimer); crTimer = setTimeout(() => p4Peak.set(0), peakDecayMs); }
    if ($appConfig.inputIndicator && raw12 >= get(p12Peak)) { p12Peak.set(raw12); clearTimeout(inTimer); inTimer = setTimeout(() => p12Peak.set(0), peakDecayMs); }
  }

  function playAlert() { if ($appConfig.sound && alertAudio) alertAudio.play().catch(() => {}); }
  function playError() { if ($appConfig.sound && errorAudio) errorAudio.play().catch(() => {}); }
  $effect(() => { if ($hasError) playError(); });

  async function toggleConnect() {
    if (connected) { await window.amp?.disconnect(); }
    else { connecting = true; try { await window.amp?.connect(); } catch(e) { console.error(e); } connecting = false; }
  }
  async function handleReset() { if (connected && !$isTransmitting) await window.amp?.sendCommand('23'); }
  async function handleSleep() { if (connected && !$isTransmitting) await window.amp?.sendCommand('44'); }
  async function handleBypass() { if (!connected || $isTransmitting) return; await window.amp?.sendCommand($isBypassed ? '22' : '21'); }
  async function handleCooling() { if (!connected || $isTransmitting) return; await window.amp?.sendCommand($p10 === 1 ? '46' : '45'); }
  async function cycleAntenna() { if (!connected || $isTransmitting) return; let next = (antenna % 3) + 1; await window.amp?.sendCommand(['31','32','33'][next-1]); }
  async function cycleBand() {
    if (!connected || $isTransmitting || $appConfig.cat !== 5) return;
    let next = ($p6 || 0) >= 7 ? 0 : ($p6 || 0) + 1;
    await window.amp?.sendCommand(['71','72','73','74','75','76','77','78'][next]);
    const antIdx = $appConfig.antennaMap[next];
    if (antIdx >= 1 && antIdx <= 3) await window.amp?.sendCommand(['31','32','33'][antIdx-1]);
  }
  async function toggleVolts() {
    if (!connected || $isTransmitting) return;
    const cfg = get(appConfig);
    const nv = !cfg.voltsMode;
    await window.amp?.sendCommand(nv ? '42' : '41');
    await window.amp?.saveConfig({ ...cfg, voltsMode: nv });
    appConfig.update(c => ({ ...c, voltsMode: nv }));
  }
</script>

<main class="app">
  <div class="neon-border-outer"></div>
  <div class="neon-border-inner"></div>

  <!-- Header bar -->
  <header class="header">
    <div class="hdr-left">
      <div class="logo-bezel">
        <div class="logo-bezel-inner">
          <!-- Green dots + signal bars row above text -->
          <div class="logo-top-row">
            <span class="green-dots"></span>
            <div class="signal-bars">
              <span class="sbar"></span><span class="sbar"></span><span class="sbar"></span>
              <span class="sbar"></span><span class="sbar"></span><span class="sbar"></span><span class="sbar"></span>
            </div>
          </div>
          <div class="logo-text-row">
            <span class="vk">VK</span><span class="amp">AMP</span>
          </div>
        </div>
      </div>
      <!-- Green line under logo, then diagonal drop to full-width bottom line -->
      <svg class="hdr-sep-svg" viewBox="0 0 140 22" preserveAspectRatio="xMinYMid meet">
        <defs>
          <linearGradient id="glnH" x1="0%" y1="0%" x2="100%" y2="0%">
            <stop offset="0%" stop-color="#6aff3a" stop-opacity="0.9"/>
            <stop offset="40%" stop-color="#39ff14" stop-opacity="0.6"/>
            <stop offset="80%" stop-color="#39ff14" stop-opacity="0.25"/>
            <stop offset="100%" stop-color="#39ff14" stop-opacity="0.05"/>
          </linearGradient>
        </defs>
        <!-- Upper green line under logo panel -->
        <line x1="0" y1="3" x2="55" y2="3" stroke="url(#glnH)" stroke-width="2"/>
        <!-- Diagonal drop -->
        <line x1="55" y1="3" x2="75" y2="18" stroke="#39ff14" stroke-width="1.5" stroke-opacity="0.5"/>
        <!-- Lower line continuing to right -->
        <line x1="75" y1="18" x2="140" y2="18" stroke="#39ff14" stroke-width="1.5" stroke-opacity="0.2"/>
      </svg>
    </div>
    <span class="header-subtitle">Ham Radio Amplifier Control</span>
    <span class="header-ip" class:online={connected}>
      {connected ? ($appConfig.mode === 'LAN' ? $appConfig.lanIp : $appConfig.comPort) : 'Disconnected'}
      <svg class="wifi-svg" viewBox="0 0 24 24" width="18" height="18" fill="none">
        <path d="M12 18.5a1.5 1.5 0 100-3 1.5 1.5 0 000 3z" fill="currentColor"/>
        <path d="M8.46 14.54a5 5 0 017.08 0" stroke="currentColor" stroke-width="1.8" stroke-linecap="round"/>
        <path d="M5.64 11.64a9 9 0 0112.72 0" stroke="currentColor" stroke-width="1.8" stroke-linecap="round"/>
        <path d="M2.81 8.81a13 13 0 0118.38 0" stroke="currentColor" stroke-width="1.8" stroke-linecap="round"/>
      </svg>
    </span>
    <!-- Full-width bottom border line (continues from the diagonal drop) -->
    <div class="hdr-bottom-line"></div>
  </header>

  <!-- Output gauge (large) -->
  <section class="output-gauge">
    <BarGauge
      value={$p9 === 0 ? 0 : $p1}
      peak={$p1Peak}
      maxRaw={776}
      scaleLabels={scaleOutput}
      label="Output"
      displayValue={$p9 !== 0 ? String(outputWatts) : '0'}
      unit="w"
      height={36}
      large={true}
    />
  </section>

  <!-- Secondary gauges (Reflected / Input / Current) -->
  <section class="sec-gauges">
    <div class="sec-col">
      <BarGauge
        value={$p9 === 0 ? 0 : $p2}
        peak={$p2Peak}
        maxRaw={sizeind1}
        scaleLabels={scaleReflected}
        label="Reflected"
        displayValue={$p9 !== 0 ? String(reflectedWatts) : '0'}
        unit="w"
        height={18}
      />
    </div>
    {#if $appConfig.inputIndicator}
      <div class="sec-col">
        <BarGauge
          value={$p9 === 0 ? 0 : $p12}
          peak={$p12Peak}
          maxRaw={247}
          scaleLabels={[5, 20, 50, 100]}
          label="Input"
          displayValue={$p9 !== 0 ? String(inputPowerW) : '0'}
          unit="w"
          height={18}
        />
      </div>
    {/if}
    <div class="sec-col">
      <BarGauge
        value={$p9 === 0 ? 0 : $p4}
        peak={$p4Peak}
        maxRaw={sizeind1}
        scaleLabels={scaleCurrent}
        label="Current"
        displayValue={$p9 !== 0 ? currentAmps : '0'}
        unit="A"
        height={18}
        amber={true}
      />
    </div>
  </section>

  <!-- Status bar -->
  <section class="status-bar">
    <button class="status-label" class:err={$hasError} onclick={handleReset}>
      <span class="status-deco-line"></span>
      {$errorText}
      <span class="status-deco-line"></span>
    </button>
    <span class="air-label" class:tx={$isTransmitting} class:bp={$isBypassed && !$isTransmitting}>
      ON AIR
    </span>
    <button class="fan-label" class:fan100={$p10 === 1} onclick={handleCooling}>
      <span class="status-deco-line"></span>
      Fan {$fanMode}
      <span class="status-deco-line"></span>
    </button>
  </section>

  <!-- Info panels -->
  <section class="info-row">
    <InfoPanel icon={'\u{1F4F6}'} value={String(antenna)} label="Antenna"
      clickable={connected && !$isTransmitting} onclick={cycleAntenna} />
    <InfoPanel value={$bandName || '--'} unit="M" label="Band"
      clickable={connected && !$isTransmitting && $appConfig.cat === 5} onclick={cycleBand} />
    <InfoPanel icon={'\u26A1'} value={swrDisplay} label="SWR" />
    <InfoPanel icon={'\u26A1'} value={String($isTransmitting ? efficiencyPct : 0)} unit="%" label="Eff %" />
    <InfoPanel icon={'\u26A1'} value={voltageV} unit="V"
      label={$appConfig.voltsMode ? 'Volts+' : 'Volts'}
      clickable={connected && !$isTransmitting} onclick={toggleVolts} active={$appConfig.voltsMode} />
    <InfoPanel icon={'\uD83C\uDF21'} value={String(tempDisplay)}
      unit={$appConfig.tempUnit === 'F' ? '\u00B0F' : '\u00B0C'}
      label={'Temp ' + ($appConfig.tempUnit === 'F' ? '\u00B0F' : '\u00B0C')} />
  </section>

  <!-- Control buttons -->
  <section class="ctrl-row">
    <ControlButton icon={'\u21BA'} label="Reset" onclick={handleReset} disabled={!connected || $isTransmitting} />
    <ControlButton icon={'\u{1F4A4}'} label="Sleep" onclick={handleSleep} disabled={!connected || $isTransmitting} />
    <ControlButton icon={'\u23CF'} label="ByPass" onclick={handleBypass} disabled={!connected || $isTransmitting} active={$isBypassed} />
    <ControlButton icon={'\u2744'} label="Cooling" onclick={handleCooling} disabled={!connected || $isTransmitting} active={$p10 === 1} />
    <ControlButton
      icon={connected ? '\u{1F7E2}' : '\u26AA'}
      label={connecting ? 'Connecting...' : (connected ? 'Connected' : 'Connect')}
      onclick={toggleConnect} active={connected} />
    <ControlButton icon={'\u2699'} label="Setup" onclick={() => showSetup = true} />
  </section>
</main>

{#if showSetup}
  <Setup onclose={() => showSetup = false} />
{/if}

<style>
  :global(*) {
    margin: 0;
    padding: 0;
    box-sizing: border-box;
  }
  :global(body) {
    background: #020202;
    color: #d0d0d0;
    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', 'Helvetica Neue', sans-serif;
    overflow: hidden;
    user-select: none;
    -webkit-app-region: drag;
  }
  :global(button, select, input, a) {
    -webkit-app-region: no-drag;
  }

  .app {
    display: flex;
    flex-direction: column;
    height: 100vh;
    padding: 10px 18px 8px 18px;
    gap: 5px;
    background:
      radial-gradient(ellipse at 50% 50%, rgba(25, 60, 25, 0.2) 0%, transparent 70%),
      linear-gradient(180deg, #0a0e0a 0%, #060806 100%);
    border: 2px solid rgba(57, 255, 20, 0.7);
    border-radius: 14px;
    margin: 4px;
    position: relative;
    overflow: hidden;
    box-shadow:
      inset 0 0 60px rgba(57, 255, 20, 0.06),
      inset 0 0 20px rgba(57, 255, 20, 0.03),
      0 0 8px rgba(57, 255, 20, 0.5),
      0 0 20px rgba(57, 255, 20, 0.35),
      0 0 45px rgba(57, 255, 20, 0.2),
      0 0 80px rgba(57, 255, 20, 0.1),
      0 0 120px rgba(57, 255, 20, 0.05);
  }

  /* Outer neon glow border layer */
  .neon-border-outer {
    position: absolute;
    top: -2px; left: -2px; right: -2px; bottom: -2px;
    border-radius: 16px;
    border: 1px solid rgba(57, 255, 20, 0.15);
    pointer-events: none;
    z-index: 0;
    box-shadow:
      0 0 30px rgba(57, 255, 20, 0.25),
      0 0 60px rgba(57, 255, 20, 0.12);
  }
  .neon-border-inner {
    position: absolute;
    top: 0; left: 0; right: 0; bottom: 0;
    border-radius: 14px;
    border: 1px solid rgba(57, 255, 20, 0.25);
    pointer-events: none;
    z-index: 0;
  }

  /* ═══════════════ HEADER ═══════════════ */
  .header {
    display: flex;
    align-items: flex-end;
    gap: 0;
    padding: 0 0 8px 0;
    z-index: 1;
    position: relative;
    min-height: 52px;
  }
  .hdr-left {
    display: flex;
    flex-direction: column;
    align-items: flex-start;
    flex-shrink: 0;
  }

  /* ── Logo bezel ── */
  .logo-bezel {
    padding: 2px;
    border-radius: 8px;
    background: linear-gradient(160deg, #4a4a4a 0%, #2a2a2a 40%, #141414 80%, #0a0a0a 100%);
    box-shadow: 0 1px 4px rgba(0,0,0,0.8), inset 0 1px 0 rgba(255,255,255,0.06);
  }
  .logo-bezel-inner {
    display: flex;
    flex-direction: column;
    background: linear-gradient(180deg, #0c100c 0%, #070907 50%, #0a0d0a 100%);
    border-radius: 6px;
    padding: 2px 14px 4px 10px;
    border: 1px solid rgba(30,30,30,0.5);
    box-shadow: inset 0 2px 8px rgba(0,0,0,0.8);
    gap: 0;
  }

  /* Top row: green dots + signal bars */
  .logo-top-row {
    display: flex;
    align-items: flex-end;
    gap: 4px;
    margin-bottom: 0px;
    padding-left: 2px;
  }
  .green-dots {
    display: block;
    width: 40px;
    height: 4px;
    background-image: radial-gradient(circle, #39ff14 1px, transparent 1px);
    background-size: 4px 4px;
    opacity: 0.5;
    filter: drop-shadow(0 0 2px rgba(57,255,20,0.4));
  }
  .signal-bars {
    display: flex;
    align-items: flex-end;
    gap: 2px;
  }
  .sbar {
    display: block;
    width: 3px;
    background: #39ff14;
    border-radius: 0.5px;
    box-shadow: 0 0 3px rgba(57,255,20,0.7);
  }
  .sbar:nth-child(1) { height: 3px; }
  .sbar:nth-child(2) { height: 5px; }
  .sbar:nth-child(3) { height: 7px; }
  .sbar:nth-child(4) { height: 9px; }
  .sbar:nth-child(5) { height: 11px; }
  .sbar:nth-child(6) { height: 13px; }
  .sbar:nth-child(7) { height: 16px; }

  /* Logo text row */
  .logo-text-row {
    display: flex;
    align-items: baseline;
    line-height: 1;
  }
  .vk {
    font-family: 'Impact', 'Arial Black', sans-serif;
    font-size: 34px;
    font-weight: 900;
    font-style: italic;
    letter-spacing: -2px;
    background: linear-gradient(180deg,
      #5a6a5a 0%, #8a9a8a 18%, #b0c4b0 32%,
      #ccdccc 46%, #a0b4a0 58%,
      #708070 74%, #4a5a4a 90%);
    -webkit-background-clip: text;
    -webkit-text-fill-color: transparent;
    background-clip: text;
    filter: drop-shadow(0 1px 0 rgba(0,0,0,0.9)) drop-shadow(0 0 6px rgba(57,255,20,0.15));
  }
  .amp {
    font-family: 'Arial Narrow', 'Helvetica Neue', sans-serif;
    font-weight: 400;
    font-size: 26px;
    letter-spacing: 5px;
    margin-left: 2px;
    background: linear-gradient(180deg,
      #7a7a7a 0%, #aaa 22%, #d0d0d0 42%,
      #eee 50%, #b8b8b8 62%,
      #888 80%, #606060 100%);
    -webkit-background-clip: text;
    -webkit-text-fill-color: transparent;
    background-clip: text;
    filter: drop-shadow(0 1px 0 rgba(0,0,0,0.8));
  }

  /* ── Green separator SVG: line under logo → diagonal → bottom line ── */
  .hdr-sep-svg {
    width: 130px;
    height: 20px;
    margin-top: -1px;
    margin-left: 2px;
    flex-shrink: 0;
    filter: drop-shadow(0 0 3px rgba(57,255,20,0.3));
  }

  /* ── Full-width green bottom line (header separator) ── */
  .hdr-bottom-line {
    position: absolute;
    bottom: 0;
    left: 0;
    right: 0;
    height: 1.5px;
    background: linear-gradient(90deg,
      rgba(57,255,20,0.6) 0%,
      rgba(57,255,20,0.3) 25%,
      rgba(57,255,20,0.12) 60%,
      rgba(57,255,20,0.06) 100%);
    box-shadow: 0 0 6px rgba(57,255,20,0.15);
  }

  /* ── Subtitle ── */
  .header-subtitle {
    color: #5a5a5a;
    font-size: 15px;
    flex: 1;
    letter-spacing: 0.8px;
    font-weight: 500;
    padding-left: 10px;
    align-self: center;
  }

  /* ── IP address + wifi ── */
  .header-ip {
    font-family: 'Courier New', 'Consolas', monospace;
    font-size: 14px;
    color: #ff4444;
    letter-spacing: 0.5px;
    display: flex;
    align-items: center;
    gap: 6px;
    padding-right: 4px;
    align-self: center;
  }
  .header-ip.online {
    color: #39ff14;
    text-shadow: 0 0 8px rgba(57,255,20,0.4);
  }
  .wifi-svg {
    color: #39ff14;
    filter: drop-shadow(0 0 4px rgba(57,255,20,0.5));
  }
  .header-ip:not(.online) .wifi-svg {
    color: #ff4444;
    filter: none;
    opacity: 0.5;
  }

  /* OUTPUT GAUGE */
  .output-gauge {
    z-index: 1;
    padding: 0;
  }

  /* SECONDARY GAUGES */
  .sec-gauges {
    display: flex;
    gap: 10px;
    z-index: 1;
  }
  .sec-col {
    flex: 1;
    min-width: 0;
    border: 1px solid rgba(57, 255, 20, 0.25);
    border-radius: 6px;
    padding: 4px 6px 2px;
    background: rgba(5, 10, 5, 0.4);
  }

  /* STATUS BAR */
  .status-bar {
    display: flex;
    justify-content: center;
    align-items: center;
    gap: 28px;
    padding: 2px 0;
    z-index: 1;
  }
  .status-label {
    background: none;
    border: none;
    font-size: 13px;
    font-weight: 600;
    color: #39ff14;
    cursor: default;
    font-family: inherit;
    padding: 0;
    display: flex;
    align-items: center;
    gap: 8px;
  }
  .status-label.err {
    color: #ff3333;
    cursor: pointer;
    text-shadow: 0 0 6px rgba(255, 51, 51, 0.5);
  }
  .status-deco-line {
    display: inline-block;
    width: 30px;
    height: 1px;
    border-top: 1px dashed rgba(57, 255, 20, 0.3);
  }
  .air-label {
    font-size: 18px;
    font-weight: 800;
    color: #39ff14;
    letter-spacing: 3px;
    font-family: 'Impact', 'Arial Narrow', sans-serif;
    text-shadow: 0 0 10px rgba(57, 255, 20, 0.3);
  }
  .air-label.tx {
    color: #ff3333;
    text-shadow: 0 0 10px rgba(255, 51, 51, 0.6);
  }
  .air-label.bp {
    color: #cc44cc;
    text-shadow: 0 0 8px rgba(204, 68, 204, 0.4);
  }
  .fan-label {
    background: none;
    border: none;
    font-size: 13px;
    font-weight: 600;
    color: #39ff14;
    cursor: pointer;
    font-family: inherit;
    padding: 0;
    display: flex;
    align-items: center;
    gap: 8px;
  }
  .fan-label.fan100 {
    color: #cc44cc;
    text-shadow: 0 0 6px rgba(204, 68, 204, 0.4);
  }

  /* INFO ROW */
  .info-row {
    display: flex;
    gap: 6px;
    justify-content: center;
    z-index: 1;
  }

  /* CONTROL ROW */
  .ctrl-row {
    display: flex;
    gap: 8px;
    justify-content: center;
    padding-top: 4px;
    border-top: 1px solid rgba(57, 255, 20, 0.12);
    z-index: 1;
  }
</style>
