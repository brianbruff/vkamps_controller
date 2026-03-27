<script>
  let { icon = '', value = '', label = '', unit = '', onclick = null, active = false, clickable = false } = $props();
</script>

<button
  class="info-panel"
  class:clickable
  class:active
  onclick={onclick}
  disabled={!clickable}
>
  <div class="info-value">
    {#if icon}<span class="info-icon">{icon}</span>{/if}
    <span class="val">{value}</span>
    {#if unit}<span class="unit">{unit}</span>{/if}
  </div>
  <div class="info-label">{label}</div>
</button>

<style>
  .info-panel {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    background: linear-gradient(180deg, rgba(12, 18, 12, 0.8) 0%, rgba(6, 10, 6, 0.9) 100%);
    border: 1.5px solid rgba(57, 255, 20, 0.3);
    border-radius: 8px;
    padding: 5px 10px 4px;
    min-width: 86px;
    font-family: inherit;
    cursor: default;
    transition: all 0.15s;
    box-shadow:
      0 0 8px rgba(57, 255, 20, 0.06),
      inset 0 0 12px rgba(57, 255, 20, 0.03);
  }
  .info-panel.clickable { cursor: pointer; }
  .info-panel.clickable:hover:not(:disabled) {
    border-color: rgba(57, 255, 20, 0.5);
    box-shadow: 0 0 12px rgba(57, 255, 20, 0.15);
  }
  .info-panel.active {
    border-color: rgba(57, 255, 20, 0.45);
    box-shadow: 0 0 10px rgba(57, 255, 20, 0.1);
  }
  .info-panel:disabled { opacity: 0.45; }
  .info-value {
    display: flex;
    align-items: baseline;
    gap: 1px;
  }
  .info-icon {
    font-size: 14px;
    color: #39ff14;
    margin-right: 3px;
    filter: drop-shadow(0 0 3px rgba(57, 255, 20, 0.4));
  }
  .val {
    font-size: 36px;
    font-weight: 800;
    color: #39ff14;
    font-family: 'Impact', 'Arial Narrow', sans-serif;
    letter-spacing: 0px;
    text-shadow: 0 0 12px rgba(57, 255, 20, 0.35), 0 0 25px rgba(57, 255, 20, 0.15);
    line-height: 1.05;
  }
  .unit {
    font-size: 14px;
    color: #6a6a6a;
    font-weight: 500;
    margin-left: 1px;
    font-family: -apple-system, sans-serif;
  }
  .info-label {
    font-size: 11px;
    color: #5a5a5a;
    text-transform: uppercase;
    letter-spacing: 0.8px;
    margin-top: 1px;
    font-weight: 500;
    font-family: -apple-system, sans-serif;
  }
</style>
