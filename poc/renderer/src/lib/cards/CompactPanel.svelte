<script>
  import ArcGauge from '../graphics/ArcGauge.svelte';
  import BandTile from './BandTile.svelte';
  import OperateToggle from '../controls/OperateToggle.svelte';
  import ControlButton from '../controls/ControlButton.svelte';
  import Icon from '../icons/Icon.svelte';

  /**
   * Compact / kiosk view. Shows only what an operator wants at a glance:
   * power out, SWR, band, fan state, on-air indicator, operate/standby,
   * bypass, and a single antenna button that cycles 1 → 2 → 3 → 1.
   *
   * @type {{
   *   outW: number,
   *   outMax: number,
   *   outTicks: number[],
   *   swr: number,
   *   bandLabels: string[],
   *   bandActive: number,
   *   bandFreq?: string,
   *   bandEnabled?: boolean,
   *   onbandselect?: (i: number) => void,
   *   antenna: number,
   *   antennaPorts?: number[],
   *   antennaLabels?: Record<number, string>,
   *   onantennacycle?: () => void,
   *   fanFull: boolean,
   *   onfan?: () => void,
   *   operate: boolean,
   *   onoperate?: (next: boolean) => void,
   *   bypass: boolean,
   *   onbypass?: () => void,
   *   txActive: boolean,
   * }}
   */
  let {
    outW,
    outMax,
    outTicks,
    swr,
    bandLabels,
    bandActive,
    bandFreq = '',
    bandEnabled = true,
    onbandselect,
    antenna,
    antennaPorts = [1, 2, 3],
    antennaLabels = { 1: 'Doublet', 2: 'Dipole', 3: '11-el Yagi' },
    onantennacycle,
    fanFull,
    onfan,
    operate,
    onoperate,
    bypass,
    onbypass,
    txActive,
  } = $props();

  const swrTone = $derived(swr < 1.5 ? 'good' : swr < 2 ? 'warn' : 'danger');
  const onAirTone = $derived(
    bypass ? 'bypass' :
    txActive ? 'tx' : 'idle'
  );
  const onAirText = $derived(
    bypass ? 'BYPASS' :
    txActive ? 'ON AIR' : 'STANDBY'
  );
  const fanLabel = $derived(fanFull ? 'Fan 100%' : 'Fan Auto');
</script>

<div class="compact">
  <!-- Big on-air banner -->
  <div class="onair" data-tone={onAirTone}>
    <span class="dot"></span>
    <span class="label">{onAirText}</span>
  </div>

  <!-- Hero: arc + big readout + side stats -->
  <section class="hero">
    <div class="arc-wrap">
      <ArcGauge value={outW} max={outMax} ticks={[0, ...outTicks]} dangerFrom={outMax * 0.917} />
    </div>

    <div class="readout">
      <div class="big">{Math.round(outW).toLocaleString()}<span class="unit">W</span></div>
      <div class="caption">Power Out · 0–{outMax} W</div>
    </div>

    <aside class="side-stats">
      <div class="stat" data-tone={swrTone}>
        <div class="stat-l">SWR</div>
        <div class="stat-v">{swr.toFixed(2)}<span class="stat-u">:1</span></div>
      </div>

      <button class="stat fan-btn" type="button" class:active={fanFull} onclick={() => onfan?.()}>
        <div class="stat-l">{fanLabel}</div>
        <div class="stat-icon"><Icon name="fan" size={28} /></div>
      </button>
    </aside>
  </section>

  <!-- Band selector ribbon -->
  <div class="band">
    <BandTile
      bands={bandLabels}
      activeIndex={bandActive}
      freq={bandFreq}
      enabled={bandEnabled}
      onselect={onbandselect} />
  </div>

  <!-- Footer controls -->
  <div class="controls">
    <OperateToggle operate={operate} onchange={onoperate} />
    <ControlButton icon="bypass" label="Bypass" kind="toggle" active={bypass} onclick={onbypass} />
    <button class="ant-btn" type="button"
            onclick={() => onantennacycle?.()}
            title="Cycle antenna {antennaPorts.join('/')}">
      <span class="ant-l">Antenna</span>
      <span class="ant-n">ANT {antenna}</span>
      <span class="ant-name">{antennaLabels[antenna] || ''}</span>
    </button>
  </div>
</div>

