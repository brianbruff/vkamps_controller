<div class="lockup">
  <div class="tickline">
    <span>FWD</span>
    <span class="ticks"></span>
    <span>1KW</span>
  </div>
  <div class="marks">VK<span class="three">3</span>AMP</div>
  <div class="tagline">Handcrafted in Australia</div>
</div>

<style>
  .lockup {
    display: flex;
    flex-direction: column;
    align-items: flex-start;
  }

  .tickline {
    display: flex;
    align-items: center;
    gap: 8px;
    font-family: var(--font-num);
    font-size: 10px;
    font-weight: 600;
    letter-spacing: 0.2em;
    color: var(--ink-3);
    margin-bottom: 4px;
  }
  .ticks {
    width: 160px;
    height: 8px;
    background:
      repeating-linear-gradient(90deg, var(--ink-4) 0 1px, transparent 1px 8px),
      repeating-linear-gradient(90deg, var(--brand) 0 2px, transparent 2px 50px);
  }

  .marks {
    font-family: var(--font-display);
    font-weight: 700;
    font-size: 30px;
    line-height: 1;
    letter-spacing: 0.04em;
    color: var(--brand);
    transform: skewX(-7deg);
  }
  .marks .three { color: var(--brand-2); }

  .tagline {
    font-style: italic;
    font-weight: 500;
    font-size: 11px;
    color: var(--ink-3);
    margin-top: 4px;
  }
</style>
