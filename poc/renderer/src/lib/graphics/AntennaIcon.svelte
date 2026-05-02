<script>
  /**
   * Antenna pictogram. `kind` is one of: 'doublet' | 'dipole' | 'yagi'.
   * @type {{ kind?: 'doublet' | 'dipole' | 'yagi' }}
   */
  let { kind = 'doublet' } = $props();
</script>

{#if kind === 'doublet'}
  <svg viewBox="0 0 40 22" width="32" height="18" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" aria-hidden="true">
    <line x1="3"  y1="11" x2="37" y2="11" />
    <line x1="20" y1="11" x2="20" y2="20" />
    <circle cx="3"  cy="11" r="1.6" fill="currentColor" />
    <circle cx="37" cy="11" r="1.6" fill="currentColor" />
  </svg>
{:else if kind === 'dipole'}
  <svg viewBox="0 0 40 22" width="32" height="18" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" aria-hidden="true">
    <line x1="3"  y1="11" x2="18" y2="11" />
    <line x1="22" y1="11" x2="37" y2="11" />
    <rect x="18" y="9" width="4" height="4" rx="1" fill="currentColor" stroke="none" />
    <line x1="20" y1="13" x2="20" y2="20" />
  </svg>
{:else}
  <svg viewBox="0 0 40 22" width="32" height="18" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" aria-hidden="true">
    <line x1="3"  y1="11" x2="37" y2="11" />
    <line x1="6"  y1="6"  x2="6"  y2="16" />
    <line x1="13" y1="4"  x2="13" y2="18" />
    <line x1="20" y1="3"  x2="20" y2="19" />
    <line x1="27" y1="4"  x2="27" y2="18" />
    <line x1="34" y1="6"  x2="34" y2="16" />
  </svg>
{/if}
