<script>
  import { onMount } from 'svelte';
  import { settings, saveSettings } from '../stores/settings.svelte.js';
  import ConnectionSection from './ConnectionSection.svelte';
  import KoefSection from './KoefSection.svelte';
  import VoltageSection from './VoltageSection.svelte';
  import CatSection from './CatSection.svelte';
  import AntennaMapSection from './AntennaMapSection.svelte';
  import OptionsSection from './OptionsSection.svelte';

  /** @type {{ open: boolean, onclose: () => void }} */
  let { open = $bindable(false), onclose } = $props();

  let draft = $state(JSON.parse(JSON.stringify(settings)));
  let ports = $state([]);
  let dialog = $state(null);

  $effect(() => {
    if (open) {
      // Reset draft when reopening
      draft = JSON.parse(JSON.stringify(settings));
      loadPorts();
      // Focus dialog for keyboard
      queueMicrotask(() => dialog?.focus());
    }
  });

  async function loadPorts() {
    if (!window.api) return;
    try { ports = await window.api.listPorts(); }
    catch { ports = []; }
  }

  function close() {
    open = false;
    onclose?.();
  }

  async function save() {
    // Diff to know what protocol commands to fire
    const prevVoltage = settings.voltage;
    const prevCat = settings.cat;
    const prevAOT = settings.alwaysOnTop;
    const prevConn = JSON.stringify({
      mode: settings.mode, lanIp: settings.lanIp, tcpPort: settings.tcpPort,
      udpPort: settings.udpPort, comPort: settings.comPort, baudRate: settings.baudRate,
    });
    const newConn = JSON.stringify({
      mode: draft.mode, lanIp: draft.lanIp, tcpPort: draft.tcpPort,
      udpPort: draft.udpPort, comPort: draft.comPort, baudRate: draft.baudRate,
    });

    await saveSettings(draft);

    if (window.api) {
      // Voltage
      if (draft.voltage !== prevVoltage) {
        const map = { '48': '51', '50': '52', '53.5': '53', '58.3': '54' };
        const cmd = map[draft.voltage];
        if (cmd) await window.api.send(cmd);
      }
      // CAT
      if (draft.cat !== prevCat) {
        const cmd = String(61 + Number(draft.cat));
        await window.api.send(cmd);
      }
      // Always on top
      if (draft.alwaysOnTop !== prevAOT) {
        await window.api.setAlwaysOnTop(!!draft.alwaysOnTop);
      }
      // Connection change → reconnect
      if (newConn !== prevConn) {
        try { await window.api.disconnect(); } catch {}
        try { await window.api.connect(); } catch {}
      }
    }

    close();
  }

  function onkey(e) {
    if (!open) return;
    if (e.key === 'Escape') { e.preventDefault(); close(); }
    else if ((e.key === 's' || e.key === 'S') && (e.metaKey || e.ctrlKey)) {
      e.preventDefault(); save();
    }
  }

  async function importFile() {
    if (window.api) {
      await window.api.importSaveTxt();
      draft = JSON.parse(JSON.stringify(await window.api.getConfig()));
    }
  }
  async function exportFile() { if (window.api) await window.api.exportSaveTxt(); }
</script>

<svelte:window onkeydown={onkey} />

{#if open}
  <div class="backdrop" onclick={close} role="presentation"></div>
  <div class="dialog"
       bind:this={dialog}
       role="dialog"
       aria-modal="true"
       aria-label="Setup"
       tabindex="-1">
    <div class="head">
      <span class="title">SETUP</span>
      <button class="close" type="button" aria-label="Close" onclick={close}>✕</button>
    </div>

    <div class="body">
      <ConnectionSection bind:draft {ports} />
      <hr />
      <KoefSection bind:draft />
      <hr />
      <VoltageSection bind:draft />
      <hr />
      <CatSection bind:draft />
      <hr />
      <AntennaMapSection bind:draft />
      <hr />
      <OptionsSection bind:draft />
      <hr />
      <section class="sect">
        <div class="hd label">Profile</div>
        <div class="row">
          <button class="link-btn" type="button" onclick={importFile}>Import save.txt…</button>
          <button class="link-btn" type="button" onclick={exportFile}>Export save.txt…</button>
        </div>
      </section>
    </div>

    <div class="foot">
      <button class="btn" type="button" onclick={close}>Cancel</button>
      <button class="btn primary" type="button" onclick={save}>Save and Apply</button>
    </div>
  </div>
{/if}

<style>
  .backdrop {
    position: fixed; inset: 0;
    background: #0e1620b3;
    backdrop-filter: blur(8px);
    z-index: 100;
  }
  .dialog {
    position: fixed;
    top: 50%; left: 50%;
    transform: translate(-50%, -50%);
    width: 560px;
    max-width: calc(100vw - 32px);
    max-height: calc(100vh - 32px);
    background: var(--color-surface);
    border: 1px solid var(--color-border-strong);
    border-radius: 8px;
    box-shadow: var(--shadow-card), 0 24px 48px #00000099;
    display: flex;
    flex-direction: column;
    z-index: 101;
    outline: none;
  }
  .head {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 14px 18px;
    border-bottom: 1px solid var(--color-border);
  }
  .title {
    font-family: var(--font-ui);
    font-weight: 600;
    letter-spacing: 0.18em;
    color: var(--color-text-strong);
  }
  .close {
    width: 28px; height: 28px;
    border-radius: 4px;
    color: var(--color-text-muted);
    display: inline-flex; align-items: center; justify-content: center;
    font-size: 14px;
  }
  .close:hover { background: var(--color-surface-hi); color: var(--color-text); }

  .body {
    padding: 14px 18px;
    overflow-y: auto;
    display: flex;
    flex-direction: column;
    gap: 12px;
  }
  .body hr {
    border: none;
    border-top: 1px solid var(--color-border);
    margin: 2px 0;
  }

  .foot {
    display: flex;
    justify-content: flex-end;
    gap: 10px;
    padding: 12px 18px;
    border-top: 1px solid var(--color-border);
  }
  .btn {
    padding: 8px 14px;
    border: 1px solid var(--color-border-strong);
    border-radius: 6px;
    background: var(--color-bg-elevated);
    color: var(--color-text);
  }
  .btn:hover { background: var(--color-surface-hi); }
  .btn.primary {
    background: var(--color-accent-fill);
    border-color: var(--color-accent);
    color: var(--color-text-strong);
  }
  .btn.primary:hover { background: var(--color-accent-lo); }

  .sect { display: flex; flex-direction: column; gap: 10px; }
  .hd { color: var(--color-text-strong); font-size: 11px; }
  .row { display: flex; gap: 10px; }
  .link-btn {
    background: transparent;
    border: 1px dashed var(--color-border-strong);
    border-radius: 4px;
    padding: 6px 10px;
    color: var(--color-accent);
  }
  .link-btn:hover { background: var(--color-accent-glow); }
</style>
