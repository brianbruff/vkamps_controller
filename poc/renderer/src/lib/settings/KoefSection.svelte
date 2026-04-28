<script>
  /** @type {{ draft: any }} */
  let { draft = $bindable() } = $props();
</script>

<section class="sect">
  <div class="hd label">Power / Koef</div>
  <div class="radio-row">
    {#each [600, 1200, 2400] as v}
      <label class="radio">
        <input type="radio" bind:group={draft.koef} value={v} />
        <span>{v} W</span>
      </label>
    {/each}
  </div>
</section>

<style>
  .sect { display: flex; flex-direction: column; gap: 10px; }
  .hd { color: var(--color-text-strong); font-size: 11px; }
  .radio-row { display: flex; gap: 16px; }
  .radio { display: inline-flex; align-items: center; gap: 6px; cursor: pointer; color: var(--color-text); }
  .radio input { accent-color: var(--color-accent); }
</style>
