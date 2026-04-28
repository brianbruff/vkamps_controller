<script>
  /**
   * @type {{
   *   label: string,
   *   tone?: 'ok' | 'tx' | 'bypass' | 'fan' | 'error' | 'idle',
   *   active?: boolean,
   *   icon?: string,
   *   onclick?: () => void,
   * }}
   */
  let { label, tone = 'idle', active = false, icon = '', onclick } = $props();

  const interactive = $derived(typeof onclick === 'function');
</script>

<button class="pill"
        type="button"
        class:active
        data-tone={tone}
        class:interactive
        disabled={!interactive}
        onclick={interactive ? onclick : undefined}>
  <span class="dot"></span>
  <span class="text">{label}</span>
</button>

<style>
  .pill {
    display: inline-flex;
    align-items: center;
    gap: 8px;
    height: 100%;
    flex: 1 1 0;
    min-width: 0;
    padding: 0 14px;
    border: 1px solid var(--color-border);
    background: var(--color-surface);
    border-radius: 6px;
    color: var(--color-text);
    font-size: var(--fs-sm);
    font-weight: 600;
    text-transform: uppercase;
    letter-spacing: 0.10em;
    cursor: default;
    transition: background 100ms, border-color 100ms;
    box-shadow: var(--shadow-card);
  }
  .pill.interactive { cursor: pointer; }
  .pill.interactive:hover {
    background: var(--color-surface-hi);
    border-color: var(--color-border-strong);
  }
  .pill[disabled] { opacity: 0.85; }

  .dot {
    width: 10px;
    height: 10px;
    border-radius: 50%;
    background: var(--color-text-faint);
    flex-shrink: 0;
  }

  .text { white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }

  /* Tones (idle by default) */
  [data-tone="ok"] .dot       { background: var(--color-ok); box-shadow: 0 0 6px var(--color-ok); }
  [data-tone="tx"] .dot       { background: var(--color-tx); box-shadow: 0 0 6px var(--color-tx); }
  [data-tone="bypass"] .dot   { background: var(--color-bypass); box-shadow: 0 0 6px var(--color-bypass); }
  [data-tone="fan"] .dot      { background: var(--color-accent); box-shadow: 0 0 6px var(--color-accent); }
  [data-tone="error"] .dot    { background: var(--color-error); box-shadow: 0 0 6px var(--color-error); }

  /* Active states invert backgrounds */
  .pill.active[data-tone="tx"]     {
    background: var(--color-tx);
    border-color: var(--color-tx);
    color: #fff;
    box-shadow: 0 0 18px #ff4d4a55, var(--shadow-card);
  }
  .pill.active[data-tone="bypass"] {
    background: var(--color-bypass);
    border-color: var(--color-bypass);
    color: #1a0e22;
  }
  .pill.active[data-tone="fan"]    {
    background: var(--color-accent-fill);
    border-color: var(--color-accent);
    color: var(--color-text-strong);
  }
  .pill.active[data-tone="error"]  {
    background: #5a1614;
    border-color: var(--color-error);
    color: #ffd6d4;
  }
  .pill.active[data-tone="ok"]     {
    border-color: var(--color-ok);
    color: var(--color-text-strong);
  }
</style>
