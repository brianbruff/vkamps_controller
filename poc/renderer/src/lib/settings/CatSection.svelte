<script>
  /** @type {{ draft: any }} */
  let { draft = $bindable() } = $props();
  const opts = [
    { i: 0, name: 'RF' },
    { i: 1, name: 'Icom' },
    { i: 2, name: 'Yaesu' },
    { i: 3, name: 'Kenwood / Flex' },
    { i: 4, name: 'Anan / SunSDR' },
    { i: 5, name: 'Manual' },
  ];
</script>

<section class="sect">
  <div class="hd label">CAT Protocol</div>
  <div class="grid">
    {#each opts as o}
      <label class="radio">
        <input type="radio" bind:group={draft.cat} value={o.i} />
        <span>{o.name}</span>
      </label>
    {/each}
  </div>
</section>

<style>
  .sect { display: flex; flex-direction: column; gap: 10px; }
  .hd { color: var(--color-text-strong); font-size: 11px; }
  .grid {
    display: grid;
    grid-template-columns: repeat(3, 1fr);
    gap: 8px 16px;
  }
  .radio { display: inline-flex; align-items: center; gap: 6px; cursor: pointer; color: var(--color-text); }
  .radio input { accent-color: var(--color-accent); }
</style>
