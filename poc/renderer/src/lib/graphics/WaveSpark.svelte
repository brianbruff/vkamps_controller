<script>
  import { onMount, onDestroy } from 'svelte';

  /** @type {{ amplitude?: number }} */
  let { amplitude = 1 } = $props();

  const W = 120;
  const H = 36;
  let phase = $state(0);

  let id;
  onMount(() => {
    id = setInterval(() => { phase = (phase + 0.4) % 1000; }, 80);
  });
  onDestroy(() => { clearInterval(id); });

  const points = $derived.by(() => {
    const out = [];
    for (let i = 0; i <= W; i += 2) {
      const y = H / 2
        + Math.sin(i * 0.18 + phase) * amplitude * 10
        + Math.sin(i * 0.42 + phase * 1.7) * amplitude * 4;
      out.push(`${i},${y.toFixed(1)}`);
    }
    return out.join(' ');
  });
</script>

<svg viewBox={`0 0 ${W} ${H}`} class="spark" aria-hidden="true">
  <polyline points={points} fill="none" stroke="var(--brand)" stroke-width="1.6" stroke-linecap="round" />
</svg>

<style>
  .spark {
    width: 100%;
    height: 28px;
    display: block;
  }
</style>
