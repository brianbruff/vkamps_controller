<script>
  import VoltIcon from '../graphics/VoltIcon.svelte';

  /**
   * @type {{
   *   value: number,
   *   max?: number,
   *   plus?: boolean,
   *   ontoggle?: () => void,
   *   sub?: string,
   * }}
   */
  let { value = 0, max = 60, plus = false, ontoggle, sub = 'HV+ rail' } = $props();

  const interactive = $derived(typeof ontoggle === 'function');
</script>

<button class="tile"
        type="button"
        class:active={plus}
        class:interactive
        disabled={!interactive}
        onclick={interactive ? ontoggle : undefined}>
  <div class="head">
    <span class="l">Plate V{plus ? '+' : ''}</span>
    <VoltIcon value={value} max={max} />
  </div>

  <div class="v display">{value.toFixed(1)}<span class="u">V</span></div>
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
    justify-content: space-between;
    gap: 8px;
    text-align: left;
    color: var(--ink-2);
    cursor: default;
    transition: border-color 120ms, background 120ms;
    height: 100%;
  }
  .tile.interactive { cursor: pointer; }
  .tile.interactive:hover { border-color: var(--brand-2); }
  .tile.active { background: var(--brand-3); border-color: var(--brand); }

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
    font-family: var(--font-num);
    font-size: 11px;
    color: var(--ink-3);
    font-weight: 500;
  }
</style>
