<script>
  import ConnectControl from './ConnectControl.svelte';
  import Icon from '../icons/Icon.svelte';

  /**
   * @type {{
   *   isMock?: boolean,
   *   onneedsetup?: () => void,
   *   ondiagnostics?: () => void,
   * }}
   */
  let { isMock = false, onneedsetup, ondiagnostics } = $props();
</script>

<header class="header">
  <div class="brand">
    <div class="logo" aria-hidden="true">
      <svg viewBox="0 0 32 32" width="28" height="28">
        <defs>
          <linearGradient id="g" x1="0" x2="0" y1="0" y2="1">
            <stop offset="0%" stop-color="#7cc3ff"/>
            <stop offset="100%" stop-color="#2c7fc4"/>
          </linearGradient>
        </defs>
        <rect x="2" y="2" width="28" height="28" rx="6" fill="#0e1620" stroke="url(#g)" stroke-width="1.5"/>
        <path d="M7 22 L13 10 L16 18 L19 10 L25 22" fill="none" stroke="url(#g)" stroke-width="2.4" stroke-linejoin="round" stroke-linecap="round"/>
      </svg>
    </div>
    <div class="title">
      <div class="brand-name">VKAmp</div>
      <div class="brand-sub label">Ham Radio Amplifier Control</div>
    </div>
  </div>

  <div class="right">
    <button class="diag-btn" type="button" aria-label="Diagnostics" title="Diagnostics" onclick={() => ondiagnostics?.()}>
      <Icon name="log" size={14} />
      <span class="diag-label">DIAG</span>
    </button>
    <span class="poc-badge">POC</span>
    <ConnectControl onneedsetup={onneedsetup} />
  </div>
</header>

<style>
  .header {
    height: 56px;
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 0 16px;
    background: var(--color-bg-elevated);
    border-bottom: 1px solid var(--color-border);
    flex: 0 0 auto;
  }

  .brand { display: flex; align-items: center; gap: 12px; }
  .logo {
    width: 32px; height: 32px;
    display: flex; align-items: center; justify-content: center;
  }

  .brand-name {
    font-family: var(--font-ui);
    font-size: var(--fs-md);
    font-weight: 600;
    color: var(--color-text-strong);
    letter-spacing: 0.02em;
  }
  .brand-sub {
    margin-top: 1px;
    color: var(--color-text-muted);
  }

  .right {
    display: flex;
    align-items: center;
    gap: 10px;
  }

  .diag-btn {
    display: inline-flex;
    align-items: center;
    gap: 5px;
    height: 26px;
    padding: 0 8px;
    border: 1px solid var(--color-border);
    border-radius: 4px;
    background: var(--color-bg-elevated);
    color: var(--color-text);
    font-family: var(--font-ui);
    font-size: 10px;
    font-weight: 600;
    letter-spacing: 0.18em;
    cursor: pointer;
  }
  .diag-btn:hover { background: var(--color-surface-hi); border-color: var(--color-border-strong); }
  .diag-label { color: var(--color-text-muted); }

  .poc-badge {
    display: inline-flex;
    align-items: center;
    height: 22px;
    padding: 0 8px;
    border: 1px solid #6b4e1f;
    background: #3a2912;
    color: #f5c97b;
    font-size: var(--fs-xs);
    font-weight: 700;
    letter-spacing: 0.18em;
    text-transform: uppercase;
    border-radius: 4px;
  }
</style>
