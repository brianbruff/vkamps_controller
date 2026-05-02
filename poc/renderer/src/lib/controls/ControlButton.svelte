<script>
  import Icon from '../icons/Icon.svelte';

  /**
   * Ghost-style control button: white background, hairline border, indigo on
   * hover/active. Active toggles use a soft brand-tint instead of a hard fill.
   *
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
    <Icon name={icon} size={16} stroke="currentColor" />
  {/if}
  <span class="text">{label}</span>
</button>

<style>
  .ctrl {
    background: var(--paper);
    border: 1px solid var(--hairline);
    border-radius: 12px;
    padding: 12px 14px;
    color: var(--ink-2);
    font-family: var(--font-ui);
    font-weight: 600;
    font-size: 14px;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    gap: 10px;
    cursor: pointer;
    width: 100%;
    height: 100%;
    transition: border-color 120ms, color 120ms, background 120ms;
  }
  .ctrl:hover:not([disabled]) {
    border-color: var(--brand-2);
    color: var(--brand);
    background: #fafbff;
  }
  .ctrl[disabled] { opacity: 0.5; cursor: not-allowed; }

  .ctrl.active {
    background: var(--brand-3);
    border-color: var(--brand);
    color: var(--brand);
  }

  .ctrl[data-tone="warn"].active {
    background: var(--warn-tint);
    border-color: var(--warn-edge);
    color: var(--warn);
  }
  .ctrl[data-tone="danger"].active {
    background: var(--danger-tint);
    border-color: var(--danger-edge);
    color: var(--danger);
  }

  .text { white-space: nowrap; }
</style>
