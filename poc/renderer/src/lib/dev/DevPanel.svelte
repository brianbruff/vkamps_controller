<script>
  /** @type {{ mockTx: boolean, onMockTx: (v:boolean)=>void }} */
  let { mockTx = $bindable(false), onMockTx } = $props();

  let open = $state(false);
  let drive = $state(0);          // 0..100, 0 = use sine
  let voltageDrift = $state(0);   // tenths of V, -50..+50
  let injectError = $state(0);    // 0..7
  let forceBypass = $state(false);
  let forceFanFull = $state(false);

  function call(action, payload) {
    if (window.api?.mock) window.api.mock(action, payload);
  }

  function onDrive(e) {
    drive = +e.target.value;
    if (drive === 0) call('drive', null);
    else call('drive', drive / 100);
  }

  function onDrift(e) {
    voltageDrift = +e.target.value;
    call('voltageDrift', voltageDrift);
  }

  function onError(e) {
    injectError = +e.target.value;
    call('injectError', injectError);
  }

  function onBypass(e) {
    forceBypass = e.target.checked;
    call('forceBypass', forceBypass);
  }

  function onFan(e) {
    forceFanFull = e.target.checked;
    call('forceFanFull', forceFanFull);
  }

  function onTx(e) {
    const v = e.target.checked;
    onMockTx?.(v);
  }

  function dropConn() { call('dropConnection', 3000); }
</script>

<div class="dev-panel" class:open>
  <button class="head" type="button" onclick={() => open = !open}>
    <span class="dot"></span>
    <span class="title">DEV — MOCK CONTROLS</span>
    <span class="caret">{open ? '▾' : '▸'}</span>
  </button>

  {#if open}
    <div class="body">
      <label class="row">
        <input type="checkbox" checked={mockTx} onchange={onTx} />
        <span>Mock TX</span>
      </label>

      <label class="row col">
        <span class="label">Output drive override (0% = use sine wave)</span>
        <div class="rng">
          <input type="range" min="0" max="100" step="1" value={drive} oninput={onDrive} />
          <span class="num val">{drive}%</span>
        </div>
      </label>

      <label class="row col">
        <span class="label">Voltage drift (±5 V)</span>
        <div class="rng">
          <input type="range" min="-50" max="50" step="1" value={voltageDrift} oninput={onDrift} />
          <span class="num val">{(voltageDrift / 10).toFixed(1)} V</span>
        </div>
      </label>

      <label class="row">
        <span class="label">Inject error</span>
        <select value={injectError} onchange={onError}>
          {#each [0,1,2,3,4,5,6,7] as i}
            <option value={i}>{i}</option>
          {/each}
        </select>
      </label>

      <label class="row">
        <input type="checkbox" checked={forceBypass} onchange={onBypass} />
        <span>Force bypass</span>
      </label>

      <label class="row">
        <input type="checkbox" checked={forceFanFull} onchange={onFan} />
        <span>Force fan 100%</span>
      </label>

      <button class="link-btn" type="button" onclick={dropConn}>Drop connection (3 s)</button>
    </div>
  {/if}
</div>

<style>
  .dev-panel {
    position: fixed;
    bottom: 12px;
    right: 12px;
    width: 260px;
    background: #0e1620e6;
    border: 1px solid #6b4e1f;
    border-radius: 6px;
    box-shadow: var(--shadow-card), 0 0 18px #00000088;
    backdrop-filter: blur(6px);
    color: var(--color-text);
    font-size: var(--fs-sm);
    z-index: 50;
    overflow: hidden;
  }
  .head {
    display: flex;
    align-items: center;
    gap: 8px;
    width: 100%;
    padding: 8px 10px;
    background: linear-gradient(90deg, #2a1d0a, #3a2912);
    color: #f5c97b;
    border-bottom: 1px solid #6b4e1f;
    font-size: 11px;
    font-weight: 700;
    letter-spacing: 0.18em;
    text-transform: uppercase;
  }
  .head .dot {
    width: 6px; height: 6px; border-radius: 50%;
    background: #f5c97b; box-shadow: 0 0 6px #f5c97b;
  }
  .head .caret { margin-left: auto; }

  .body {
    display: flex;
    flex-direction: column;
    gap: 10px;
    padding: 10px;
  }
  .row { display: flex; align-items: center; gap: 8px; cursor: pointer; }
  .row.col { flex-direction: column; align-items: stretch; gap: 4px; }
  .rng { display: flex; align-items: center; gap: 8px; }
  .rng input[type="range"] { flex: 1 1 auto; accent-color: var(--color-accent); }
  .val { color: var(--color-text-strong); min-width: 48px; text-align: right; font-size: 11px; }

  select, input[type="checkbox"] { accent-color: var(--color-accent); }
  select { padding: 4px 6px; }

  .link-btn {
    background: transparent;
    border: 1px dashed var(--color-border-strong);
    border-radius: 4px;
    padding: 6px 10px;
    color: var(--color-accent);
    text-align: center;
  }
  .link-btn:hover { background: var(--color-accent-glow); }
</style>
