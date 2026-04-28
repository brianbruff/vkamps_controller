<script>
  import Icon from '../icons/Icon.svelte';

  /**
   * @type {{
   *   icon?: string,
   *   label: string,
   *   kind?: 'momentary' | 'toggle' | 'modal',
   *   active?: boolean,
   *   tone?: 'default' | 'warn' | 'danger',
   *   disabled?: boolean,
   *   onclick?: () => void,
   * }}
   */
  let { icon = '', label, kind = 'momentary', active = false, tone = 'default', disabled = false, onclick } = $props();
</script>

<button class="ctrl"
        type="button"
        data-kind={kind}
        data-tone={tone}
        class:active
        {disabled}
        onclick={onclick}>
  {#if icon}
    <Icon name={icon} size={16} stroke={active ? 'var(--color-text-strong)' : 'currentColor'} />
  {/if}
  <span class="text">{label}</span>
</button>

<style>
  .ctrl {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    gap: 8px;
    width: 100%;
    height: 100%;
    padding: 0 14px;
    border: 1px solid var(--color-border);
    background: transparent;
    color: var(--color-text);
    border-radius: 6px;
    font-size: var(--fs-base);
    font-weight: 500;
    letter-spacing: 0.04em;
    transition: background 100ms, border-color 100ms, color 100ms;
    box-shadow: var(--shadow-card);
  }
  .ctrl:hover:not([disabled]) {
    background: var(--color-surface-hi);
    border-color: var(--color-border-strong);
  }
  .ctrl:active:not([disabled]) {
    background: var(--color-accent-fill);
  }
  .ctrl[disabled] { opacity: 0.5; cursor: not-allowed; }

  .ctrl.active {
    background: var(--color-accent-fill);
    border-color: var(--color-accent);
    color: var(--color-text-strong);
  }

  .ctrl[data-tone="warn"].active {
    background: #4a3a14;
    border-color: var(--color-warn);
    color: var(--color-warn);
  }
  .ctrl[data-tone="danger"].active {
    background: #5a1614;
    border-color: var(--color-tx);
    color: #ffd6d4;
  }

  .text { white-space: nowrap; }
</style>
