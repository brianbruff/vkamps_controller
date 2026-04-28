<script>
  import { onMount, tick } from 'svelte';

  /** @type {{ open: boolean, onclose: () => void }} */
  let { open = $bindable(false), onclose } = $props();

  // ---- State ----
  let entries = $state([]);
  let levelFilter = $state('all');
  let categoryFilter = $state('all');
  let searchTerm = $state('');
  let autoScroll = $state(true);
  let scroller = $state(null);
  let lastSavedPath = $state('');

  let unsub = null;
  let unsubCleared = null;

  const LEVELS = ['all', 'info', 'warn', 'error', 'debug'];
  const CATEGORIES = ['all', 'lifecycle', 'config', 'transport', 'tx', 'rx', 'command', 'error', 'ui'];

  $effect(() => {
    if (open) backfillAndSubscribe();
    else teardown();
  });

  async function backfillAndSubscribe() {
    if (!window.api) return;
    try {
      const all = await window.api.getDiagEntries();
      entries = Array.isArray(all) ? all : [];
    } catch { entries = []; }

    // Subscribe to live entries.
    if (unsub) unsub();
    unsub = window.api.onDiagEntry((e) => {
      // Avoid huge arrays: keep last 5000 in renderer too.
      const next = entries.length >= 5000 ? entries.slice(entries.length - 4999) : entries;
      next.push(e);
      entries = next;
      if (autoScroll) queueScrollPin();
    });
    if (unsubCleared) unsubCleared();
    unsubCleared = window.api.onDiagCleared(() => { entries = []; });

    await tick();
    queueScrollPin();
  }

  function teardown() {
    if (unsub) { unsub(); unsub = null; }
    if (unsubCleared) { unsubCleared(); unsubCleared = null; }
  }

  let pinScheduled = false;
  function queueScrollPin() {
    if (pinScheduled) return;
    pinScheduled = true;
    requestAnimationFrame(() => {
      pinScheduled = false;
      if (autoScroll && scroller) {
        scroller.scrollTop = scroller.scrollHeight;
      }
    });
  }

  function onScroll() {
    if (!scroller) return;
    const atBottom = scroller.scrollTop + scroller.clientHeight >= scroller.scrollHeight - 4;
    if (!atBottom && autoScroll) autoScroll = false;
  }

  function pinToBottom() {
    autoScroll = true;
    queueScrollPin();
  }

  async function onClear() {
    if (!window.api) return;
    await window.api.clearDiag();
    entries = [];
  }

  async function onSave() {
    if (!window.api) return;
    try {
      const p = await window.api.saveDiag();
      if (p) lastSavedPath = p;
    } catch (err) {
      // Surfaced via diagnostic log; just no-op visually.
    }
  }

  function close() {
    open = false;
    onclose?.();
  }

  function onkey(e) {
    if (!open) return;
    if (e.key === 'Escape') { e.preventDefault(); close(); }
  }

  // ---- Display helpers ----
  function fmtTs(iso) {
    try {
      const d = new Date(iso);
      const pad = (n, w = 2) => String(n).padStart(w, '0');
      return `${pad(d.getHours())}:${pad(d.getMinutes())}:${pad(d.getSeconds())}.${pad(d.getMilliseconds(), 3)}`;
    } catch { return iso; }
  }

  function fmtDetail(detail) {
    if (detail == null) return '';
    if (typeof detail !== 'object') return String(detail);
    const parts = [];
    for (const k of Object.keys(detail)) {
      const v = detail[k];
      let s;
      if (v == null) s = String(v);
      else if (typeof v === 'string') s = v;
      else if (Array.isArray(v)) s = v.join(', ');
      else if (typeof v === 'object') { try { s = JSON.stringify(v); } catch { s = String(v); } }
      else s = String(v);
      parts.push(`${k}=${s}`);
    }
    return parts.join('  ');
  }

  const filtered = $derived.by(() => {
    const term = (searchTerm || '').toLowerCase();
    return entries.filter((e) => {
      if (levelFilter !== 'all' && e.level !== levelFilter) return false;
      if (categoryFilter !== 'all' && e.category !== categoryFilter) return false;
      if (term) {
        const a = (e.message || '').toLowerCase();
        const b = e.detail ? (() => { try { return JSON.stringify(e.detail).toLowerCase(); } catch { return ''; } })() : '';
        if (!a.includes(term) && !b.includes(term)) return false;
      }
      return true;
    });
  });
</script>

<svelte:window onkeydown={onkey} />

