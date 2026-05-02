<script>
  /**
   * Chip-style status pill. Tones map onto the light theme palette:
   *   ok     → good (green)
   *   tx     → danger (red, on-air)
   *   bypass → brand (indigo)
   *   fan    → brand-soft (indigo tint)
   *   error  → danger
   *   idle   → neutral
   *
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

<button class="chip"
        type="button"
        class:active
        data-tone={tone}
        class:interactive
        disabled={!interactive}
        onclick={interactive ? onclick : undefined}>
  <span class="d"></span>
  <span class="text">{label}</span>
</button>

<style>
  .chip {
    display: inline-flex;
    align-items: center;
    gap: 8px;
    height: 100%;
    flex: 1 1 0;
    min-width: 0;
    padding: 0 16px;
    border-radius: 999px;
    border: 1px solid var(--hairline);
    background: var(--paper);
    color: var(--ink-2);
    font-size: 12px;
    font-weight: 600;
    letter-spacing: 0.06em;
    cursor: default;
    transition: background 120ms, border-color 120ms, color 120ms;
  }
  .chip.interactive { cursor: pointer; }
  .chip.interactive:hover { border-color: var(--brand-2); }
  .chip[disabled] { opacity: 1; }

  .d {
    width: 8px;
    height: 8px;
    border-radius: 50%;
    background: var(--ink-4);
    flex-shrink: 0;
  }

  .text {
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
  }

  /* Tone backgrounds (resting, not 'active') */
  .chip[data-tone="ok"] {
    color: var(--good);
    background: var(--good-tint);
    border-color: var(--good-edge);
  }
  .chip[data-tone="ok"] .d { background: currentColor; animation: pulse 1.6s infinite; }

  .chip[data-tone="tx"] {
    color: var(--danger);
    background: var(--danger-tint);
    border-color: var(--danger-edge);
  }
  .chip[data-tone="tx"] .d { background: currentColor; animation: pulse 1.2s infinite; }

  .chip[data-tone="bypass"] {
    color: var(--brand);
    background: var(--brand-3);
    border-color: var(--brand-4);
  }
  .chip[data-tone="bypass"] .d { background: currentColor; }

  .chip[data-tone="fan"] {
    color: var(--ink-2);
    background: var(--paper);
    border-color: var(--hairline);
  }
  .chip[data-tone="fan"] .d { background: var(--ink-4); }
  .chip[data-tone="fan"].active {
    color: var(--brand);
    background: var(--brand-3);
    border-color: var(--brand-4);
  }
  .chip[data-tone="fan"].active .d { background: currentColor; animation: pulse 1.6s infinite; }

  .chip[data-tone="error"] {
    color: var(--danger);
    background: var(--danger-tint);
    border-color: var(--danger-edge);
  }
  .chip[data-tone="error"] .d { background: currentColor; animation: pulse 1s infinite; }
</style>
