<script>
  /**
   * @type {{
   *   value: number,
   *   max: number,
   *   unit?: string,
   *   peakHold?: number,
   *   ticks?: number[],
   *   label?: string,
   * }}
   */
  let {
    value = 0,
    max = 1200,
    unit = 'W',
    peakHold = 0,
    ticks = [],
    label = 'OUTPUT POWER',
  } = $props();

  const pct = $derived(Math.max(0, Math.min(100, (value / max) * 100)));
  const peakPct = $derived(peakHold ? Math.max(0, Math.min(100, (peakHold / max) * 100)) : 0);
  const over = $derived(pct >= 90);
</script>

<section class="output-meter" aria-label="Output power">
  <div class="head">
    <span class="label">{label}</span>
    <div class="readout">
      <span class="num readout-val">{Math.round(value)}</span>
      <span class="readout-unit">{unit}</span>
    </div>
  </div>

  <div class="track">
    <div class="fill" class:over style="width: {pct}%;"></div>
    {#if peakPct > 0 && peakPct > pct}
      <div class="peak" style="left: {peakPct}%;"></div>
    {/if}
    <div class="grid"></div>
  </div>

  <div class="ticks num">
    <span>0</span>
    {#each ticks as t}
      <span>{t}</span>
    {/each}
  </div>
</section>

<style>
  .output-meter {
    display: flex;
    flex-direction: column;
    gap: 8px;
    padding: 12px 14px 10px;
    background: var(--color-surface);
    border: 1px solid var(--color-border);
    border-radius: 8px;
    box-shadow: var(--shadow-card);
    height: 100%;
    justify-content: space-between;
  }

  .head {
    display: flex;
    justify-content: space-between;
    align-items: baseline;
  }
  .readout { display: flex; align-items: baseline; gap: 6px; }
  .readout-val {
    font-size: var(--fs-2xl);
    line-height: 0.9;
    font-weight: 600;
    color: var(--color-text-strong);
    text-shadow: 0 0 18px var(--color-accent-glow);
  }
  .readout-unit {
    font-size: var(--fs-md);
    color: var(--color-text-muted);
    text-transform: uppercase;
    letter-spacing: 0.08em;
  }

  .track {
    position: relative;
    height: 22px;
    background: var(--gauge-track);
    border: 1px solid var(--color-border-strong);
    border-radius: 4px;
    overflow: hidden;
  }
  .fill {
    position: absolute;
    inset: 0 auto 0 0;
    background: linear-gradient(180deg, var(--color-accent-hi) 0%, var(--gauge-fill) 55%, var(--color-accent-lo) 100%);
    transition: width 60ms linear;
    box-shadow: inset 0 0 10px var(--color-accent-glow), 0 0 12px var(--color-accent-glow);
  }
  .fill.over {
    background: linear-gradient(180deg, #ff8a87 0%, var(--gauge-fill-over) 55%, #b8302d 100%);
    box-shadow: inset 0 0 10px #ff4d4a44, 0 0 14px #ff4d4a55;
  }
  .peak {
    position: absolute; top: -2px; bottom: -2px;
    width: 2px;
    background: var(--gauge-peak);
    box-shadow: 0 0 6px #ffffff99;
    transform: translateX(-1px);
  }
  .grid {
    position: absolute; inset: 0;
    background-image: repeating-linear-gradient(
      to right,
      transparent 0,
      transparent calc(10% - 1px),
      var(--gauge-tick) calc(10% - 1px),
      var(--gauge-tick) 10%
    );
    opacity: 0.35;
    pointer-events: none;
  }

  .ticks {
    display: flex;
    justify-content: space-between;
    font-size: var(--fs-xs);
    color: var(--color-text-faint);
    padding: 0 2px;
  }
</style>
