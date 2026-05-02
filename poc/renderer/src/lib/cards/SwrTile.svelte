<script>
  import SwrGauge from '../graphics/SwrGauge.svelte';
  import Icon from '../icons/Icon.svelte';

  /** @type {{ value: number }} */
  let { value = 1 } = $props();

  const tone = $derived(value < 1.5 ? 'good' : value < 2 ? 'warn' : 'danger');
  const text = $derived(
    tone === 'good'  ? '● Nominal' :
    tone === 'warn'  ? '● Elevated' :
                       '● High — fold-back'
  );
</script>

<div class="tile">
  <div class="head">
    <span class="l">SWR</span>
    <span class="ico-pill"><Icon name="gauge" size={18} /></span>
  </div>

  <div class="gauge">
    <SwrGauge value={value} />
  </div>

  <div class="v display">{value.toFixed(2)}<span class="u">:1</span></div>
  <div class="sub" data-tone={tone}>{text}</div>
</div>

<style>
  .tile {
    background: var(--paper);
    border: 1px solid var(--hairline);
    border-radius: 14px;
    padding: 16px 18px;
    display: flex;
    flex-direction: column;
    gap: 6px;
    height: 100%;
  }
  .gauge { flex: 1 1 auto; min-height: 0; display: grid; place-items: center; }
  .gauge :global(svg) { width: 100%; height: 100%; max-height: 160px; }
  .head {
    display: flex;
    align-items: center;
    justify-content: space-between;
  }
  .l {
    color: var(--ink-3);
    font-size: 11px;
    letter-spacing: 0.18em;
    text-transform: uppercase;
    font-weight: 600;
  }
  .ico-pill {
    width: 36px; height: 36px;
    border-radius: 10px;
    background: var(--brand-3);
    color: var(--brand);
    display: grid; place-items: center;
  }
  .gauge { margin: -2px 0 4px; }
  .v {
    font-weight: 700;
    font-size: 32px;
    color: var(--ink);
    line-height: 1;
  }
  .v .u {
    font-family: var(--font-ui);
    font-size: 13px;
    color: var(--ink-3);
    margin-left: 4px;
    font-weight: 500;
  }
  .sub {
    font-size: 11px;
    font-family: var(--font-num);
    font-weight: 500;
  }
  .sub[data-tone="good"]   { color: var(--good); }
  .sub[data-tone="warn"]   { color: var(--warn); }
  .sub[data-tone="danger"] { color: var(--danger); }
</style>
