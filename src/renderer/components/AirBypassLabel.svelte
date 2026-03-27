<script>
  import { isTransmitting, isBypassed, p9, isConnected } from '../stores/telemetry.js';
  import { appConfig } from '../stores/config.js';

  let bypass = $derived($isBypassed);
  let transmitting = $derived($isTransmitting);
  let connected = $derived($isConnected);

  async function toggleBypass() {
    if (!connected || transmitting) return;
    if (window.amp) {
      if (bypass) {
        await window.amp.sendCommand('22');
      } else {
        await window.amp.sendCommand('21');
      }
    }
  }
</script>

<button
  class="air-label"
  class:transmitting={transmitting}
  class:bypassed={bypass && !transmitting}
  class:idle={!transmitting && !bypass}
  onclick={toggleBypass}
  disabled={!connected}
>
  ON AIR
</button>

<style>
  .air-label {
    background: none;
    border: none;
    font-size: 14px;
    font-weight: 700;
    padding: 4px 12px;
    cursor: pointer;
    font-family: inherit;
    letter-spacing: 1px;
    transition: color 0.15s;
  }
  .air-label:disabled {
    opacity: 0.4;
    cursor: default;
  }
  .air-label.idle {
    color: #39ff14;
  }
  .air-label.transmitting {
    color: #ff3333;
    text-shadow: 0 0 8px rgba(255, 51, 51, 0.6);
  }
  .air-label.bypassed {
    color: #cc44cc;
  }
</style>
