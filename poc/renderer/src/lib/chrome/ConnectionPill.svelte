<script>
  import Icon from '../icons/Icon.svelte';

  /**
   * @type {{ transport: 'USB' | 'TCP' | 'UDP' | 'LAN' | 'MOCK', address: string, status: 'closed' | 'connecting' | 'open' | 'error' }}
   */
  let { transport = 'TCP', address = '', status = 'closed' } = $props();

  const dotClass = $derived(
    status === 'open' ? 'ok'
    : status === 'connecting' ? 'pending'
    : status === 'error' ? 'err'
    : 'idle'
  );

  const iconName = $derived(
    transport === 'USB' ? 'usb'
    : transport === 'MOCK' ? 'bolt'
    : 'wifi'
  );

  const labelText = $derived(
    transport === 'MOCK' ? 'MOCK'
    : transport === 'USB' ? 'USB'
    : transport === 'UDP' ? 'UDP'
    : 'TCP'
  );
</script>

<div class="pill" data-status={status}>
  <span class="dot {dotClass}"></span>
  <Icon name={iconName} size={13} />
  <span class="label-name">{labelText}</span>
  {#if address}
    <span class="addr num">{address}</span>
  {/if}
</div>

<style>
  .pill {
    display: inline-flex;
    align-items: center;
    gap: 7px;
    height: 26px;
    padding: 0 10px;
    border: 1px solid var(--color-border);
    border-radius: 13px;
    background: var(--color-bg-elevated);
    color: var(--color-text);
    font-size: var(--fs-sm);
  }
  .pill[data-status="open"] { border-color: var(--color-accent-lo); }
  .pill[data-status="error"] { border-color: var(--color-error); }

  .dot {
    width: 8px;
    height: 8px;
    border-radius: 50%;
    background: var(--color-text-faint);
    box-shadow: 0 0 0 1px #00000040;
    flex-shrink: 0;
  }
  .dot.ok       { background: var(--color-ok); box-shadow: 0 0 6px var(--color-ok); }
  .dot.pending  { background: var(--color-warn); animation: pulse 1.2s infinite; }
  .dot.err      { background: var(--color-error); }
  .dot.idle     { background: var(--color-text-faint); }

  .label-name {
    font-family: var(--font-ui);
    font-size: 11px;
    font-weight: 600;
    letter-spacing: 0.10em;
    text-transform: uppercase;
    color: var(--color-text-strong);
  }
  .addr {
    font-size: 11px;
    color: var(--color-text-muted);
  }

  @keyframes pulse {
    0%, 100% { opacity: 1; }
    50%      { opacity: 0.45; }
  }
</style>
