<script>
  import Icon from '../icons/Icon.svelte';
  import WaveSpark from '../graphics/WaveSpark.svelte';

  /**
   * Mini meter card. Shows: icon-pill + label + sub line, large value,
   * thin progress bar (with warn/danger tint), footer scale (0/mid/max).
   *
   * @type {{
   *   label: string,
   *   value: number,
   *   max: number,
   *   unit?: string,
   *   peakHold?: number,
   *   ticks?: number[],
   *   overThreshold?: number,
   *   warnThreshold?: number,
   *   compact?: boolean,
   *   icon?: string,
   *   sub?: string,
   *   spark?: number | null,
   *   decimals?: number,
   * }}
   */
  let {
    label,
    value,
    max,
    unit = '',
    peakHold = 0,
    ticks = null,
    overThreshold = 0.95,
    warnThreshold = 0.8,
    compact = false,
    icon = '',
    sub = '',
    spark = null,
    decimals = 0,
  } = $props();

  const pct = $derived(Math.max(0, Math.min(100, (value / max) * 100)));
  const peakPct = $derived(peakHold ? Math.max(0, Math.min(100, (peakHold / max) * 100)) : 0);
  const tone = $derived(pct >= overThreshold * 100 ? 'danger' : pct >= warnThreshold * 100 ? 'warn' : '');

  const displayValue = $derived(decimals > 0 ? value.toFixed(decimals) : Math.round(value));
  const footTicks = $derived(ticks && ticks.length ? [0, ticks[Math.floor(ticks.length / 2)] ?? max / 2, max] : [0, Math.round(max / 2), max]);
</script>

<div class="meter card" class:compact>
  <div class="head">
    <div class="titlerow">
      {#if icon}
        <div class="ico-wrap">
          <Icon name={icon} size={18} stroke="currentColor" />
        </div>
      {/if}
      <div class="meta">
        <div class="card-label">{label}</div>
        {#if sub}
          <div class="sub">{sub}</div>
        {:else if spark != null}
          <div class="sub-spark">
            <WaveSpark amplitude={spark} />
          </div>
        {/if}
      </div>
    </div>
    <div class="val display">{displayValue}{#if unit}<span class="u">{unit}</span>{/if}</div>
  </div>

  <div class="mb {tone}">
    <i style="width: {pct}%"></i>
    {#if peakPct > 0 && peakPct > pct}
      <em class="peak" style="left: {peakPct}%"></em>
    {/if}
  </div>

  <div class="mb-foot num">
    {#each footTicks as t, i}
      <span>{t}{i === footTicks.length - 1 && unit ? ' ' + unit : ''}</span>
    {/each}
  </div>
</div>

<style>
  .card {
    background: var(--paper);
    border: 1px solid var(--hairline);
    border-radius: 14px;
    padding: 16px 18px;
    display: flex;
    flex-direction: column;
    gap: 12px;
    height: 100%;
  }
  .meter.compact { padding: 12px 14px; gap: 8px; }

  .head {
    display: flex;
    align-items: flex-start;
    justify-content: space-between;
    gap: 12px;
  }
  .titlerow {
    display: flex;
    align-items: center;
    gap: 10px;
    min-width: 0;
  }
  .ico-wrap {
    width: 34px; height: 34px;
    border-radius: 9px;
    background: var(--brand-3);
    color: var(--brand);
    display: grid;
    place-items: center;
    flex-shrink: 0;
  }
  .meta { min-width: 0; }
  .card-label {
    font-size: 11px;
    color: var(--ink-3);
    letter-spacing: 0.18em;
    text-transform: uppercase;
    font-weight: 600;
  }
  .sub {
    font-family: var(--font-num);
    font-size: 11px;
    color: var(--ink-3);
    margin-top: 2px;
    font-weight: 500;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
  }
  .sub-spark {
    margin-top: 4px;
    width: 110px;
  }

  .val {
    font-weight: 700;
    font-size: 38px;
    color: var(--ink);
    line-height: 1;
    white-space: nowrap;
  }
  .compact .val { font-size: 24px; }
  .val .u {
    font-family: var(--font-ui);
    font-size: 13px;
    color: var(--ink-3);
    margin-left: 4px;
    font-weight: 500;
  }

  .mb {
    position: relative;
    flex: 1 1 auto;
    min-height: 12px;
    max-height: 64px;
    border-radius: 999px;
    background: var(--paper-2);
    border: 1px solid var(--hairline-2);
    overflow: hidden;
  }
  .mb i {
    position: absolute;
    left: 0; top: 0; bottom: 0;
    background: var(--brand);
    border-radius: inherit;
    transition: width 200ms cubic-bezier(.4,0,.2,1);
  }
  .mb.warn i   { background: var(--warn); }
  .mb.danger i { background: var(--danger); }
  .mb .peak {
    position: absolute;
    top: -1px; bottom: -1px;
    width: 2px;
    background: var(--ink);
    transform: translateX(-1px);
  }

  .mb-foot {
    display: flex;
    justify-content: space-between;
    font-size: 10px;
    color: var(--ink-3);
    font-weight: 500;
  }
</style>
