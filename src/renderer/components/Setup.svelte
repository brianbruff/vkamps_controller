<script>
  import { appConfig, saveAppConfig } from '../stores/config.js';
  import { get } from 'svelte/store';

  let { onclose = () => {} } = $props();

  let cfg = $state({ ...get(appConfig) });
  let ports = $state([]);
  let loading = $state(true);

  const KOEF_OPTIONS = [600, 1200, 2400];
  const VOLTAGE_OPTIONS = ['48', '50', '53.5', '58.3'];
  const CAT_OPTIONS = [
    { value: 0, label: 'RF' },
    { value: 1, label: 'Icom' },
    { value: 2, label: 'Yaesu' },
    { value: 3, label: 'Kenwood / Flex' },
    { value: 4, label: 'Anan / SunSDR' },
    { value: 5, label: 'Manual' },
  ];
  const BAND_LABELS = ['160', '80', '40', '30', '20', '17-15', '12-10', '6'];

  $effect(() => {
    loadPorts();
  });

  async function loadPorts() {
    if (window.amp) {
      ports = await window.amp.listPorts();
    }
    loading = false;
  }

  async function save() {
    await saveAppConfig(cfg);
    if (window.amp) {
      await window.amp.setAlwaysOnTop(cfg.alwaysOnTop);
    }
    onclose();
  }

  function updateAntennaMap(index, value) {
    const v = parseInt(value);
    if (v >= 0 && v <= 3) {
      cfg.antennaMap = [...cfg.antennaMap];
      cfg.antennaMap[index] = v;
    }
  }

  async function handleImport() {
    if (window.amp) {
      const ok = await window.amp.importSaveTxt();
      if (ok) {
        const newCfg = await window.amp.getConfig();
        cfg = { ...newCfg };
        appConfig.set(newCfg);
      }
    }
  }

  async function handleExport() {
    if (window.amp) {
      await window.amp.exportSaveTxt();
    }
  }
</script>

