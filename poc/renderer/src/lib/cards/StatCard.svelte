<script>
  import Icon from '../icons/Icon.svelte';

  /**
   * Generic secondary tile. Used for simple readouts (e.g. Efficiency).
   *
   * @type {{
   *   icon?: string,
   *   value: string | number,
   *   unit?: string,
   *   label: string,
   *   sub?: string,
   *   mode?: 'readonly' | 'cycle' | 'toggle',
   *   active?: boolean,
   *   enabled?: boolean,
   *   onactivate?: () => void,
   * }}
   */
  let {
    icon = '',
    value,
    unit = '',
    label,
    sub = '',
    mode = 'readonly',
    active = false,
    enabled = true,
    onactivate,
  } = $props();

  const interactive = $derived(mode !== 'readonly' && enabled && typeof onactivate === 'function');
</script>

<button class="tile"
        type="button"
        data-mode={mode}
        class:active
        class:interactive
        class:disabled={!enabled}
        disabled={!interactive}
        onclick={interactive ? onactivate : undefined}>

  <div class="head">
    <span class="l">{label}</span>
    {#if icon}
      <span class="ico-pill"><Icon name={icon} size={18} /></span>
    {/if}
  </div>

  <div class="v display">{value}{#if unit}<span class="u">{unit}</span>{/if}</div>
  {#if sub}<div class="sub">{sub}</div>{/if}
</button>

<style>
  .tile {
    background: var(--paper);
    border: 1px solid var(--hairline);
    border-radius: 14px;
    padding: 16px 18px;
    display: flex;
    flex-direction: column;
    gap: 8px;
    text-align: left;
    cursor: default;
    color: var(--ink-2);
    transition: border-color 120ms, background 120ms;
    height: 100%;
  }
  .tile.interactive { cursor: pointer; }
  .tile.interactive:hover { border-color: var(--brand-2); }
  .tile.disabled { opacity: 0.55; }

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
  .v {
    font-weight: 700;
    font-size: 28px;
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
    font-family: var(--font-num);
    font-size: 11px;
    color: var(--ink-3);
    font-weight: 500;
  }

  .tile.active {
    background: var(--brand-3);
    border-color: var(--brand);
  }
</style>