{#if open}
  <div class="overlay" role="dialog" aria-modal="true" aria-label="Diagnostics">
    <div class="head">
      <span class="title">DIAGNOSTICS</span>
      <span class="counts num">
        {filtered.length} / {entries.length} entries
        {#if lastSavedPath}<span class="saved-path"> · saved → {lastSavedPath}</span>{/if}
      </span>
      <span class="grow"></span>
      <button class="btn" type="button" onclick={onClear}>Clear</button>
      <button class="btn primary" type="button" onclick={onSave}>Save log…</button>
      <button class="btn close" type="button" aria-label="Close" onclick={close}>✕</button>
    </div>

    <div class="filters">
      <label class="f">
        <span class="label">Level</span>
        <select bind:value={levelFilter}>
          {#each LEVELS as lv}<option value={lv}>{lv}</option>{/each}
        </select>
      </label>
      <label class="f">
        <span class="label">Category</span>
        <select bind:value={categoryFilter}>
          {#each CATEGORIES as cat}<option value={cat}>{cat}</option>{/each}
        </select>
      </label>
      <label class="f grow">
        <span class="label">Search</span>
        <input type="text" bind:value={searchTerm} placeholder="filter messages or detail" />
      </label>
      <label class="auto">
        <input type="checkbox" bind:checked={autoScroll} onchange={() => autoScroll && pinToBottom()} />
        <span>Auto-scroll</span>
      </label>
      {#if !autoScroll}
        <button class="btn small" type="button" onclick={pinToBottom}>↓ Bottom</button>
      {/if}
    </div>

    <div class="log" bind:this={scroller} onscroll={onScroll}>
      {#if !filtered.length}
        <div class="empty">No entries match the current filter.</div>
      {/if}
      {#each filtered as e (e.ts + ':' + e.message)}
        <div class="row" data-level={e.level}>
          <span class="ts num">{fmtTs(e.ts)}</span>
          <span class="lv lv-{e.level}">{(e.level || '').toUpperCase()}</span>
          <span class="cat">{(e.category || '').toUpperCase()}</span>
          <span class="msg">{e.message}</span>
        </div>
        {#if e.detail}
          <div class="row detail" data-level={e.level}>
            <span class="ts"></span>
            <span class="lv"></span>
            <span class="cat"></span>
            <span class="msg detail-text">↳ {fmtDetail(e.detail)}</span>
          </div>
        {/if}
      {/each}
    </div>
  </div>
{/if}

<style>
  .overlay {
    position: fixed; inset: 0;
    background: var(--color-bg);
    display: grid;
    grid-template-rows: auto auto 1fr;
    z-index: 200;
    color: var(--color-text);
  }

  .head {
    display: flex;
    align-items: center;
    gap: 12px;
    padding: 10px 14px;
    background: var(--color-bg-elevated);
    border-bottom: 1px solid var(--color-border);
  }
  .title {
    font-family: var(--font-ui);
    font-size: 13px;
    font-weight: 600;
    letter-spacing: 0.18em;
    color: var(--color-text-strong);
  }
  .counts { font-size: 11px; color: var(--color-text-muted); }
  .saved-path { color: var(--color-text-faint); }
  .grow { flex: 1 1 auto; }

  .btn {
    height: 26px;
    padding: 0 10px;
    border: 1px solid var(--color-border-strong);
    border-radius: 4px;
    background: var(--color-bg-elevated);
    color: var(--color-text);
    font-family: var(--font-ui);
    font-size: 11px;
    font-weight: 600;
    letter-spacing: 0.04em;
    cursor: pointer;
  }
  .btn:hover { background: var(--color-surface-hi); }
  .btn.primary {
    background: var(--color-accent-fill);
    border-color: var(--color-accent);
    color: var(--color-text-strong);
  }
  .btn.primary:hover { background: var(--color-accent-lo); }
  .btn.close { padding: 0 8px; }
  .btn.small { height: 22px; padding: 0 8px; font-size: 10px; }

  .filters {
    display: flex;
    align-items: end;
    gap: 12px;
    padding: 8px 14px;
    background: var(--color-surface);
    border-bottom: 1px solid var(--color-border);
  }
  .filters .f { display: flex; flex-direction: column; gap: 4px; }
  .filters .f.grow { flex: 1 1 auto; }
  .filters .label { color: var(--color-text-muted); font-size: 10px; letter-spacing: 0.14em; }
  .filters select, .filters input[type="text"] {
    height: 24px;
    padding: 0 6px;
    border: 1px solid var(--color-border);
    border-radius: 3px;
    background: var(--color-bg);
    color: var(--color-text);
    font-family: var(--font-num);
    font-size: 11px;
  }
  .filters select { min-width: 110px; }
  .filters input[type="text"] { width: 100%; }
  .auto {
    display: inline-flex; align-items: center; gap: 6px;
    color: var(--color-text);
    font-size: 11px;
    margin-bottom: 1px;
  }
  .auto input { accent-color: var(--color-accent); }

  .log {
    overflow-y: auto;
    padding: 4px 0;
    background: var(--color-bg);
    font-family: var(--font-num);
    font-size: 12px;
    line-height: 1.42;
  }
  .empty {
    padding: 14px;
    color: var(--color-text-muted);
    font-style: italic;
    text-align: center;
  }

  .row {
    display: grid;
    grid-template-columns: 96px 56px 96px 1fr;
    gap: 10px;
    padding: 1px 14px;
    align-items: baseline;
    color: var(--color-text);
  }
  .row[data-level="error"] {
    background: #ff4d4a14;
    border-left: 3px solid var(--color-error);
    padding-left: 11px;
  }
  .row[data-level="warn"] {
    background: #f5c97b10;
    border-left: 3px solid var(--color-warn);
    padding-left: 11px;
  }
  .row[data-level="debug"] {
    color: var(--color-text-muted);
  }

  .row .ts { color: var(--color-text-muted); }
  .row .lv {
    font-weight: 700;
    letter-spacing: 0.04em;
    font-size: 11px;
  }
  .lv-info  { color: var(--color-accent-hi); }
  .lv-warn  { color: var(--color-warn); }
  .lv-error { color: var(--color-error); }
  .lv-debug { color: var(--color-text-faint); }
  .row .cat {
    color: var(--color-text-muted);
    font-size: 11px;
    letter-spacing: 0.04em;
  }
  .row .msg {
    color: var(--color-text-strong);
    word-break: break-word;
  }
  .row.detail .msg.detail-text {
    color: var(--color-text-muted);
    font-size: 11px;
    padding-left: 8px;
  }
</style>
