<script>
  /**
   * @type {{
   *   label: string,
   *   value: number,
   *   max: number,
   *   unit?: string,
   *   peakHold?: number,
   *   ticks?: number[],
   *   overThreshold?: number,
   *   compact?: boolean,
   * }}
   */
  let {
    label,
    value,
    max,
    unit = '',
    peakHold = 0,
    ticks = null,
    overThreshold = 0.9,
    compact = false,
  } = $props();

  const pct = $derived(Math.max(0, Math.min(100, (value / max) * 100)));
  const peakPct = $derived(peakHold ? Math.max(0, Math.min(100, (peakHold / max) * 100)) : 0);
  const over = $derived(pct >= overThreshold * 100);

  const displayValue = $derived(Math.round(value));
</script>

<div class="meter-bar" class:compact>
  <div class="row top">
    <span class="label">{label}</span>
    <div class="value">
      <span class="num val">{displayValue}</span>
      {#if unit}<span class="unit">{unit}</span>{/if}
    </div>
  </div>

  <div class="track">
    <div class="fill" class:over style="width: {pct}%;"></div>
    {#if peakPct > 0 && peakPct > pct}
      <div class="peak" style="left: {peakPct}%;"></div>
    {/if}
  </div>

  {#if ticks && ticks.length}
    <div class="ticks num">
      <span>0</span>
      {#each ticks as t}
        <span>{t}</span>
      {/each}
    </div>
  {/if}
</div>

<style>
  .meter-bar {
    display: flex;
    flex-direction: column;
    gap: 6px;
    padding: 8px 10px 6px;
    background: var(--color-surface);
    border: 1px solid var(--color-border);
    border-radius: 6px;
    box-shadow: var(--shadow-card);
    height: 100%;
  }
  .meter-bar.compact { padding: 6px 8px 4px; gap: 4px; }

  .row.top {
    display: flex;
    justify-content: space-between;
    align-items: baseline;
  }
  .value { display: flex; align-items: baseline; gap: 4px; }
  .val {
    font-size: var(--fs-xl);
    font-weight: 600;
    color: var(--color-text-strong);
    line-height: 1;
  }
  .compact .val { font-size: var(--fs-lg); }
  .unit {
    font-size: var(--fs-sm);
    color: var(--color-text-muted);
    text-transform: uppercase;
    letter-spacing: 0.06em;
  }

  .track {
    position: relative;
    height: 12px;
    background: var(--gauge-track);
    border: 1px solid var(--color-border);
    border-radius: 3px;
    overflow: hidden;
  }
  .compact .track { height: 9px; }
  .fill {
    position: absolute;
    inset: 0 auto 0 0;
    background: linear-gradient(180deg, var(--color-accent-hi) 0%, var(--gauge-fill) 60%, var(--color-accent-lo) 100%);
    transition: width 80ms linear;
    box-shadow: inset 0 0 6px var(--color-accent-glow);
  }
  .fill.over {
    background: linear-gradient(180deg, #ff8a87 0%, var(--gauge-fill-over) 60%, #b8302d 100%);
    box-shadow: inset 0 0 6px #ff4d4a55;
  }
  .peak {
    position: absolute;
    top: -1px;
    bottom: -1px;
    width: 2px;
    background: var(--gauge-peak);
    box-shadow: 0 0 4px #ffffff80;
    transform: translateX(-1px);
  }

  .ticks {
    display: flex;
    justify-content: space-between;
    font-size: var(--fs-xs);
    color: var(--color-text-faint);
    padding: 0 1px;
  }
</style>
