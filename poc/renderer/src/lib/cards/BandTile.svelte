<script>
  /**
   * Band ribbon tile. The list of bands is configurable so it works with the
   * existing 8-slot CAT band table from the firmware.
   *
   * @type {{
   *   bands: string[],
   *   activeIndex: number,
   *   freq?: string,
   *   onselect?: (i: number) => void,
   *   enabled?: boolean,
   * }}
   */
  let {
    bands,
    activeIndex,
    freq = '',
    onselect,
    enabled = true,
  } = $props();

  const activeLabel = $derived(bands[activeIndex] ?? '—');
  const labelText = $derived(activeLabel.replace(/m$/i, ''));
</script>

<div class="tile" class:disabled={!enabled}>
  <div class="head">
    <span class="l">Band</span>
    <div class="v display">{labelText}<span class="u">m</span></div>
  </div>
  {#if freq}
    <div class="sub">{freq}</div>
  {/if}

  <div class="ribbon" style="grid-template-columns: repeat({bands.length}, 1fr);">
    {#each bands as b, i}
      <button type="button"
              class="seg"
              class:on={i === activeIndex}
              disabled={!enabled || !onselect}
              onclick={() => onselect?.(i)}
              title={b}>
        <span class="lab">{b.replace(/m$/i, '')}</span>
      </button>
    {/each}
  </div>
</div>

<style>
  .tile {
    background: var(--paper);
    border: 1px solid var(--hairline);
    border-radius: 14px;
    padding: 16px 18px;
    display: flex;
    flex-direction: column;
    gap: 8px;
    height: 100%;
  }
  .tile.disabled { opacity: 0.6; }

  .head {
    display: flex;
    align-items: center;
    justify-content: space-between;
  }
  .l {
    color: var(--ink-3);
    font-size: 11px;
    letter-spacing: 0.18em;
    text-transform: uppercase;
    font-weight: 600;
  }
  .v {
    font-weight: 700;
    font-size: clamp(26px, 4.5vh, 48px);
    color: var(--ink);
    line-height: 1;
  }
  .v .u {
    font-family: var(--font-ui);
    font-size: 13px;
    color: var(--ink-3);
    margin-left: 4px;
    font-weight: 500;
  }
  .sub {
    font-family: var(--font-num);
    font-size: 11px;
    color: var(--ink-3);
    font-weight: 500;
  }

  .ribbon {
    margin-top: 4px;
    display: grid;
    gap: 4px;
    flex: 1 1 auto;
    min-height: 38px;
    max-height: 80px;
    border-radius: 8px;
    border: 1px solid var(--hairline-2);
    background: var(--paper-2);
    padding: 4px;
    position: relative;
  }
  .seg {
    border-radius: 4px;
    background: var(--brand-4);
    border: none;
    cursor: pointer;
    color: var(--ink-3);
    font-family: var(--font-num);
    font-size: clamp(9px, 1.4vh, 14px);
    font-weight: 600;
    transition: background 120ms, color 120ms;
    display: grid;
    place-items: center;
    line-height: 1;
  }
  .seg:hover:not(:disabled) { background: var(--brand-2); color: #fff; }
  .seg:disabled { cursor: default; }
  .seg.on {
    background: var(--brand);
    color: #fff;
  }
  .lab { display: block; }
</style>
