<script>
  let {
    value = 0,
    peak = 0,
    maxRaw = 776,
    scaleLabels = [],
    label = '',
    displayValue = '',
    unit = 'w',
    height = 22,
    large = false,
    amber = false,
  } = $props();

  let fillPct = $derived(Math.max(0, Math.min((value / maxRaw) * 100, 100)));
  let peakPct = $derived(Math.max(0, Math.min((peak / maxRaw) * 100, 100)));
  // Scale the gradient so it covers full track width, not just fill width
  let bgScale = $derived(fillPct > 0 ? (10000 / fillPct) : 100);
</script>

<div class="gauge-wrap" class:large>
  <div class="gauge-header">
    <span class="gauge-label">{label}</span>
    <span class="gauge-value" class:amber>{displayValue}<span class="gauge-unit">{unit}</span></span>
  </div>
  <div class="gauge-track" style="height: {height}px;">
    <!-- Fill bar: gradient spans full track width via background-size -->
    <div
      class="gauge-fill"
      class:amber
      style="width: {fillPct}%; background-size: {bgScale}% 100%;"
    ></div>
    <!-- Spectrum analyzer bars for large gauge -->
    {#if large && fillPct > 70}
      <div class="spectrum-zone" style="left: {Math.max(fillPct - 18, 65)}%; width: {Math.min(18, fillPct - 65)}%;">
        {#each Array(14) as _, i}
          <div class="spectrum-bar" style="height: {25 + Math.random() * 75}%; opacity: {0.5 + Math.random() * 0.5};"></div>
        {/each}
      </div>
    {/if}
    <!-- Peak hold marker -->
    {#if peakPct > 1}
      <div class="gauge-peak" style="left: {peakPct}%;"></div>
    {/if}
    <!-- Top shine -->
    <div class="gauge-shine"></div>
  </div>
  {#if scaleLabels.length > 0}
    <div class="gauge-ticks">
      <span class="tick" style="left: 0%;">0</span>
      {#each scaleLabels as tick, i}
        <span class="tick" class:tick-last={i === scaleLabels.length - 1} style="left: {((i + 1) / scaleLabels.length) * 100}%;">{tick}</span>
      {/each}
    </div>
  {/if}
</div>

<style>
  .gauge-wrap { width: 100%; }

  .gauge-header {
    display: flex;
    justify-content: space-between;
    align-items: baseline;
    margin-bottom: 2px;
    padding: 0 2px;
  }
  .gauge-label {
    font-size: 13px;
    color: #888;
    font-weight: 600;
  }
  .large .gauge-label {
    font-size: 20px;
    color: #aaa;
    font-weight: 700;
  }

  .gauge-value {
    color: #39ff14;
    font-size: 22px;
    font-weight: 800;
    font-family: 'Impact', 'Arial Narrow', 'Helvetica Neue', sans-serif;
    letter-spacing: 0.5px;
    text-shadow: 0 0 14px rgba(57, 255, 20, 0.5), 0 0 30px rgba(57, 255, 20, 0.2);
  }
  .gauge-value.amber {
    color: #e8c831;
    text-shadow: 0 0 14px rgba(232, 200, 49, 0.5), 0 0 30px rgba(232, 200, 49, 0.2);
  }
  .large .gauge-value {
    font-size: 64px;
    letter-spacing: -1px;
    line-height: 1;
  }
  .gauge-unit {
    font-size: 14px;
    color: #777;
    font-weight: 400;
    margin-left: 4px;
    font-family: -apple-system, BlinkMacSystemFont, sans-serif;
  }
  .large .gauge-unit {
    font-size: 24px;
  }

  /* Track */
  .gauge-track {
    position: relative;
    width: 100%;
    border-radius: 5px;
    overflow: hidden;
    background: #040704;
    border: 1px solid #1a2f1a;
    box-shadow:
      inset 0 2px 6px rgba(0,0,0,0.6),
      inset 0 0 2px rgba(0,0,0,0.4),
      0 1px 0 rgba(57, 255, 20, 0.04);
  }

  /* Fill — gradient spans full track via background-size */
  .gauge-fill {
    position: absolute;
    top: 2px; left: 2px; bottom: 2px;
    border-radius: 3px;
    background-image: linear-gradient(90deg,
      #0e6e0e 0%,
      #1aaa1a 8%,
      #30dd30 16%,
      #39ff14 28%,
      #55ff22 40%,
      #88ee11 52%,
      #bbcc00 63%,
      #ddaa00 73%,
      #ee7700 82%,
      #ff4400 91%,
      #ff1100 100%
    );
    background-position: left center;
    background-repeat: no-repeat;
    transition: width 60ms linear;
    z-index: 2;
    box-shadow:
      0 0 18px rgba(57, 255, 20, 0.5),
      0 0 40px rgba(57, 255, 20, 0.2),
      inset 0 1px 0 rgba(255,255,255,0.15);
  }
  .gauge-fill.amber {
    background-image: linear-gradient(90deg,
      #3a2a00 0%,
      #6a5a00 12%,
      #9a8a18 28%,
      #c8b828 45%,
      #e0d030 60%,
      #e8c831 75%,
      #eeaa22 88%,
      #ff8800 100%
    );
    box-shadow:
      0 0 18px rgba(232, 200, 49, 0.4),
      0 0 40px rgba(232, 200, 49, 0.15),
      inset 0 1px 0 rgba(255,255,255,0.08);
  }

  /* Spectrum analyzer bars overlay */
  .spectrum-zone {
    position: absolute;
    top: 2px;
    bottom: 2px;
    display: flex;
    align-items: flex-end;
    gap: 1px;
    z-index: 3;
    pointer-events: none;
  }
  .spectrum-bar {
    flex: 1;
    min-width: 3px;
    border-radius: 1px 1px 0 0;
    background: linear-gradient(to top,
      #ff4400 0%,
      #ff6600 30%,
      #ffaa00 60%,
      #ffcc00 80%,
      #ffee44 100%
    );
    box-shadow: 0 0 4px rgba(255, 100, 0, 0.6);
  }

  .gauge-peak {
    position: absolute;
    top: 2px; bottom: 2px;
    width: 3px;
    background: #ff6600;
    z-index: 5;
    border-radius: 1px;
    box-shadow: 0 0 8px rgba(255, 102, 0, 0.8);
    transform: translateX(-1.5px);
  }

  .gauge-shine {
    position: absolute;
    top: 2px; left: 2px; right: 2px;
    height: 40%;
    background: linear-gradient(180deg,
      rgba(255,255,255,0.06) 0%,
      transparent 100%
    );
    z-index: 4;
    pointer-events: none;
    border-radius: 3px 3px 0 0;
  }

  .gauge-ticks {
    position: relative;
    height: 14px;
    margin-top: 2px;
  }
  .tick {
    position: absolute;
    transform: translateX(-50%);
    font-size: 10px;
    color: #666;
    font-family: -apple-system, sans-serif;
    font-weight: 500;
  }
  .large .tick {
    font-size: 11px;
  }
  .tick-last {
    color: #ff3333 !important;
    font-weight: 700 !important;
    text-shadow: 0 0 6px rgba(255, 51, 51, 0.4);
  }
</style>
