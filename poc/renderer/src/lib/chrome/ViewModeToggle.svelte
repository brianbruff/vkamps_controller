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
  /* Classic browser-style tabs that sit at the bottom of the header and
     visually attach to the panel below. The active tab uses the panel's
     paper colour so it reads as one continuous surface. */
  .vmt {
    display: inline-flex;
    align-items: flex-end;
    gap: 4px;
    position: relative;
    /* Drop the tab 1px below the header bottom so the active tab's
       bottom edge sits underneath the panel border. */
    margin-bottom: -1px;
  }
  .seg {
    height: 34px;
    padding: 0 22px;
    border: 1px solid rgba(255, 255, 255, 0.18);
    border-bottom: none;
    border-radius: 12px 12px 0 0;
    background: rgba(255, 255, 255, 0.05);
    color: rgba(244, 243, 255, 0.65);
    font-family: var(--font-ui);
    font-size: 11px;
    font-weight: 600;
    letter-spacing: 0.18em;
    text-transform: uppercase;
    cursor: pointer;
    transition: background 120ms, color 120ms, border-color 120ms;
  }
  .seg:hover:not(.active) {
    background: rgba(255, 255, 255, 0.12);
    color: #ffffff;
    border-color: rgba(255, 255, 255, 0.35);
  }
  .seg.active {
    background: var(--paper);
    color: var(--ink-2);
    border-color: var(--hairline);
    /* Slightly taller so the white tongue clearly extends down toward
       the panel below. */
    height: 38px;
    z-index: 2;
  }
</style>
