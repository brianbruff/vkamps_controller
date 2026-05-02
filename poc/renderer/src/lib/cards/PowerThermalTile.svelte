<script>
  import VoltIcon from '../graphics/VoltIcon.svelte';
  import ThermoIcon from '../graphics/ThermoIcon.svelte';

  /**
   * Combined Plate-voltage + Heatsink-temp tile. Volts side is tappable to
   * toggle the HV+ rail; temp side is read-only.
   *
   * @type {{
   *   volts: number,
   *   voltsMax?: number,
   *   plus?: boolean,
   *   onvolts?: () => void,
   *   temp: number,
   *   tempUnit?: '°C' | '°F',
   *   tempWarnAt?: number,
   *   tempDangerAt?: number,
   *   tempMax?: number,
   * }}
   */
  let {
    volts = 0,
    voltsMax = 60,
    plus = false,
    onvolts,
    temp = 0,
    tempUnit = '°C',
    tempWarnAt = 55,
    tempDangerAt = 75,
    tempMax = 120,
  } = $props();

  const voltsInteractive = $derived(typeof onvolts === 'function');
  const tempTone = $derived(
    temp < tempWarnAt ? 'good' :
    temp < tempDangerAt ? 'warn' : 'danger'
  );
  const tempText = $derived(
    tempTone === 'good' ? 'Nominal' :
    tempTone === 'warn' ? 'Warm'    : 'Hot'
  );
</script>

<div class="tile">
  <button class="cell volts"
          type="button"
          class:active={plus}
          class:interactive={voltsInteractive}
          disabled={!voltsInteractive}
          onclick={voltsInteractive ? onvolts : undefined}>
    <div class="head">
      <span class="l">Plate V{plus ? '+' : ''}</span>
      <VoltIcon value={volts} max={voltsMax} />
    </div>
    <div class="v display">{volts.toFixed(1)}<span class="u">V</span></div>
    <div class="sub">HV+ rail</div>
  </button>

  <div class="divider" aria-hidden="true"></div>

  <div class="cell temp">
    <div class="head">
      <span class="l">Heatsink</span>
      <ThermoIcon value={temp} max={tempMax} warnAt={tempWarnAt} dangerAt={tempDangerAt} />
    </div>
    <div class="v display">{Math.round(temp)}<span class="u">{tempUnit}</span></div>
    <div class="sub" data-tone={tempTone}>● {tempText}</div>
  </div>
</div>

<style>
  .tile {
    background: var(--paper);
    border: 1px solid var(--hairline);
    border-radius: 14px;
    display: grid;
    grid-template-columns: 1fr 1px 1fr;
    height: 100%;
    overflow: hidden;
    min-height: 0;
  }
  .divider {
    background: var(--hairline);
  }

  .cell {
    background: transparent;
    border: 0;
    text-align: left;
    color: var(--ink-2);
    padding: 14px 16px;
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    gap: 6px;
    min-width: 0;
    cursor: default;
    transition: background 120ms;
  }
  .cell.interactive { cursor: pointer; }
  .cell.interactive:hover { background: var(--paper-2); }
  .cell.active { background: var(--brand-3); }

  .head {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 8px;
  }
  .l {
    color: var(--ink-3);
    font-size: 11px;
    letter-spacing: 0.16em;
    text-transform: uppercase;
    font-weight: 600;
    white-space: nowrap;
  }
  .v {
    font-weight: 700;
    font-size: 30px;
    color: var(--ink);
    line-height: 1;
  }
  .v .u {
    font-family: var(--font-ui);
    font-size: 12px;
    color: var(--ink-3);
    margin-left: 4px;
    font-weight: 500;
  }
  .sub {
    font-family: var(--font-num);
    font-size: 11px;
    color: var(--ink-3);
    font-weight: 500;
  }
  .sub[data-tone="good"]   { color: var(--good); }
  .sub[data-tone="warn"]   { color: var(--warn); }
  .sub[data-tone="danger"] { color: var(--danger); }
</style>
