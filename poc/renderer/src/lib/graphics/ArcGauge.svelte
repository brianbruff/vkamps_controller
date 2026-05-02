<script>
  import { polar, describeArc } from './arc-math.js';

  /**
   * Half-circle output gauge — sweeps from 180° (left) to 360° (right).
   *
   * @type {{
   *   value: number,
   *   max?: number,
   *   ticks?: number[],
   *   dangerFrom?: number,
   * }}
   */
  let {
    value = 0,
    max = 1200,
    ticks = [0, 200, 400, 600, 800, 1000, 1200],
    dangerFrom = 1100,
  } = $props();

  const W = 360;
  const H = 240;
  const cx = W / 2;
  const cy = H - 30;
  const R = 140;
  const SW = 22;
  const startA = 180;
  const endA = 360;

  const pct = $derived(Math.max(0, Math.min(1, value / max)));
  const valA = $derived(startA + (endA - startA) * pct);
  const dangerStartA = $derived(startA + (endA - startA) * (dangerFrom / max));
  const needle = $derived(polar(cx, cy, R, valA));

  const trackPath = describeArc(cx, cy, R, startA, endA);
  const valuePath = $derived(describeArc(cx, cy, R, startA, valA));
  const dangerPath = $derived(describeArc(cx, cy, R, dangerStartA, endA));
</script>

<svg viewBox={`0 0 ${W} ${H}`} preserveAspectRatio="xMidYMid meet" class="arc">
  <defs>
    <linearGradient id="arc-fill" x1="0" x2="1" y1="0" y2="0">
      <stop offset="0" stop-color="var(--brand)" />
      <stop offset="1" stop-color="var(--brand-2)" />
    </linearGradient>
  </defs>

  <!-- background arc -->
  <path d={trackPath} stroke="var(--paper-3)" stroke-width={SW} fill="none" stroke-linecap="round" />
  <!-- danger zone tint -->
  <path d={dangerPath} stroke="var(--danger-tint)" stroke-width={SW} fill="none" stroke-linecap="round" />
  <!-- value arc -->
  <path d={valuePath} stroke="url(#arc-fill)" stroke-width={SW} fill="none" stroke-linecap="round" />

  <!-- ticks -->
  {#each ticks as t}
    {@const a = startA + (endA - startA) * (t / max)}
    {@const p1 = polar(cx, cy, R + SW / 2 + 4, a)}
    {@const p2 = polar(cx, cy, R + SW / 2 + 12, a)}
    {@const lp = polar(cx, cy, R + SW / 2 + 24, a)}
    {@const isDanger = t >= dangerFrom}
    <g>
      <line x1={p1.x} y1={p1.y} x2={p2.x} y2={p2.y}
            stroke={isDanger ? 'var(--danger)' : 'var(--ink-4)'} stroke-width="1.5" />
      <text x={lp.x} y={lp.y} text-anchor="middle" dominant-baseline="central"
            font-size="11" font-family="var(--font-num)" font-weight="500"
            fill={isDanger ? 'var(--danger)' : 'var(--ink-3)'}>
        {t}
      </text>
    </g>
  {/each}

  <!-- needle dot -->
  <circle cx={needle.x} cy={needle.y} r="8" fill="var(--paper)" stroke="var(--brand)" stroke-width="3" />
</svg>

<style>
  .arc {
    width: 100%;
    height: 100%;
    display: block;
  }
</style>
