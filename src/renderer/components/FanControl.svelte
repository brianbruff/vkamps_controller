<script>
  import { fanMode, isConnected, isTransmitting, p10 } from '../stores/telemetry.js';
  import { get } from 'svelte/store';

  let cooling = $state(false);

  async function toggleFan() {
    if (!get(isConnected) || get(isTransmitting)) return;
    if (window.amp) {
      if (!cooling) {
        await window.amp.sendCommand('45');
        cooling = true;
      } else {
        await window.amp.sendCommand('46');
        cooling = false;
      }
    }
  }
</script>

<button
  class="fan-label"
  class:active={$p10 === 1}
  onclick={toggleFan}
>
  Fan {$fanMode}
</button>

<style>
  .fan-label {
    background: none;
    border: none;
    font-size: 12px;
    font-weight: 600;
    color: #39ff14;
    cursor: pointer;
    padding: 2px 8px;
    font-family: inherit;
    transition: color 0.15s;
  }
  .fan-label.active {
    color: #cc44cc;
  }
</style>
