<script>
  import ArcGauge from '../graphics/ArcGauge.svelte';

  /**
   * Hero output meter: half-circle arc + linear bar + Peak/Avg/Headroom stats.
   *
   * @type {{
   *   value: number,
   *   max: number,
   *   unit?: string,
   *   peakHold?: number,
   *   avg?: number,
   *   ticks?: number[],
   *   label?: string,
   * }}
   */
  let {
    value = 0,
    max = 1200,
    unit = 'W',
    peakHold = 0,
    avg = 0,
    ticks = [],
    label = 'Output Power',
  } = $props();

  const pct = $derived(Math.max(0, Math.min(100, (value / max) * 100)));
  const dangerPct = $derived(Math.max(0, Math.min(100, ((max - max * 0.917) / max) * 100)));
  const headroom = $derived(Math.max(0, Math.round(max - value)));
  const dangerThreshold = $derived(max * 0.917); // mirror of design
  const warnThreshold = $derived(max * 0.875);
  const chipTone = $derived(value > dangerThreshold ? 'danger' : value > warnThreshold ? 'warn' : 'good');
  const chipText = $derived(value > dangerThreshold ? 'Near Limit' : value > warnThreshold ? 'High Drive' : 'Within Spec');
  const arcTicks = $derived(ticks.length ? [0, ...ticks] : [0, max * 0.25, max * 0.5, max * 0.75, max]);
</script>

