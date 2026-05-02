<script>
  /** @type {{ value: number, max?: number }} */
  let { value = 0, max = 60 } = $props();

  const pct = $derived(Math.max(0, Math.min(1, value / max)));
  const fillW = $derived(Math.max(2, 28 * pct));
</script>

<svg viewBox="0 0 40 22" width="36" height="20" aria-hidden="true">
  <rect x="1"  y="3" width="32" height="16" rx="3" fill="var(--paper-3)" stroke="var(--hairline)" />
  <rect x="33" y="8" width="4"  height="6"  rx="1" fill="var(--hairline)" />
  <rect x="3"  y="5" width={fillW} height="12" rx="2" fill="var(--brand)" />
</svg>
