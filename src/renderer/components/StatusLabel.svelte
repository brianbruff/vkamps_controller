<script>
  import { errorText, hasError, errorCode } from '../stores/telemetry.js';

  async function handleClick() {
    if ($hasError && window.amp) {
      await window.amp.sendCommand('23');
    }
  }
</script>

<button
  class="status-label"
  class:error={$hasError}
  onclick={handleClick}
  title={$hasError ? 'Click to reset error' : ''}
>
  {$errorText}
</button>

<style>
  .status-label {
    background: none;
    border: none;
    font-size: 12px;
    font-weight: 600;
    color: #39ff14;
    cursor: default;
    padding: 2px 8px;
    font-family: inherit;
  }
  .status-label.error {
    color: #ff3333;
    cursor: pointer;
    animation: blink 1s ease-in-out infinite;
  }
  @keyframes blink {
    50% { opacity: 0.5; }
  }
</style>