<section class="hero card" aria-label={label}>
  <div class="arc-wrap">
    <ArcGauge value={value} max={max} ticks={arcTicks} dangerFrom={dangerThreshold} />
    <div class="readout">
      <div class="num-display">{Math.round(value).toLocaleString()}<span class="unit">{unit}</span></div>
      <div class="lbl">{label}</div>
    </div>
  </div>

  <div class="right">
    <div class="head">
      <h2>Forward Power · 0–{max} W</h2>
      <span class="chip {chipTone}">
        <span class="d"></span>
        {chipText}
      </span>
    </div>

    <div class="bar-track">
      <div class="danger-zone" style="width:{100 - (dangerThreshold / max) * 100}%"></div>
      <div class="bar-fill" style="width:{pct}%"></div>
    </div>
    <div class="bar-ticks num">
      {#each [0, ...ticks, max].filter((v, i, a) => a.indexOf(v) === i) as t}
        <span class:danger={t >= dangerThreshold}>{t}</span>
      {/each}
    </div>

    <div class="stats">
      <div class="s">
        <div class="l">Peak</div>
        <div class="v display">{Math.round(peakHold || 0).toLocaleString()}<span class="u">{unit}</span></div>
      </div>
      <div class="s">
        <div class="l">Average</div>
        <div class="v display">{Math.round(avg || 0).toLocaleString()}<span class="u">{unit}</span></div>
      </div>
      <div class="s">
        <div class="l">Headroom</div>
        <div class="v display">{headroom.toLocaleString()}<span class="u">{unit}</span></div>
      </div>
    </div>
  </div>
</section>

<style>
  .card {
    background: var(--paper);
    border: 1px solid var(--hairline);
    border-radius: 14px;
    padding: 20px 24px;
    height: 100%;
  }
  .hero {
    display: grid;
    /* Arc occupies ~28% of the row width; lets the gauge grow on big screens. */
    grid-template-columns: minmax(220px, 28%) minmax(0, 1fr);
    gap: 24px;
    align-items: stretch;
    height: 100%;
  }
  /* Stack the arc above the bar/stats when the panel gets narrow. */
  @media (max-width: 760px) {
    .hero {
      grid-template-columns: 1fr;
      gap: 16px;
    }
    .arc-wrap {
      max-width: 320px;
      margin: 0 auto;
      width: 100%;
    }
  }
  @media (max-width: 460px) {
    .stats {
      grid-template-columns: 1fr;
    }
  }

  .arc-wrap {
    position: relative;
    aspect-ratio: 1.5 / 1;
    min-height: 0;
  }
  .readout {
    position: absolute;
    left: 0; right: 0; bottom: 4px;
    text-align: center;
    pointer-events: none;
  }
  .num-display {
    font-family: var(--font-display);
    font-weight: 700;
    font-size: clamp(var(--fs-hero), 6vw, 96px);
    color: var(--ink);
    line-height: 1;
    font-variant-numeric: tabular-nums;
  }
  .unit {
    font-family: var(--font-ui);
    font-size: 18px;
    color: var(--ink-3);
    margin-left: 6px;
    font-weight: 500;
  }
  .lbl {
    font-size: 12px;
    color: var(--ink-3);
    letter-spacing: 0.18em;
    text-transform: uppercase;
    font-weight: 600;
    margin-top: 6px;
  }

  .right {
    display: flex;
    flex-direction: column;
    gap: 14px;
    justify-content: center;
  }
  .head {
    display: flex;
    align-items: center;
    justify-content: space-between;
  }
  .head h2 {
    margin: 0;
    font-family: var(--font-ui);
    font-weight: 600;
    font-size: 13px;
    letter-spacing: 0.18em;
    color: var(--ink-3);
    text-transform: uppercase;
  }

  .chip {
    display: inline-flex;
    align-items: center;
    gap: 8px;
    padding: 6px 12px;
    border-radius: 999px;
    font-size: 12px;
    font-weight: 600;
    background: var(--paper);
    border: 1px solid var(--hairline);
    color: var(--ink-2);
  }
  .chip.good   { color: var(--good);   background: var(--good-tint);   border-color: var(--good-edge); }
  .chip.warn   { color: var(--warn);   background: var(--warn-tint);   border-color: var(--warn-edge); }
  .chip.danger { color: var(--danger); background: var(--danger-tint); border-color: var(--danger-edge); }
  .chip .d { width: 8px; height: 8px; border-radius: 50%; background: currentColor; }
  .chip.good .d { animation: pulse 1.6s infinite; }

  .bar-track {
    position: relative;
    height: clamp(18px, 3vh, 36px);
    border-radius: 999px;
    background: var(--paper-2);
    border: 1px solid var(--hairline);
    overflow: hidden;
  }
  .bar-fill {
    position: absolute;
    inset: 1px auto 1px 1px;
    background: linear-gradient(90deg, var(--brand) 0%, var(--brand-2) 100%);
    border-radius: 8px;
    transition: width 200ms cubic-bezier(.4,0,.2,1);
  }
  .danger-zone {
    position: absolute;
    top: 0; bottom: 0; right: 0;
    background: repeating-linear-gradient(135deg,
      rgba(200,50,75,0) 0 5px, rgba(200,50,75,.14) 5px 10px);
    border-left: 1px dashed rgba(200,50,75,.4);
  }

  .bar-ticks {
    display: flex;
    justify-content: space-between;
    font-size: 11px;
    color: var(--ink-3);
    font-weight: 500;
  }
  .bar-ticks span.danger { color: var(--danger); }

  .stats {
    display: grid;
    grid-template-columns: repeat(3, minmax(0, 1fr));
    gap: 12px;
  }
  .s {
    background: var(--paper-2);
    border: 1px solid var(--hairline-2);
    border-radius: 10px;
    padding: clamp(10px, 1.6vh, 18px) clamp(14px, 1.8vw, 22px);
  }
  .s .l {
    font-size: 10px;
    color: var(--ink-3);
    letter-spacing: 0.16em;
    text-transform: uppercase;
    font-weight: 600;
  }
  .s .v {
    font-weight: 600;
    font-size: clamp(22px, 3.2vh, 36px);
    color: var(--ink);
    margin-top: 4px;
    line-height: 1;
  }
  .s .v .u {
    font-family: var(--font-ui);
    font-size: 12px;
    color: var(--ink-3);
    margin-left: 4px;
    font-weight: 500;
  }
</style>
