<script>
  /** @type {{ draft: any }} */
  let { draft = $bindable() } = $props();

  const labels = ['160m', '80m', '40m', '30m', '20m', '17m', '12m', '6m'];
</script>

<section class="sect">
  <div class="hd label">Antenna Map <span class="hint">(per band, 1–4)</span></div>
  <div class="grid">
    {#each labels as bnd, i}
      <label class="cell">
        <span class="label">{bnd}</span>
        <input type="number" min="1" max="4" step="1" bind:value={draft.antennaMap[i]} />
      </label>
    {/each}
  </div>
</section>

<style>
  .sect { display: flex; flex-direction: column; gap: 10px; }
  .hd { color: var(--color-text-strong); font-size: 11px; }
  .hint { color: var(--color-text-muted); letter-spacing: 0; text-transform: none; font-weight: 400; margin-left: 6px; }
  .grid {
    display: grid;
    grid-template-columns: repeat(4, 1fr);
    gap: 8px;
  }
  .cell { display: flex; flex-direction: column; gap: 4px; }
  input { width: 100%; }
</style>
