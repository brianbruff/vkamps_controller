<script>
  /** @type {{ draft: any }} */
  let { draft = $bindable() } = $props();
  const opts = ['48', '50', '53.5', '58.3'];
</script>

<section class="sect">
  <div class="hd label">Voltage Reference</div>
  <div class="radio-row">
    {#each opts as v}
      <label class="radio">
        <input type="radio" bind:group={draft.voltage} value={v} />
        <span>{v} V</span>
      </label>
    {/each}
  </div>
</section>

<style>
  .sect { display: flex; flex-direction: column; gap: 10px; }
  .hd { color: var(--color-text-strong); font-size: 11px; }
  .radio-row { display: flex; gap: 16px; flex-wrap: wrap; }
  .radio { display: inline-flex; align-items: center; gap: 6px; cursor: pointer; color: var(--color-text); }
  .radio input { accent-color: var(--color-accent); }
</style>