<div class="setup-overlay" onclick={onclose}>
  <!-- svelte-ignore a11y_click_events_have_key_events -->
  <div class="setup-modal" role="dialog" onclick={(e) => e.stopPropagation()}>
    <div class="setup-header">
      <h2>Setup</h2>
      <button class="close-btn" onclick={onclose}>&times;</button>
    </div>

    <div class="setup-body">
      <!-- Connection Mode -->
      <fieldset class="panel">
        <legend>Connection</legend>
        <div class="radio-group">
          <label class="radio-label">
            <input type="radio" bind:group={cfg.mode} value="USB" />
            USB / Serial
          </label>
          <label class="radio-label">
            <input type="radio" bind:group={cfg.mode} value="LAN" />
            LAN (TCP/UDP)
          </label>
        </div>

        {#if cfg.mode === 'USB'}
          <div class="field">
            <label>COM Port</label>
            <select bind:value={cfg.comPort}>
              <option value="">Select port...</option>
              {#each ports as port}
                <option value={port.path}>{port.path} {port.manufacturer ? `(${port.manufacturer})` : ''}</option>
              {/each}
            </select>
          </div>
          <div class="field">
            <label>Baud Rate</label>
            <select bind:value={cfg.baudRate}>
              <option value={600}>600</option>
              <option value={1200}>1200</option>
              <option value={2400}>2400</option>
            </select>
          </div>
        {:else}
          <div class="field">
            <label>IP Address</label>
            <input type="text" bind:value={cfg.lanIp} placeholder="192.168.0.55" />
          </div>
          <div class="field-row">
            <div class="field">
              <label>TCP Port</label>
              <input type="number" bind:value={cfg.tcpPort} min="1" max="65535" />
            </div>
            <div class="field">
              <label>UDP Port</label>
              <input type="number" bind:value={cfg.udpPort} min="1" max="65535" />
            </div>
          </div>
        {/if}
      </fieldset>

      <!-- Power / Koef -->
      <fieldset class="panel">
        <legend>Power / Koef</legend>
        <div class="radio-group horizontal">
          {#each KOEF_OPTIONS as k}
            <label class="radio-label">
              <input type="radio" bind:group={cfg.koef} value={k} />
              {k}
            </label>
          {/each}
        </div>
      </fieldset>

      <!-- Voltage -->
      <fieldset class="panel">
        <legend>Voltage</legend>
        <div class="radio-group horizontal">
          {#each VOLTAGE_OPTIONS as v}
            <label class="radio-label">
              <input type="radio" bind:group={cfg.voltage} value={v} />
              {v}V
            </label>
          {/each}
        </div>
      </fieldset>

      <!-- CAT Protocol -->
      <fieldset class="panel">
        <legend>CAT Protocol</legend>
        <div class="radio-group">
          {#each CAT_OPTIONS as opt}
            <label class="radio-label">
              <input type="radio" bind:group={cfg.cat} value={opt.value} />
              {opt.label}
            </label>
          {/each}
        </div>
      </fieldset>

      <!-- Antenna Map -->
      <fieldset class="panel">
        <legend>Antenna Map (per band)</legend>
        <div class="antenna-grid">
          {#each BAND_LABELS as band, i}
            <div class="antenna-field">
              <label>{band}m</label>
              <input
                type="number"
                min="0"
                max="3"
                value={cfg.antennaMap[i]}
                oninput={(e) => updateAntennaMap(i, e.target.value)}
              />
            </div>
          {/each}
        </div>
      </fieldset>

      <!-- Options -->
      <fieldset class="panel">
        <legend>Options</legend>
        <div class="checkbox-group">
          <label><input type="checkbox" bind:checked={cfg.alwaysOnTop} /> Always On Top</label>
          <label><input type="checkbox" checked={cfg.tempUnit === 'F'} onchange={(e) => cfg.tempUnit = e.target.checked ? 'F' : 'C'} /> Fahrenheit</label>
          <label><input type="checkbox" bind:checked={cfg.sound} /> Sound Alerts</label>
          <label><input type="checkbox" bind:checked={cfg.inputIndicator} /> Input Indicator</label>
        </div>
      </fieldset>

      <!-- Import / Export -->
      <div class="import-export">
        <button class="secondary-btn" onclick={handleImport}>Import save.txt</button>
        <button class="secondary-btn" onclick={handleExport}>Export save.txt</button>
      </div>
    </div>

    <div class="setup-footer">
      <button class="save-btn" onclick={save}>Save and Exit</button>
    </div>
  </div>
</div>

<style>
  .setup-overlay {
    position: fixed;
    inset: 0;
    background: rgba(0, 0, 0, 0.75);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 100;
  }
  .setup-modal {
    background: #0d0d0d;
    border: 1px solid #2a4a2a;
    border-radius: 10px;
    width: 520px;
    max-height: 90vh;
    overflow-y: auto;
    box-shadow: 0 0 40px rgba(57, 255, 20, 0.1);
  }
  .setup-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 12px 16px;
    border-bottom: 1px solid #1a3a1a;
  }
  .setup-header h2 {
    margin: 0;
    font-size: 16px;
    color: #39ff14;
    font-weight: 600;
  }
  .close-btn {
    background: none;
    border: none;
    color: #666;
    font-size: 22px;
    cursor: pointer;
    padding: 0 4px;
    line-height: 1;
  }
  .close-btn:hover { color: #ff3333; }

  .setup-body {
    padding: 12px 16px;
    display: flex;
    flex-direction: column;
    gap: 10px;
  }
  .panel {
    border: 1px solid #1a3a1a;
    border-radius: 6px;
    padding: 10px 12px;
    margin: 0;
  }
  legend {
    color: #39ff14;
    font-size: 11px;
    text-transform: uppercase;
    letter-spacing: 0.5px;
    padding: 0 6px;
  }
  .radio-group {
    display: flex;
    flex-direction: column;
    gap: 4px;
  }
  .radio-group.horizontal {
    flex-direction: row;
    gap: 16px;
  }
  .radio-label {
    color: #ccc;
    font-size: 12px;
    display: flex;
    align-items: center;
    gap: 6px;
    cursor: pointer;
  }
  .field {
    margin-top: 8px;
  }
  .field label {
    display: block;
    font-size: 11px;
    color: #888;
    margin-bottom: 3px;
  }
  .field input, .field select {
    width: 100%;
    background: #111;
    border: 1px solid #333;
    border-radius: 4px;
    color: #ccc;
    padding: 5px 8px;
    font-size: 12px;
    font-family: inherit;
  }
  .field-row {
    display: flex;
    gap: 10px;
  }
  .field-row .field { flex: 1; }

  .antenna-grid {
    display: grid;
    grid-template-columns: repeat(4, 1fr);
    gap: 6px;
    margin-top: 4px;
  }
  .antenna-field {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 2px;
  }
  .antenna-field label {
    font-size: 10px;
    color: #888;
  }
  .antenna-field input {
    width: 36px;
    text-align: center;
    background: #111;
    border: 1px solid #333;
    border-radius: 4px;
    color: #ccc;
    padding: 3px;
    font-size: 12px;
  }

  .checkbox-group {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 4px;
  }
  .checkbox-group label {
    color: #ccc;
    font-size: 12px;
    display: flex;
    align-items: center;
    gap: 6px;
    cursor: pointer;
  }

  .import-export {
    display: flex;
    gap: 8px;
  }
  .secondary-btn {
    flex: 1;
    background: #1a1a1a;
    border: 1px solid #333;
    color: #888;
    padding: 6px;
    border-radius: 4px;
    font-size: 11px;
    cursor: pointer;
    font-family: inherit;
  }
  .secondary-btn:hover {
    border-color: #39ff14;
    color: #ccc;
  }

  .setup-footer {
    padding: 12px 16px;
    border-top: 1px solid #1a3a1a;
    display: flex;
    justify-content: flex-end;
  }
  .save-btn {
    background: linear-gradient(180deg, #1a4a1a, #0d2a0d);
    border: 1px solid #39ff14;
    color: #39ff14;
    padding: 8px 24px;
    border-radius: 6px;
    font-size: 13px;
    font-weight: 600;
    cursor: pointer;
    font-family: inherit;
    transition: all 0.15s;
  }
  .save-btn:hover {
    background: linear-gradient(180deg, #2a5a2a, #1a3a1a);
    box-shadow: 0 0 12px rgba(57, 255, 20, 0.2);
  }
</style>
