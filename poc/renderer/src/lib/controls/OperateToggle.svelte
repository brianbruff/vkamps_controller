<script>
  import Icon from '../icons/Icon.svelte';

  /**
   * Segmented Standby/Operate toggle.
   *
   * @type {{
   *   operate: boolean,
   *   onchange?: (next: boolean) => void,
   *   disabled?: boolean,
   * }}
   */
  let { operate, onchange, disabled = false } = $props();
</script>

<div class="op-toggle" class:disabled>
  <button type="button"
          class="seg standby"
          class:active={!operate}
          {disabled}
          onclick={() => onchange?.(false)}>
    <span class="ico"><Icon name="pause" size={18} /></span>
    <span class="lbl">Standby</span>
    <span class="micro">RF Muted</span>
  </button>
  <button type="button"
          class="seg operate"
          class:active={operate}
          {disabled}
          onclick={() => onchange?.(true)}>
    <span class="ico"><Icon name="power" size={18} /></span>
    <span class="lbl">Operate</span>
    <span class="micro">On Air</span>
  </button>
</div>

<style>
  .op-toggle {
    display: grid;
    grid-template-columns: 1fr 1fr;
    border: 1px solid var(--hairline);
    border-radius: 12px;
    overflow: hidden;
    background: var(--paper);
    height: 100%;
  }
  .op-toggle.disabled { opacity: 0.6; }

  .seg {
    padding: 12px 10px;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 10px;
    font-weight: 600;
    font-size: 14px;
    color: var(--ink-3);
    background: transparent;
    border: none;
    cursor: pointer;
    transition: background 120ms, color 120ms;
  }
  .seg .ico { display: grid; place-items: center; opacity: 0.85; }
  .seg .micro {
    font-size: 10px;
    letter-spacing: 0.18em;
    text-transform: uppercase;
    opacity: 0.7;
    margin-left: 6px;
    font-weight: 600;
  }

  .seg.standby.active {
    background: var(--paper-2);
    color: var(--ink);
  }
  .seg.operate.active {
    background: var(--brand);
    color: #fff;
  }
  .seg.operate.active .micro { opacity: 0.9; }

  .seg:hover:not(:disabled):not(.active) {
    background: var(--paper-2);
    color: var(--ink-2);
  }
  .seg:disabled { cursor: default; }

  @media (max-width: 720px) {
    .seg .micro { display: none; }
  }
</style>
