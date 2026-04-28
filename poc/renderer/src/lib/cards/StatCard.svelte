<script>
  import Icon from '../icons/Icon.svelte';

  /**
   * @type {{
   *   icon?: string,
   *   value: string | number,
   *   unit?: string,
   *   label: string,
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
    mode = 'readonly',
    active = false,
    enabled = true,
    onactivate,
  } = $props();

  const interactive = $derived(mode !== 'readonly' && enabled && typeof onactivate === 'function');
</script>

<button class="card"
        type="button"
        data-mode={mode}
        class:active
        class:interactive
        class:disabled={!enabled}
        disabled={!interactive}
        onclick={interactive ? onactivate : undefined}>

  <div class="top">
    {#if icon}
      <Icon name={icon} size={14} stroke="var(--color-text-muted)" />
    {/if}
    <span class="label">{label}</span>
  </div>

  <div class="middle">
    <span class="num val">{value}</span>
    {#if unit}<span class="unit">{unit}</span>{/if}
  </div>

  {#if mode === 'cycle' && enabled}
    <div class="chev"><Icon name="chevron" size={12} stroke="var(--color-text-faint)" /></div>
  {/if}
</button>

<style>
  .card {
    position: relative;
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    gap: 4px;
    padding: 8px 10px;
    background: var(--color-surface);
    border: 1px solid var(--color-border);
    border-radius: 6px;
    color: var(--color-text);
    text-align: left;
    height: 100%;
    cursor: default;
    box-shadow: var(--shadow-card);
    transition: background 100ms, border-color 100ms;
  }
  .card.interactive { cursor: pointer; }
  .card.interactive:hover {
    background: var(--color-surface-hi);
    border-color: var(--color-border-strong);
  }
  .card.disabled { opacity: 0.55; }

  .card.active {
    background: var(--color-accent-fill);
    border-color: var(--color-accent);
    color: var(--color-text-strong);
  }

  .top { display: flex; align-items: center; gap: 6px; }
  .middle {
    display: flex;
    align-items: baseline;
    gap: 4px;
  }
  .val {
    font-size: var(--fs-lg);
    font-weight: 600;
    color: var(--color-text-strong);
    line-height: 1.1;
  }
  .unit {
    font-size: var(--fs-xs);
    color: var(--color-text-muted);
    text-transform: uppercase;
    letter-spacing: 0.08em;
  }
  .chev {
    position: absolute;
    right: 6px;
    bottom: 6px;
    opacity: 0.5;
  }
</style>
