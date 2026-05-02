<script>
  /**
   * Two-segment pill toggle for the header — switches between the full
   * 5-tile panel and the compact kiosk view.
   *
   * @type {{
   *   value: 'full' | 'compact',
   *   onchange?: (next: 'full' | 'compact') => void,
   * }}
   */
  let { value, onchange } = $props();
</script>

<div class="vmt" role="tablist" aria-label="View mode">
  <button type="button"
          role="tab"
          class="seg"
          class:active={value === 'full'}
          aria-selected={value === 'full'}
          onclick={() => onchange?.('full')}>
    Full
  </button>
  <button type="button"
          role="tab"
          class="seg"
          class:active={value === 'compact'}
          aria-selected={value === 'compact'}
          onclick={() => onchange?.('compact')}>
    Compact
  </button>
</div>

<style>
  .vmt {
    display: inline-flex;
    height: 28px;
    padding: 2px;
    border-radius: 999px;
    border: 1px solid rgba(255, 255, 255, 0.35);
    background: rgba(255, 255, 255, 0.08);
    backdrop-filter: blur(4px);
  }
  .seg {
    border: 0;
    background: transparent;
    color: rgba(244, 243, 255, 0.7);
    font-family: var(--font-ui);
    font-size: 11px;
    font-weight: 600;
    letter-spacing: 0.14em;
    padding: 0 14px;
    border-radius: 999px;
    cursor: pointer;
    transition: background 120ms, color 120ms;
  }
  .seg:hover:not(.active) { color: #ffffff; }
  .seg.active {
    background: #ffffff;
    color: #1d1740;
  }
</style>
