<script>
  /** @type {{ draft: any }} */
  let { draft = $bindable() } = $props();
</script>

<section class="sect">
  <div class="hd label">Options</div>
  <div class="grid">
    <label class="check">
      <input type="checkbox" bind:checked={draft.alwaysOnTop} />
      <span>Always on top</span>
    </label>
    <label class="check">
      <input type="checkbox" bind:checked={draft.showFahrenheit} />
      <span>Fahrenheit</span>
    </label>
    <label class="check">
      <input type="checkbox" bind:checked={draft.sound} />
      <span>Sound alerts</span>
    </label>
    <label class="check">
      <input type="checkbox" bind:checked={draft.inputIndicator} />
      <span>Show input indicator</span>
    </label>
    <label class="check">
      <input type="checkbox" bind:checked={draft.verboseRxLogging} />
      <span>Verbose RX logging</span>
    </label>
  </div>
</section>

<style>
  .sect { display: flex; flex-direction: column; gap: 10px; }
  .hd { color: var(--color-text-strong); font-size: 11px; }
  .grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 8px 16px;
  }
  .check { display: inline-flex; align-items: center; gap: 8px; cursor: pointer; color: var(--color-text); }
  .check input { accent-color: var(--color-accent); }
</style>
