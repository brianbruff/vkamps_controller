<script>
  /** @type {{ value: number, max?: number, warnAt?: number, dangerAt?: number }} */
  let { value = 0, max = 120, warnAt = 55, dangerAt = 75 } = $props();

  const pct = $derived(Math.max(0, Math.min(1, value / max)));
  const fillH = $derived(pct * 30);
  const color = $derived(
    value < warnAt ? 'var(--good)' :
    value < dangerAt ? 'var(--warn)' : 'var(--danger)'
  );
</script>

<svg viewBox="0 0 24 56" width="22" height="48" aria-hidden="true">
  <rect x="9" y="4" width="6" height="38" rx="3" fill="var(--paper-3)" stroke="var(--hairline)" />
  <circle cx="12" cy="46" r="7" fill={color} />
  <rect x="10" y={42 - fillH} width="4" height={fillH + 4} rx="2" fill={color} />
</svg>
