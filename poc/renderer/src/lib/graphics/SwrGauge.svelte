<script>
  import { polar, describeArc } from './arc-math.js';

  /** @type {{ value: number }} */
  let { value = 1 } = $props();

  const W = 120;
  const H = 70;
  const cx = 60;
  const cy = 60;
  const R = 50;
  const startA = 180;
  const endA = 360;

  const v = $derived(Math.max(1, Math.min(3, value)));
  const pct = $derived((v - 1) / 2);
  const valA = $derived(startA + (endA - startA) * pct);
  const np = $derived(polar(cx, cy, R - 6, valA));
  const color = $derived(v < 1.5 ? 'var(--good)' : v < 2 ? 'var(--warn)' : 'var(--danger)');

  const goodArc = describeArc(cx, cy, R, startA, startA + (endA - startA) * 0.25);
  const warnArc = describeArc(cx, cy, R, startA + (endA - startA) * 0.25, startA + (endA - startA) * 0.5);
  const dangerArc = describeArc(cx, cy, R, startA + (endA - startA) * 0.5, endA);
</script>

<svg viewBox={`0 0 ${W} ${H}`} class="swr">
  <path d={goodArc}   stroke="#cfeadd" stroke-width="6" fill="none" stroke-linecap="round" />
  <path d={warnArc}   stroke="#fbf0d4" stroke-width="6" fill="none" stroke-linecap="round" />
  <path d={dangerArc} stroke="#fbe1e6" stroke-width="6" fill="none" stroke-linecap="round" />
  <line x1={cx} y1={cy} x2={np.x} y2={np.y} stroke={color} stroke-width="2.5" stroke-linecap="round" />
  <circle cx={cx} cy={cy} r="4" fill={color} />
</svg>

<style>
  .swr {
    width: 100%;
    height: 56px;
    display: block;
  }
</style>