<style>
  .compact {
    display: grid;
    grid-template-rows: 64px 1fr 92px 60px;
    gap: 14px;
    height: 100%;
    min-height: 0;
  }

  /* On-air banner */
  .onair {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 18px;
    border-radius: 14px;
    border: 1px solid var(--hairline);
    background: var(--paper-2);
    color: var(--ink-3);
    font-family: var(--font-display);
    font-weight: 700;
    font-size: 30px;
    letter-spacing: 0.32em;
    transition: background 200ms, color 200ms, border-color 200ms;
  }
  .onair .dot {
    width: 14px; height: 14px;
    border-radius: 50%;
    background: currentColor;
    opacity: 0.6;
  }
  .onair[data-tone="tx"] {
    background: var(--danger-tint);
    border-color: var(--danger-edge);
    color: var(--danger);
  }
  .onair[data-tone="tx"] .dot { animation: pulse 1.2s infinite; opacity: 1; }
  .onair[data-tone="bypass"] {
    background: var(--warn-tint);
    border-color: var(--warn-edge);
    color: var(--warn);
  }
  .onair[data-tone="idle"] {
    background: var(--paper-2);
    color: var(--ink-3);
  }

  /* Hero row */
  .hero {
    display: grid;
    grid-template-columns: 360px 1fr 220px;
    gap: 18px;
    align-items: center;
    min-height: 0;
  }
  .arc-wrap {
    position: relative;
    height: 100%;
    min-height: 0;
    display: grid;
    place-items: center;
  }
  .arc-wrap :global(svg) { width: 100%; height: 100%; max-height: 280px; }

  .readout {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    gap: 8px;
  }
  .big {
    font-family: var(--font-display);
    font-weight: 700;
    font-size: 110px;
    line-height: 1;
    color: var(--ink);
    font-variant-numeric: tabular-nums;
  }
  .big .unit {
    font-family: var(--font-ui);
    font-size: 28px;
    color: var(--ink-3);
    margin-left: 10px;
    font-weight: 500;
  }
  .caption {
    font-size: 13px;
    color: var(--ink-3);
    letter-spacing: 0.18em;
    text-transform: uppercase;
    font-weight: 600;
  }

  .side-stats {
    display: flex;
    flex-direction: column;
    gap: 12px;
    height: 100%;
    min-height: 0;
  }
  .stat {
    flex: 1 1 0;
    background: var(--paper-2);
    border: 1px solid var(--hairline-2);
    border-radius: 14px;
    padding: 14px 18px;
    display: flex;
    flex-direction: column;
    justify-content: center;
    gap: 6px;
    min-height: 0;
    color: var(--ink-2);
    text-align: left;
  }
  .stat-l {
    font-size: 11px;
    color: var(--ink-3);
    letter-spacing: 0.18em;
    text-transform: uppercase;
    font-weight: 600;
  }
  .stat-v {
    font-family: var(--font-num);
    font-weight: 700;
    font-size: 38px;
    color: var(--ink);
    line-height: 1;
  }
  .stat-u {
    font-family: var(--font-ui);
    font-size: 14px;
    color: var(--ink-3);
    margin-left: 6px;
    font-weight: 500;
  }
  .stat[data-tone="good"]   .stat-v { color: var(--good); }
  .stat[data-tone="warn"]   .stat-v { color: var(--warn); }
  .stat[data-tone="danger"] .stat-v { color: var(--danger); }

  .fan-btn {
    cursor: pointer;
    transition: border-color 120ms, background 120ms;
  }
  .fan-btn:hover { border-color: var(--brand-2); }
  .fan-btn.active {
    background: var(--brand-3);
    border-color: var(--brand);
    color: var(--brand);
  }
  .stat-icon {
    color: var(--ink-3);
    display: flex;
    justify-content: flex-end;
  }
  .fan-btn.active .stat-icon { color: var(--brand); }

  /* Band selector — let the existing tile fill the row. */
  .band {
    min-height: 0;
  }
  .band :global(.tile) { height: 100%; }

  /* Bottom controls */
  .controls {
    display: grid;
    grid-template-columns: 2fr 1fr 1.4fr;
    gap: 12px;
  }

  .ant-btn {
    display: grid;
    grid-template-columns: 1fr auto;
    grid-template-rows: auto auto;
    align-items: center;
    column-gap: 14px;
    padding: 0 18px;
    border-radius: 12px;
    border: 1px solid var(--hairline);
    background: var(--paper);
    color: var(--ink-2);
    cursor: pointer;
    text-align: left;
    transition: border-color 120ms, color 120ms, background 120ms;
  }
  .ant-btn:hover { border-color: var(--brand-2); color: var(--brand); background: #fafbff; }
  .ant-l {
    grid-column: 1;
    grid-row: 1;
    font-size: 11px;
    letter-spacing: 0.18em;
    text-transform: uppercase;
    color: var(--ink-3);
    font-weight: 600;
  }
  .ant-n {
    grid-column: 2;
    grid-row: 1 / span 2;
    font-family: var(--font-num);
    font-weight: 700;
    font-size: 28px;
    color: var(--ink);
    line-height: 1;
    align-self: center;
  }
  .ant-name {
    grid-column: 1;
    grid-row: 2;
    font-size: 12px;
    color: var(--ink-3);
    font-weight: 500;
  }

  @keyframes pulse {
    0%, 100% { transform: scale(1);   opacity: 1;   }
    50%      { transform: scale(1.4); opacity: 0.5; }
  }
</style>
