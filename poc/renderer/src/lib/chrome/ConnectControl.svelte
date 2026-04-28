<script>
  import Icon from '../icons/Icon.svelte';
  import { settings, saveSettings } from '../stores/settings.svelte.js';
  import { transportState, setStatus, setError } from '../stores/transport.svelte.js';

  /**
   * Live connect/disconnect surface that lives in the header.
   *
   * Props:
   *   onneedsetup — called when user clicks Connect with no required setting
   *                 (e.g. mode=USB but no comPort) so the parent can open the
   *                 SettingsModal at the Connection section.
   */
  let { onneedsetup } = $props();

  // Editable IP draft (not persisted until user presses Connect with a change).
  let ipDraft = $state(settings.lanIp || '');
  let ipDirty = $state(false);

  // When the underlying setting is updated externally (modal save), refresh
  // the draft as long as the user hasn't typed into it.
  $effect(() => {
    if (!ipDirty) ipDraft = settings.lanIp || '';
  });

  const isLan = $derived(settings.mode === 'TCP' || settings.mode === 'UDP' || settings.mode === 'LAN');
  const transportLabel = $derived(
    transportState.isMock ? 'MOCK'
    : settings.mode === 'USB' ? 'USB'
    : settings.mode === 'UDP' ? 'UDP'
    : 'TCP'
  );

  const status = $derived(transportState.status);
  const isOpen = $derived(status === 'open');
  const isConnecting = $derived(status === 'connecting');
  const isError = $derived(status === 'error');

  const dotClass = $derived(
    isOpen ? 'ok' : isConnecting ? 'pending' : isError ? 'err' : 'idle'
  );

  function onIpInput(e) {
    ipDirty = true;
    ipDraft = e.target.value;
  }

  async function doConnect() {
    if (!window.api) return;
    // Gate: USB needs comPort; LAN needs lanIp.
    if (settings.mode === 'USB' && !settings.comPort) {
      window.api.logUi?.('warn', 'Connect blocked: no COM port configured');
      onneedsetup?.();
      return;
    }
    if (isLan && !ipDraft) {
      window.api.logUi?.('warn', 'Connect blocked: no IP configured');
      onneedsetup?.();
      return;
    }

    // Commit draft IP if it changed.
    if (isLan && ipDirty && ipDraft !== settings.lanIp) {
      await saveSettings({ lanIp: ipDraft });
      ipDirty = false;
    }

    window.api.logUi?.('info', 'Connect button clicked', {
      mode: settings.mode,
      target: settings.mode === 'USB'
        ? `${settings.comPort} @ ${settings.baudRate}`
        : `${settings.lanIp}:${settings.tcpPort} (TCP+UDP)`,
    });

    setStatus('connecting');
    try {
      await window.api.connect();
      // The 'connected' event will flip status to 'open' via amp:connection.
    } catch (err) {
      const msg = err?.message || String(err);
      setError(msg);
      window.api.logUi?.('error', `Connect failed: ${msg}`, { message: msg });
    }
  }

  async function doDisconnect() {
    if (!window.api) return;
    window.api.logUi?.('info', 'Disconnect button clicked');
    try {
      await window.api.disconnect();
      setStatus('closed');
    } catch (err) {
      const msg = err?.message || String(err);
      setError(msg);
    }
  }

  function onSubmit(e) {
    e.preventDefault();
    if (isOpen) doDisconnect(); else doConnect();
  }

  const buttonLabel = $derived(
    isOpen ? 'Disconnect'
    : isConnecting ? 'Connecting…'
    : 'Connect'
  );

  const errorTooltip = $derived(transportState.lastError || '');
</script>

