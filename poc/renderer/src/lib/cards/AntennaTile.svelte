<script>
  import AntennaIcon from '../graphics/AntennaIcon.svelte';

  const KIND_BY_INDEX = { 1: 'doublet', 2: 'dipole', 3: 'yagi', 4: 'yagi' };

  /**
   * @type {{
   *   active: number,
   *   labels?: Record<number, string>,
   *   showLabels?: boolean,
   *   onselect?: (n: number) => void,
   *   ports?: number[],
   * }}
   */
  let {
    active,
    labels = { 1: 'Doublet', 2: 'Dipole', 3: '11-el Yagi', 4: 'Aux' },
    showLabels = true,
    onselect,
    ports = [1, 2, 3, 4],
  } = $props();
</script>

<div class="tile">
  <div class="head">
    <span class="l">Antenna Selector</span>
    <span class="sub">{ports.length} ports</span>
  </div>

  <div class="ant-row" style="grid-template-columns: repeat({ports.length}, 1fr);">
    {#each ports as n}
      <button type="button"
              class="a"
              class:on={active === n}
              onclick={() => onselect?.(n)}>
        <span class="icon">
          <AntennaIcon kind={KIND_BY_INDEX[n] || 'doublet'} />
        </span>
        <span class="num display">{n}</span>
        {#if showLabels && labels[n]}<span class="nm">{labels[n]}</span>{/if}
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
  .sub {
    font-family: var(--font-num);
    font-size: 11px;
    color: var(--ink-3);
    font-weight: 500;
  }

  .ant-row {
    display: grid;
    gap: 8px;
    margin-top: 4px;
    flex: 1 1 auto;
    min-height: 0;
  }
  .a {
    text-align: center;
    padding: 10px 6px 8px;
    border-radius: 10px;
    border: 1px solid var(--hairline);
    background: var(--paper);
    color: var(--ink-2);
    cursor: pointer;
    transition: border-color 120ms, background 120ms, color 120ms;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    gap: 4px;
  }
  .a:hover { border-color: var(--brand-2); }
  .a.on {
    background: var(--brand-3);
    border-color: var(--brand);
    color: var(--brand);
  }
  .a .icon {
    height: clamp(22px, 3vh, 36px);
    display: grid;
    place-items: center;
    color: currentColor;
  }
  .a .num {
    font-weight: 700;
    font-size: clamp(22px, 3vh, 38px);
    line-height: 1;
  }
  .a .nm {
    font-size: 10px;
    letter-spacing: 0.06em;
    color: var(--ink-3);
    font-weight: 500;
  }
  .a.on .nm { color: var(--brand-2); font-weight: 600; }
</style>
