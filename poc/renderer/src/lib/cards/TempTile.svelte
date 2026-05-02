<script>
  import ThermoIcon from '../graphics/ThermoIcon.svelte';

  /**
   * @type {{
   *   value: number,
   *   max?: number,
   *   warnAt?: number,
   *   dangerAt?: number,
   *   unit?: '°C' | '°F',
   * }}
   */
  let { value = 0, max = 120, warnAt = 55, dangerAt = 75, unit = '°C' } = $props();

  const tone = $derived(value < warnAt ? 'good' : value < dangerAt ? 'warn' : 'danger');
  const text = $derived(
    tone === 'good'  ? '● Nominal' :
    tone === 'warn'  ? '● Warm'    :
                       '● Hot'
  );
</script>

<div class="tile">
  <div class="head">
    <span class="l">Heatsink</span>
    <ThermoIcon value={value} max={max} warnAt={warnAt} dangerAt={dangerAt} />
  </div>
  <div class="v display">{Math.round(value)}<span class="u">{unit}</span></div>
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
    justify-content: space-between;
    gap: 8px;
    height: 100%;
  }
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
  .v {
    font-weight: 700;
    font-size: clamp(28px, 5vh, 56px);
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
