<script>
  /** @type {{ name: string, size?: number, stroke?: string }} */
  let { name, size = 16, stroke = 'currentColor' } = $props();
</script>

<svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke={stroke}
     stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
  {#if name === 'reset'}
    <path d="M3 12a9 9 0 1 0 3-6.7" />
    <polyline points="3 4 3 10 9 10" />
  {:else if name === 'sleep'}
    <path d="M21 12.8A9 9 0 1 1 11.2 3a7 7 0 0 0 9.8 9.8z" />
  {:else if name === 'bypass'}
    <path d="M4 7h11l-3-3" /><path d="M20 17H9l3 3" />
  {:else if name === 'setup'}
    <circle cx="12" cy="12" r="3" />
    <path d="M19.4 15a1.7 1.7 0 0 0 .3 1.8l.1.1a2 2 0 1 1-2.8 2.8l-.1-.1a1.7 1.7 0 0 0-1.8-.3 1.7 1.7 0 0 0-1 1.5V21a2 2 0 1 1-4 0v-.1a1.7 1.7 0 0 0-1.1-1.5 1.7 1.7 0 0 0-1.8.3l-.1.1a2 2 0 1 1-2.8-2.8l.1-.1a1.7 1.7 0 0 0 .3-1.8 1.7 1.7 0 0 0-1.5-1H3a2 2 0 1 1 0-4h.1a1.7 1.7 0 0 0 1.5-1.1 1.7 1.7 0 0 0-.3-1.8l-.1-.1a2 2 0 1 1 2.8-2.8l.1.1a1.7 1.7 0 0 0 1.8.3H9a1.7 1.7 0 0 0 1-1.5V3a2 2 0 1 1 4 0v.1a1.7 1.7 0 0 0 1 1.5 1.7 1.7 0 0 0 1.8-.3l.1-.1a2 2 0 1 1 2.8 2.8l-.1.1a1.7 1.7 0 0 0-.3 1.8V9a1.7 1.7 0 0 0 1.5 1H21a2 2 0 1 1 0 4h-.1a1.7 1.7 0 0 0-1.5 1z" />
  {:else if name === 'fan'}
    <circle cx="12" cy="12" r="2.5" />
    <path d="M12 2a4 4 0 0 0-4 4c0 2 2 3 4 3s4-1 4-3a4 4 0 0 0-4-4z" />
    <path d="M22 12a4 4 0 0 0-4-4c-2 0-3 2-3 4s1 4 3 4a4 4 0 0 0 4-4z" />
    <path d="M12 22a4 4 0 0 0 4-4c0-2-2-3-4-3s-4 1-4 3a4 4 0 0 0 4 4z" />
    <path d="M2 12a4 4 0 0 0 4 4c2 0 3-2 3-4s-1-4-3-4a4 4 0 0 0-4 4z" />
  {:else if name === 'antenna'}
    <line x1="12" y1="22" x2="12" y2="13" />
    <path d="M5 13c1-3 3-5 7-5s6 2 7 5" />
    <path d="M3 16c1-5 4-8 9-8s8 3 9 8" />
  {:else if name === 'bolt'}
    <polyline points="13 2 4 14 11 14 9 22 20 10 13 10 13 2" />
  {:else if name === 'thermo'}
    <path d="M14 14.8V4a2 2 0 0 0-4 0v10.8a4 4 0 1 0 4 0z" />
  {:else if name === 'wifi'}
    <path d="M5 12.5a10 10 0 0 1 14 0" />
    <path d="M8.5 16a5 5 0 0 1 7 0" />
    <line x1="12" y1="20" x2="12" y2="20" />
  {:else if name === 'usb'}
    <circle cx="12" cy="20" r="2" />
    <path d="M12 18V6" />
    <polygon points="9 6 15 6 12 2" fill={stroke} />
    <circle cx="6" cy="12" r="1" />
    <path d="M6 12l6 4" />
    <circle cx="18" cy="9" r="1" />
    <path d="M18 9l-6 5" />
  {:else if name === 'tx'}
    <circle cx="12" cy="12" r="3" />
    <path d="M5 12a7 7 0 0 1 14 0" />
    <path d="M2 12a10 10 0 0 1 20 0" />
  {:else if name === 'check'}
    <polyline points="20 6 9 17 4 12" />
  {:else if name === 'x'}
    <line x1="6" y1="6" x2="18" y2="18" />
    <line x1="18" y1="6" x2="6" y2="18" />
  {:else if name === 'snowflake'}
    <line x1="12" y1="2" x2="12" y2="22" />
    <line x1="2" y1="12" x2="22" y2="12" />
    <line x1="5" y1="5" x2="19" y2="19" />
    <line x1="19" y1="5" x2="5" y2="19" />
  {:else if name === 'chevron'}
    <polyline points="9 6 15 12 9 18" />
  {:else if name === 'log'}
    <line x1="8" y1="6" x2="21" y2="6" />
    <line x1="8" y1="12" x2="21" y2="12" />
    <line x1="8" y1="18" x2="21" y2="18" />
    <line x1="3" y1="6" x2="3.01" y2="6" />
    <line x1="3" y1="12" x2="3.01" y2="12" />
    <line x1="3" y1="18" x2="3.01" y2="18" />
  {:else if name === 'plug'}
    <path d="M9 7V3" />
    <path d="M15 7V3" />
    <rect x="7" y="7" width="10" height="6" rx="2" />
    <path d="M12 13v4a4 4 0 0 1-4 4" />
  {/if}
</svg>