<form class="ctl" onsubmit={onSubmit} data-status={status}>
  <span class="dot {dotClass}" title={errorTooltip}></span>

  <span class="mode">
    <Icon name={settings.mode === 'USB' ? 'usb' : 'wifi'} size={13} />
    <span class="mode-label">{transportLabel}</span>
  </span>

  {#if settings.mode === 'USB'}
    <span class="addr num" title={settings.comPort || '(no port)'}>
      {settings.comPort || '— no port —'}
    </span>
  {:else}
    <input
      class="ip num"
      type="text"
      value={ipDraft}
      oninput={onIpInput}
      placeholder="192.168.0.55"
      spellcheck="false"
      autocomplete="off"
      aria-label="IP address" />
    <span class="port num">:{settings.tcpPort}</span>
  {/if}

  <button
    class="btn"
    class:primary={!isOpen}
    class:active={isOpen}
    type="submit"
    disabled={isConnecting}
    aria-label={buttonLabel}>
    {#if isConnecting}
      <span class="spinner" aria-hidden="true"></span>
    {/if}
    <span>{buttonLabel}</span>
  </button>
</form>

<style>
  .ctl {
    display: inline-flex;
    align-items: center;
    gap: 8px;
    height: 30px;
    padding: 0 8px 0 10px;
    border: 1px solid var(--color-border);
    border-radius: 6px;
    background: var(--color-bg-elevated);
    color: var(--color-text);
    font-size: var(--fs-sm);
  }
  .ctl[data-status="open"]    { border-color: var(--color-accent-lo); }
  .ctl[data-status="error"]   { border-color: var(--color-error); }
  .ctl[data-status="connecting"] { border-color: var(--color-warn); }

  .dot {
    width: 8px; height: 8px;
    border-radius: 50%;
    background: var(--color-text-faint);
    box-shadow: 0 0 0 1px #00000040;
    flex-shrink: 0;
  }
  .dot.ok       { background: var(--color-ok); box-shadow: 0 0 6px var(--color-ok); }
  .dot.pending  { background: var(--color-accent); animation: pulse 1.2s infinite; }
  .dot.err      { background: var(--color-error); box-shadow: 0 0 4px var(--color-error); }
  .dot.idle     { background: var(--color-text-faint); }

  .mode {
    display: inline-flex; align-items: center; gap: 5px;
    color: var(--color-text-muted);
  }
  .mode-label {
    font-family: var(--font-ui);
    font-size: 11px;
    font-weight: 600;
    letter-spacing: 0.10em;
    text-transform: uppercase;
    color: var(--color-text-strong);
  }
  .addr { color: var(--color-text-muted); font-size: 11px; }

  .ip {
    width: 130px;
    height: 22px;
    padding: 0 6px;
    border: 1px solid var(--color-border);
    border-radius: 3px;
    background: var(--color-surface);
    color: var(--color-text-strong);
    font-size: 11px;
  }
  .ip:focus {
    outline: none;
    border-color: var(--color-accent);
    background: var(--color-bg);
  }
  .port {
    color: var(--color-text-muted);
    font-size: 11px;
    margin-left: -4px;
  }

  .btn {
    display: inline-flex;
    align-items: center;
    gap: 6px;
    height: 22px;
    padding: 0 10px;
    border: 1px solid var(--color-border-strong);
    border-radius: 3px;
    background: var(--color-bg-elevated);
    color: var(--color-text);
    font-family: var(--font-ui);
    font-size: 11px;
    font-weight: 600;
    letter-spacing: 0.04em;
    cursor: pointer;
  }
  .btn:hover:not(:disabled) {
    background: var(--color-surface-hi);
    border-color: var(--color-border-strong);
  }
  .btn.primary {
    background: var(--color-accent-fill);
    border-color: var(--color-accent);
    color: var(--color-text-strong);
  }
  .btn.primary:hover:not(:disabled) { background: var(--color-accent-lo); }
  .btn.active {
    background: var(--color-bg-elevated);
    border-color: var(--color-accent-lo);
    color: var(--color-text-strong);
  }
  .btn:disabled { cursor: progress; opacity: 0.7; }

  .spinner {
    width: 10px; height: 10px;
    border: 1.5px solid var(--color-text-faint);
    border-top-color: var(--color-accent);
    border-radius: 50%;
    animation: spin 0.8s linear infinite;
  }

  @keyframes pulse {
    0%, 100% { opacity: 1; }
    50%      { opacity: 0.45; }
  }
  @keyframes spin {
    to { transform: rotate(360deg); }
  }
</style>
