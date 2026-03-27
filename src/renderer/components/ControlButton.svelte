<script>
  let { label = '', icon = '', onclick = null, disabled = false, active = false } = $props();
</script>

<button
  class="ctrl-btn"
  class:active
  {disabled}
  {onclick}
>
  {#if icon}<span class="btn-icon">{icon}</span>{/if}
  <span class="btn-label">{label}</span>
</button>

<style>
  .ctrl-btn {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 4px;
    background: linear-gradient(180deg, rgba(10, 16, 10, 0.7) 0%, rgba(5, 8, 5, 0.8) 100%);
    border: 1.5px solid rgba(57, 255, 20, 0.25);
    border-radius: 8px;
    padding: 8px 16px 6px;
    color: #39ff14;
    font-size: 14px;
    font-weight: 600;
    cursor: pointer;
    font-family: -apple-system, sans-serif;
    transition: all 0.15s;
    min-width: 82px;
    letter-spacing: 0.3px;
    box-shadow:
      0 0 6px rgba(57, 255, 20, 0.05),
      inset 0 0 10px rgba(57, 255, 20, 0.02);
  }
  .ctrl-btn:hover:not(:disabled) {
    border-color: rgba(57, 255, 20, 0.45);
    background: linear-gradient(180deg, rgba(15, 25, 15, 0.7) 0%, rgba(8, 14, 8, 0.8) 100%);
    box-shadow: 0 0 12px rgba(57, 255, 20, 0.1);
  }
  .ctrl-btn:active:not(:disabled) { transform: scale(0.96); }
  .ctrl-btn:disabled {
    opacity: 0.25;
    cursor: default;
  }
  .ctrl-btn.active {
    border-color: rgba(57, 255, 20, 0.45);
    box-shadow: 0 0 10px rgba(57, 255, 20, 0.12);
  }
  .btn-icon {
    font-size: 26px;
    line-height: 1;
    filter: drop-shadow(0 0 6px rgba(57, 255, 20, 0.35));
  }
  .btn-label {
    letter-spacing: 0.3px;
    font-size: 12px;
  }
</style>
