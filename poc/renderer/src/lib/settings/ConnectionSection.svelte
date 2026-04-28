<script>
  /** @type {{ draft: any, ports: any[] }} */
  let { draft = $bindable(), ports = [] } = $props();

  const isUSB = $derived(draft.mode === 'USB');
  const isTCP = $derived(draft.mode === 'TCP' || draft.mode === 'LAN');
  const isUDP = $derived(draft.mode === 'UDP');
</script>

<section class="sect">
  <div class="hd label">Connection</div>

  <div class="radio-row">
    <label class="radio">
      <input type="radio" bind:group={draft.mode} value="USB" />
      <span>USB / Serial</span>
    </label>
    <label class="radio">
      <input type="radio" bind:group={draft.mode} value="TCP" />
      <span>TCP</span>
    </label>
    <label class="radio">
      <input type="radio" bind:group={draft.mode} value="UDP" />
      <span>UDP</span>
    </label>
  </div>

  <div class="row" class:dim={!isUSB}>
    <label>
      <span class="label">COM port</span>
      <select bind:value={draft.comPort}>
        {#if !ports.length}
          <option value="">— no ports detected —</option>
        {:else}
          {#each ports as p}
            <option value={p.path}>{p.path}{p.manufacturer ? ` · ${p.manufacturer}` : ''}</option>
          {/each}
        {/if}
      </select>
    </label>
    <label>
      <span class="label">Baud</span>
      <select bind:value={draft.baudRate}>
        {#each [600, 1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200] as b}
          <option value={b}>{b}</option>
        {/each}
      </select>
    </label>
  </div>

  <div class="row" class:dim={isUSB}>
    <label class="grow">
      <span class="label">IP address</span>
      <input type="text" bind:value={draft.lanIp} />
    </label>
    <label>
      <span class="label">TCP port</span>
      <input type="number" min="1" max="65535" bind:value={draft.tcpPort} />
    </label>
    <label>
      <span class="label">UDP port</span>
      <input type="number" min="1" max="65535" bind:value={draft.udpPort} />
    </label>
  </div>
</section>

<style>
  .sect { display: flex; flex-direction: column; gap: 10px; }
  .hd { color: var(--color-text-strong); font-size: 11px; }

  .radio-row { display: flex; gap: 16px; }

  .row {
    display: flex;
    gap: 10px;
    align-items: flex-end;
  }
  .row label { display: flex; flex-direction: column; gap: 4px; flex: 0 0 auto; }
  .row label.grow { flex: 1 1 auto; min-width: 0; }
  .row.dim { opacity: 0.4; pointer-events: none; }

  input[type="number"] { width: 80px; }
  input[type="text"] { width: 100%; }
  select { min-width: 110px; }

  .radio {
    display: inline-flex;
    align-items: center;
    gap: 6px;
    cursor: pointer;
    color: var(--color-text);
  }
  .radio input { accent-color: var(--color-accent); }
</style>
