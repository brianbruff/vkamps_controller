<script>
  /**
   * Band ribbon tile. The highlighted segment IS the value display — no
   * separate big readout is shown.
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
</script>

<div class="tile" class:disabled={!enabled}>
  <div class="head">
    <span class="l">Band</span>
    {#if freq}
      <span class="freq">{freq}</span>
    {/if}
  </div>

  <div class="ribbon" style="--band-count: {bands.length};">
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
    padding: 14px 16px;
    display: flex;
    flex-direction: column;
    gap: 8px;
    height: 100%;
    overflow: hidden;
    min-height: 0;
  }
  .tile.disabled { opacity: 0.6; }

  .head {
    display: flex;
    align-items: baseline;
    justify-content: space-between;
    gap: 12px;
    flex: 0 0 auto;
  }
  .l {
    color: var(--ink-3);
    font-size: 11px;
    letter-spacing: 0.18em;
    text-transform: uppercase;
    font-weight: 600;
  }
  .freq {
    font-family: var(--font-num);
    font-size: 12px;
    color: var(--ink-2);
    font-weight: 600;
  }

  .ribbon {
    display: grid;
    grid-template-columns: repeat(var(--band-count, 7), minmax(0, 1fr));
    gap: 6px;
    flex: 1 1 auto;
    min-height: 0;
    border-radius: 10px;
    border: 1px solid var(--hairline-2);
    background: var(--paper-2);
    padding: 6px;
  }
  .seg {
    border-radius: 8px;
    background: var(--paper);
    border: 1px solid var(--hairline-2);
    cursor: pointer;
    color: var(--ink-2);
    font-family: var(--font-num);
    font-weight: 700;
    font-size: 14px;
    transition: background 120ms, color 120ms, border-color 120ms;
    display: grid;
    place-items: center;
    line-height: 1;
    overflow: hidden;
    padding: 4px 2px;
    min-width: 0;
  }
  .seg .lab {
    white-space: nowrap;
    overflow: hidden;
    text-overflow: clip;
    max-width: 100%;
  }
  .seg:hover:not(:disabled) {
    border-color: var(--brand-2);
    color: var(--brand);
  }
  .seg:disabled { cursor: default; }
  .seg.on {
    background: var(--brand);
    border-color: var(--brand);
    color: #fff;
  }
</style>
