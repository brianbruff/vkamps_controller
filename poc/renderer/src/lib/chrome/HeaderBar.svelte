<script>
  import ConnectControl from './ConnectControl.svelte';
  import Icon from '../icons/Icon.svelte';
  import vkLogo from '../../assets/vk_logo.jpeg';

  /**
   * @type {{
   *   onneedsetup?: () => void,
   *   ondiagnostics?: () => void,
   * }}
   */
  let { onneedsetup, ondiagnostics } = $props();
</script>

<header class="header" aria-label="VK Amps">
  <img src={vkLogo} alt="VK Amps" class="logo" />

  <div class="right">
    <button class="diag-btn" type="button" aria-label="Diagnostics" title="Diagnostics" onclick={() => ondiagnostics?.()}>
      <Icon name="log" size={14} />
      <span class="diag-label">DIAG</span>
    </button>
    <div class="connect-slot">
      <ConnectControl onneedsetup={onneedsetup} />
    </div>
  </div>
</header>

<style>
  .header {
    position: relative;
    height: 64px;
    display: flex;
    align-items: center;
    justify-content: flex-end;
    padding: 0 24px;
    background-color: #1d1740;
    border-bottom: 1px solid #00000033;
    flex: 0 0 auto;
    overflow: hidden;
  }

  /* Logo pinned to the left, fading out toward the right. */
  .logo {
    position: absolute;
    left: 0;
    top: 0;
    bottom: 0;
    height: 100%;
    width: auto;
    display: block;
    pointer-events: none;
    user-select: none;
    mask-image: linear-gradient(90deg, #000 0%, #000 65%, transparent 100%);
    -webkit-mask-image: linear-gradient(90deg, #000 0%, #000 65%, transparent 100%);
  }

  .right {
    position: relative;
    z-index: 1;
    display: flex;
    align-items: center;
    gap: 10px;
  }

  .diag-btn {
    display: inline-flex;
    align-items: center;
    gap: 6px;
    height: 28px;
    padding: 0 10px;
    border: 1px solid rgba(255, 255, 255, 0.35);
    border-radius: 999px;
    background: rgba(255, 255, 255, 0.10);
    color: #f4f3ff;
    font-family: var(--font-ui);
    font-size: 11px;
    font-weight: 600;
    letter-spacing: 0.16em;
    cursor: pointer;
    backdrop-filter: blur(4px);
    transition: border-color 120ms, background 120ms, color 120ms;
  }
  .diag-btn:hover {
    border-color: #ffffff;
    background: rgba(255, 255, 255, 0.18);
    color: #ffffff;
  }
  .diag-label { color: inherit; }

  /* ConnectControl over the dark logo background. */
  .connect-slot :global(form.ctl) {
    background: rgba(255, 255, 255, 0.10);
    border-color: rgba(255, 255, 255, 0.35);
    color: #f4f3ff;
    backdrop-filter: blur(4px);
  }
  .connect-slot :global(form.ctl[data-status="open"])      { border-color: #6ee7b7; }
  .connect-slot :global(form.ctl[data-status="error"])     { border-color: #fca5a5; }
  .connect-slot :global(form.ctl[data-status="connecting"]){ border-color: #fcd34d; }

  .connect-slot :global(.mode),
  .connect-slot :global(.mode-label),
  .connect-slot :global(.addr),
  .connect-slot :global(.port) {
    color: #e6e4ff;
  }

  .connect-slot :global(.ip) {
    background: rgba(255, 255, 255, 0.12);
    border-color: rgba(255, 255, 255, 0.30);
    color: #ffffff;
  }
  .connect-slot :global(.ip::placeholder) { color: rgba(255, 255, 255, 0.55); }
  .connect-slot :global(.ip:focus) {
    background: rgba(255, 255, 255, 0.18);
    border-color: #ffffff;
  }

  .connect-slot :global(.btn) {
    background: rgba(255, 255, 255, 0.12);
    border-color: rgba(255, 255, 255, 0.40);
    color: #ffffff;
  }
  .connect-slot :global(.btn:hover:not(:disabled)) {
    background: rgba(255, 255, 255, 0.22);
    border-color: #ffffff;
  }
  .connect-slot :global(.btn.primary) {
    background: #ffffff;
    border-color: #ffffff;
    color: #1d1740;
  }
  .connect-slot :global(.btn.primary:hover:not(:disabled)) {
    background: #f4f3ff;
    border-color: #f4f3ff;
    color: #1d1740;
  }
  .connect-slot :global(.btn.active) {
    background: rgba(255, 255, 255, 0.10);
    border-color: #6ee7b7;
    color: #ffffff;
  }
</style>
