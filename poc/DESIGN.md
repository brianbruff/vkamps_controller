# VKAmp POC — Design Plan

**Status:** Build-ready spec for the proof-of-concept Electron + Svelte 5 client.
**Audience:** Implementer scaffolding `poc/`.
**Source-of-truth refs:** `docs/PRD.md`, `docs/PROTOCOL.md`.

This document specifies the visual language, layout, component inventory, command-surface bindings, and dev-mode mock data shape so an implementer can build the POC directly without re-deriving decisions. The design is inspired by **OpenHpsdr Zeus** — dark slate + bright blue — explicitly **not** the green VKAmp mockup.

---

## 1. POC Tester Banner

The app must be unmistakably labeled as a tester / proof-of-concept build at all times. We use **two reinforcing markers**:

### 1a. Persistent top ribbon (always visible)
A thin strip pinned to the top of the window content area (below the OS title bar), spanning full width. Always rendered, never dismissable.

- **Height:** 22px
- **Background:** repeating diagonal stripes — `repeating-linear-gradient(45deg, #2a1d0a 0 10px, #3a2912 10px 20px)` (dim amber so it reads as caution but doesn't fight the blue accent)
- **Foreground text** (centered, small caps, letterspaced):
  > `⚠  POC TESTER BUILD  ·  NOT FOR FIELD USE  ·  PROTOCOL UNVERIFIED  ⚠`
- **Text color:** `#f5c97b` (warm amber, matches the stripe)
- **Font:** 11px, weight 600, tracking 0.18em, uppercase

### 1b. Corner badge in the header
A static pill in the top-right of the app header, beside the connection pill:

- Text: `POC` in 10px uppercase, letterspaced
- Background: `#3a2912`, 1px border `#6b4e1f`, text color `#f5c97b`, 4px radius

### 1c. Window title
Electron `BrowserWindow` title: `VKAmp [POC TESTER] — Helios DX Replacement`

The ribbon is the load-bearing marker; the badge and title are belt-and-suspenders so screenshots and screen-shares can never be mistaken for a release build.

---

## 2. Color Tokens

Locked palette — implement as CSS custom properties on `:root` in `src/app.css`. WCAG AA verified against `--color-bg` for `--color-text` (>= 9.5:1) and `--color-text-muted` (>= 4.6:1).

```css
:root {
  /* Surfaces */
  --color-bg:            #0e1620;  /* app background */
  --color-bg-elevated:   #121a26;  /* secondary background */
  --color-surface:       #1a2331;  /* panel / card */
  --color-surface-hi:    #21304180;  /* hover overlay (50% alpha) */
  --color-border:        #2a3543;  /* subtle 1px borders */
  --color-border-strong: #3a4858;  /* dividers, focused inputs */

  /* Text */
  --color-text:          #c8d3df;  /* body text */
  --color-text-strong:   #e8eef5;  /* numerics, headings */
  --color-text-muted:    #7a8896;  /* labels, small caps */
  --color-text-faint:    #4f5b6a;  /* disabled, scale ticks */

  /* Brand / accent (bright blue) */
  --color-accent:        #4daefb;  /* primary blue — borders, fills, focus */
  --color-accent-hi:     #7cc3ff;  /* hover */
  --color-accent-lo:     #2c7fc4;  /* pressed */
  --color-accent-fill:   #1f4d7a;  /* active button background */
  --color-accent-glow:   #4daefb33;  /* 20% alpha for soft glows */

  /* Status colors */
  --color-ok:            #4ad27a;  /* connected / status OK */
  --color-tx:            #ff4d4a;  /* ON AIR — warm red */
  --color-bypass:        #d36cff;  /* bypass active — magenta */
  --color-error:         #ff4d4a;  /* error states */
  --color-warn:          #f5c97b;  /* warnings, POC ribbon */

  /* Gauge fills */
  --gauge-fill:          var(--color-accent);  /* default — bright blue */
  --gauge-fill-warn:     #ffb347;  /* 80–90% range (optional) */
  --gauge-fill-over:     var(--color-error);  /* >90% — over-limit only */
  --gauge-track:         #1a2331;
  --gauge-tick:          #3a4858;
  --gauge-peak:          #e8eef5;  /* peak-hold marker */

  /* Shadows / focus */
  --shadow-card:         0 1px 0 #00000040, 0 4px 12px #00000033;
  --focus-ring:          0 0 0 2px var(--color-bg), 0 0 0 4px var(--color-accent);
}
```

**Usage rules:**
- Gauges fill solid `--gauge-fill` (blue). Switch to `--gauge-fill-over` only when the value crosses the 90% threshold of the configured ceiling.
- Active control = `--color-accent-fill` background + 1px `--color-accent` border + `--color-text-strong` text.
- Inactive control = transparent background + 1px `--color-border` border + `--color-text` text.
- Hover (any clickable) = overlay `--color-surface-hi` and brighten border to `--color-border-strong`.
- Focus visible (keyboard) = `--focus-ring`.

---

## 3. Typography

```css
:root {
  /* Font stacks */
  --font-ui: "Inter", -apple-system, BlinkMacSystemFont, "Segoe UI", system-ui, sans-serif;
  --font-num: "JetBrains Mono", "SF Mono", ui-monospace, "Cascadia Mono", Menlo, monospace;

  /* Sizes */
  --fs-xs:    10px;   /* small caps labels, scale ticks */
  --fs-sm:    12px;   /* secondary labels */
  --fs-base:  14px;   /* body */
  --fs-md:    16px;   /* card values */
  --fs-lg:    20px;   /* section heads */
  --fs-xl:    32px;   /* sub-meter readouts */
  --fs-2xl:   56px;   /* primary output power */
}

/* Numeric cells */
.num {
  font-family: var(--font-num);
  font-variant-numeric: tabular-nums;
  font-feature-settings: "tnum", "zero";
  letter-spacing: -0.01em;
}

/* Section / unit labels */
.label {
  font-family: var(--font-ui);
  font-size: var(--fs-xs);
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.14em;
  color: var(--color-text-muted);
}
```

**Bundle Inter and JetBrains Mono locally** under `src/assets/fonts/` and `@font-face` them so the app does not depend on the network or installed fonts. Use only `400` and `600` weights of each to keep bundle small.

| Element                          | Class / size                                  |
|----------------------------------|-----------------------------------------------|
| Output power readout             | `.num` `--fs-2xl` 600 `--color-text-strong`   |
| Sub-meter readouts (Refl / I / In) | `.num` `--fs-xl` 600 `--color-text-strong`  |
| Stat-card values (V, °C, etc.)   | `.num` `--fs-lg` 600 `--color-text-strong`   |
| Stat-card labels                 | `.label`                                      |
| Body / button text               | `--font-ui` `--fs-base` 500 `--color-text`    |
| Status pill text                 | `--font-ui` `--fs-sm` 600 uppercase 0.10em    |
| Scale ticks                      | `.num` `--fs-xs` `--color-text-faint`         |

---

## 4. Layout

Default window size **1024 × 640** (stretches the original 900 × 371 to give room for Zeus-style breathing). Resizable; minimum **880 × 580**.

Whole window is a vertical CSS grid. The tester ribbon sits outside the main grid as a flex row above it.

```
┌─────────────────────────────────────────────────────────────────────┐
│ ⚠  POC TESTER BUILD · NOT FOR FIELD USE · PROTOCOL UNVERIFIED  ⚠  │ <- ribbon (1b)
├─────────────────────────────────────────────────────────────────────┤
│ [VKamp]  Ham Radio Amplifier Control      [POC]  [● TCP 192.168…]   │ <- header
├─────────────────────────────────────────────────────────────────────┤
│ OUTPUT POWER                                              1175 W    │
│ ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓░░░░░░░░░│         │ <- output meter
│ 0   100  200  400  600  800 1000 1200    (ticks per koef; shown @1200) │
├─────────────────────────────────────────────────────────────────────┤
│ REFLECTED       │ INPUT            │ CURRENT                        │
│   5 W           │  28 W            │  34.5 A                        │ <- sub-meters
│ ▓▓▓░░░░ 0–50    │ ▓▓▓▓░░░ 0–100    │ ▓▓▓▓▓▓░ 0–40                    │
├─────────────────────────────────────────────────────────────────────┤
│ ● STATUS OK   ⚫ ON AIR   ❄  FAN AUTO                                │ <- status pills
├─────────────────────────────────────────────────────────────────────┤
│ ANT 1  │ BAND 30M │ SWR 1.14 │ EFF 75% │ VOLTS 46.9V+ │ TEMP 27°C   │ <- stat cards
├─────────────────────────────────────────────────────────────────────┤
│   [↻ Reset]  [☾ Sleep]  [⇄ ByPass]  [⚙ Setup]    [⚡ Mock TX*]      │ <- footer
└─────────────────────────────────────────────────────────────────────┘
* Mock TX button only appears when running with VKAMP_MOCK=1
```

**CSS Grid (root container):**
```css
.app-shell {
  display: grid;
  grid-template-rows:
    auto    /* tester ribbon */
    56px    /* header */
    1fr     /* main content (own grid) */;
  height: 100vh;
}

.main {
  display: grid;
  grid-template-rows:
    minmax(120px, 1.4fr)  /* output meter */
    minmax(110px, 1fr)    /* sub-meters */
    44px                  /* status pills */
    72px                  /* stat cards */
    72px                  /* footer controls */;
  gap: 12px;
  padding: 12px 16px;
  background: var(--color-bg);
}

.sub-meters { display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 12px; }
.stat-cards { display: grid; grid-template-columns: repeat(6, 1fr); gap: 8px; }
.status-pills { display: flex; gap: 12px; align-items: stretch; }
.footer-controls { display: grid; grid-template-columns: repeat(5, 1fr); gap: 8px; }
```

**Element-by-element references to protocol meters/state:**

| Region            | Field(s) shown                                    | Source       |
|-------------------|---------------------------------------------------|--------------|
| Output meter      | Output Power `p1` (raw 0–776 → watts via koef). **Ceiling auto-scales per koef: 600 / 1200 / 2400 W.** Tick marks per `SCALE_LABELS.output[koef]` from `protocol.js`. | UDP / serial-4 |
| Reflected sub     | Reflected Power `p2`                              | UDP / serial-4 |
| Input sub         | Input Power, derived from `p12` (USB: `p12²/476`, LAN: `p12²/540`) | UDP / serial-4 |
| Current sub       | `p4 / 10` Amps                                    | UDP / serial-4 |
| Status pill 1     | Connection / error: `p9`, `p8`                    | TCP / serial-8 |
| Status pill 2     | TX / ByPass: `p11` (1=bypassed) + derived TX flag | TCP / serial-8 |
| Status pill 3     | Fan mode (local toggle, last-sent 45/46)          | local state  |
| Stat card ANT     | Antenna `p7` (1–3)                                | TCP / serial-8 |
| Stat card BAND    | Band index `p6` → label                           | TCP / serial-8 |
| Stat card SWR     | Derived: `(p1+p2)/(p1−p2)` clamped ≥1.0, capped 9.99 (matches original `calcSWR`) | derived |
| Stat card EFF     | Efficiency formula (PRD 7.4) per koef             | derived      |
| Stat card VOLTS   | `p5 / 10` V (suffix `+` if Volts+ active)         | TCP / serial-8 |
| Stat card TEMP    | `p3` °C (or `18·p3/10 + 32` °F per setting)       | TCP / serial-8 |

**Control type per element** (this is the contract for the implementer):

| Element            | Type             | UI behavior                                              |
|--------------------|------------------|----------------------------------------------------------|
| Reset (footer)     | momentary button | sends `23` once on click                                 |
| Sleep (footer)     | momentary button | sends `44`; visually latches "asleep" until next packet  |
| ByPass (footer)    | toggle           | sends `21` if off, `22` if on (state from `p11`)         |
| Setup (footer)     | modal trigger    | opens `SettingsModal`                                    |
| Mock TX (footer)   | toggle, dev only | starts/stops the local mock telemetry generator's TX     |
| Status pill (TX/Bypass) | toggle      | same effect as footer ByPass; mirrored for parity        |
| Status pill (Fan)  | toggle           | sends `45` (→ "Fan 100%") or `46` (→ "Fan Auto")         |
| Stat card ANT      | selector (cycle) | sends `31`+(n-1) cycling 1→2→3→4→1 (clamped to map)      |
| Stat card BAND     | selector (cycle) | sends `71`+i, **only enabled when CAT = Manual (66)**    |
| Stat card VOLTS    | toggle           | sends `41` (Normal) or `42` (Volts+); suffix updates     |
| Stat card SWR/EFF/TEMP | read-only    | display only                                             |
| Settings: transport| radio group      | persists; reconnects on save                             |
| Settings: koef     | radio group      | persists; affects Eff & display scaling (no protocol cmd)|
| Settings: voltage  | radio group      | sends `51`/`52`/`53`/`54`                                |
| Settings: CAT      | radio group      | sends `61`–`66`                                          |
| Settings: ant map  | 8 number inputs  | persists; clamps the antenna selector per band           |
| Settings: options  | checkboxes       | persist; affect display only (except always-on-top → IPC)|

---

## 5. Component Inventory

All components are Svelte 5 with runes (`$state`, `$derived`, `$props`). File tree under `poc/src/`:

```
src/
├── App.svelte
├── app.css                       — palette + base resets + .num / .label
├── lib/
│   ├── chrome/
│   │   ├── TesterBanner.svelte   — persistent striped ribbon (§1a)
│   │   ├── HeaderBar.svelte      — logo + title + corner POC badge + ConnectionPill
│   │   └── ConnectionPill.svelte — transport icon, address, OK/error dot
│   ├── meters/
│   │   ├── OutputMeter.svelte    — full-width labeled bar, peak-hold tick, big readout
│   │   ├── MeterBar.svelte       — reusable horizontal scaled bar (label, value, units, peak)
│   │   └── meter-scale.ts        — tick generator (nice-numbers algorithm)
│   ├── status/
│   │   └── StatusPill.svelte     — colored dot + label + optional click handler
│   ├── cards/
│   │   └── StatCard.svelte       — icon + value + unit + label, click-cycle / toggle variants
│   ├── controls/
│   │   └── ControlButton.svelte  — footer-style icon button (active/inactive variants)
│   ├── settings/
│   │   ├── SettingsModal.svelte  — frame, focus trap, save/cancel
│   │   ├── ConnectionSection.svelte
│   │   ├── KoefSection.svelte
│   │   ├── VoltageSection.svelte
│   │   ├── CatSection.svelte
│   │   ├── AntennaMapSection.svelte
│   │   └── OptionsSection.svelte
│   ├── icons/
│   │   └── Icon.svelte           — inline-SVG sprite renderer (reset, sleep, bypass, setup, fan, antenna, bolt, thermo, wifi, usb)
│   ├── dev/
│   │   └── DevPanel.svelte       — collapsible bottom-right tray, only when VKAMP_MOCK=1
│   └── stores/
│       ├── telemetry.svelte.ts   — latest packet + peak holds + derived (eff, swr, watts)
│       ├── deviceState.svelte.ts — band, antenna, bypass, volts+, fan, error, txActive
│       ├── settings.svelte.ts    — persisted config (electron-store backed via IPC)
│       └── transport.svelte.ts   — connection state (connecting / open / error / closed)
└── main/                         — Electron main-process code (transport drivers + IPC bridge)
    ├── main.ts
    ├── preload.ts
    ├── transport/
    │   ├── serial.ts
    │   ├── tcp.ts
    │   ├── udp.ts
    │   └── mock.ts
    └── store.ts
```

**One-line responsibilities:**
- `TesterBanner.svelte` — renders the striped ribbon; pure presentational, no props.
- `HeaderBar.svelte` — flex row: logo svg, title, POC badge, slot for ConnectionPill.
- `ConnectionPill.svelte` — props `{ transport, address, status }`; colored dot + monospaced address.
- `OutputMeter.svelte` — props `{ value, max, unit, peakHold }`; renders large readout, full-width bar, tick scale, peak marker.
- `MeterBar.svelte` — props `{ label, value, max, unit, peakHold, overThreshold = 0.9 }`; same engine as OutputMeter, smaller chrome. Switches fill to red beyond `overThreshold`.
- `StatusPill.svelte` — props `{ label, tone, active, onclick? }`; tone ∈ `ok|tx|bypass|fan|error|idle`; active state inverts colors.
- `StatCard.svelte` — props `{ icon, value, unit, label, mode = 'readonly' | 'cycle' | 'toggle', enabled = true, onactivate? }`; cycle variant shows a subtle right-edge chevron.
- `ControlButton.svelte` — props `{ icon, label, kind = 'momentary' | 'toggle', active?, onclick }`.
- `SettingsModal.svelte` — `bind:open`; renders backdrop, traps focus, reads from `settings` store into a draft, commits on Save.
- `Icon.svelte` — `<Icon name="reset" />` resolves a single 24-icon sprite for crisp rendering.
- `DevPanel.svelte` — exposes mock-data sliders (output, current, voltage), error injector, connect/disconnect simulator.

---

## 6. Command Surface — UI ↔ Protocol Mapping

Every protocol command in PROTOCOL.md has a designated UI trigger. Commands marked **(auto)** are issued by the transport layer, not by user clicks.

| Cmd     | Action                          | UI Trigger                                                                |
|---------|---------------------------------|---------------------------------------------------------------------------|
| `11`    | Connect / init                  | **(auto)** Issued by transport on open and after `99` during handshake.   |
| `99`    | Disconnect / reset              | **(auto)** Issued by transport on app exit / manual disconnect.           |
| `21`    | Bypass ON                       | Footer ByPass button click when `bypass=false`. Mirrored on Status pill 2.|
| `22`    | Bypass OFF                      | Same control, when `bypass=true`.                                         |
| `23`    | Reset / clear error             | Footer Reset button. Also fires on click of the error-state Status pill 1.|
| `31`    | Antenna 1                       | ANT stat card — cycle to position 1.                                      |
| `32`    | Antenna 2                       | ANT stat card — cycle to position 2.                                      |
| `33`    | Antenna 3                       | ANT stat card — cycle to position 3.                                      |
| `41`    | Volts normal                    | VOLTS stat card click when state is Volts+.                               |
| `42`    | Volts+                          | VOLTS stat card click when state is Normal.                               |
| `44`    | Sleep / standby                 | Footer Sleep button.                                                      |
| `45`    | Fan 100%                        | FAN status pill click when state is Auto.                                 |
| `46`    | Fan Auto                        | FAN status pill click when state is 100%.                                 |
| `51`    | Voltage → 48V                   | Settings → Voltage Reference radio.                                       |
| `52`    | Voltage → 50V                   | Settings → Voltage Reference radio.                                       |
| `53`    | Voltage → 53.5V                 | Settings → Voltage Reference radio.                                       |
| `54`    | Voltage → 58.3V                 | Settings → Voltage Reference radio.                                       |
| `61`    | CAT: RF                         | Settings → CAT radio.                                                     |
| `62`    | CAT: Icom                       | Settings → CAT radio.                                                     |
| `63`    | CAT: Yaesu                      | Settings → CAT radio.                                                     |
| `64`    | CAT: Kenwood/Flex               | Settings → CAT radio.                                                     |
| `65`    | CAT: Anan/SunSDR                | Settings → CAT radio.                                                     |
| `66`    | CAT: Manual                     | Settings → CAT radio. **Enables BAND stat-card cycling.**                 |
| `71`    | Band 160m                       | BAND stat card cycle (only when CAT = Manual).                            |
| `72`    | Band 80m                        | BAND stat card cycle.                                                     |
| `73`    | Band 40m                        | BAND stat card cycle.                                                     |
| `74`    | Band 30m                        | BAND stat card cycle.                                                     |
| `75`    | Band 20m                        | BAND stat card cycle.                                                     |
| `76`    | Band 17–15m                     | BAND stat card cycle.                                                     |
| `77`    | Band 12–10m                     | BAND stat card cycle.                                                     |
| `78`    | Band 6m                         | BAND stat card cycle.                                                     |

When CAT ≠ Manual, the BAND stat card is shown read-only (cursor: default, no chevron, label dimmed to `--color-text-muted`); the band updates from incoming `p6`.

All outgoing writes go through `window.api.send(cmd: string)` exposed by the preload, which appends `\n` for serial and forwards as-is for TCP/UDP. The renderer never touches sockets directly.

---

## 7. Mock Telemetry (Dev Mode)

When the env var `VKAMP_MOCK=1` (or `--mock` CLI flag), the main process boots `transport/mock.ts` instead of opening a real socket. The mock emits packets in the same shape as the real transports so the renderer code is identical in both modes.

### 7a. Packet shapes (single source of truth)

```ts
// poc/src/shared/packet.ts — used by both main and renderer
export type EightField = {
  kind: 'state';        // corresponds to TCP packet / serial 8-field
  p3: number;           // temperature raw
  p5: number;           // voltage * 10
  p6: number;           // band index 1..8 from device (converted to 0..7 internally)
  p7: number;           // antenna 1..3
  p8: 0 | 1 | 2 | 3;    // error code
  p9: number;           // device status, 0 = OK
  p10: number;          // reserved
  p11: 0 | 1;           // bypass state
};

export type FourField = {
  kind: 'meter';        // corresponds to UDP packet / serial 4-field
  p1: number;           // output power 0..776
  p2: number;           // reflected power 0..50
  p4: number;           // current * 10
  p12: number;          // input power raw
};

export type Packet = EightField | FourField;
```

The transport adapters all normalize to this discriminated union. Renderer subscribes via `window.api.onPacket((pkt) => ...)`.

### 7b. Mock generator behavior

```ts
// drives at 50 Hz (20 ms tick) — close to real TCP/UDP poll rate
const tick = 20;
let t = 0;          // seconds
let txOn = false;   // toggled by Mock TX button
let injectError: 0 | 1 | 2 | 3 = 0;
let band = 4;       // 30m default (device uses 1-8, where 4=30m)
let antenna = 1;
let bypass = 0;
let voltsPlus = false;
let fanFull = false;
let errorClearedAt = 0;

setInterval(() => {
  t += tick / 1000;

  // ── 4-field meter packet every tick ──
  const baseDrive = txOn ? 0.5 + 0.5 * Math.sin(2 * Math.PI * 0.4 * t) : 0;
  const noise = (s = 1) => (Math.random() - 0.5) * s;

  const p1 = bypass ? 0 : Math.max(0, Math.round(720 * baseDrive + noise(8)));
  const p2 = bypass ? 0 : Math.max(0, Math.round(p1 * 0.04 + noise(2)));
  const p4 = bypass ? 12 : Math.max(0, Math.round((40 + p1 * 0.06) + noise(3))); // tenths of A
  const p12 = bypass ? 0 : Math.max(0, Math.round(20 * baseDrive + noise(2)));

  emit({ kind: 'meter', p1, p2, p4, p12 });

  // ── 8-field state packet every 5 ticks (~100 ms) ──
  if (Math.round(t * 1000 / tick) % 5 === 0) {
    const v = (voltsPlus ? 535 : 480) + Math.round(noise(4)); // tenths of V
    const tempBase = 28 + (txOn ? 12 * baseDrive : 0);
    const p3 = Math.round(tempBase + noise(0.6));
    const p5 = v;
    const p6 = band;
    const p7 = antenna;
    const p8 = injectError;
    const p9 = 0;
    const p10 = 0;
    const p11 = bypass;
    emit({ kind: 'state', p3, p5, p6, p7, p8, p9, p10, p11 });
  }
}, tick);

// Mock also handles outbound commands so the round-trip feels real:
function onCommand(cmd: string) {
  if (cmd === '21') bypass = 1;
  if (cmd === '22') bypass = 0;
  if (cmd === '23') injectError = 0;
  if (/^3[1-4]$/.test(cmd)) antenna = +cmd[1];
  if (cmd === '41') voltsPlus = false;
  if (cmd === '42') voltsPlus = true;
  if (cmd === '45') fanFull = true;
  if (cmd === '46') fanFull = false;
  if (/^7[1-8]$/.test(cmd)) band = +cmd[1] - 1;
  // 51-54, 61-66, 11, 99 are accepted with no state change
}
```

### 7c. Dev panel controls (`DevPanel.svelte`)

When `VKAMP_MOCK=1`, the dev panel docks to bottom-right and exposes:
- **Mock TX** toggle (mirrored to footer)
- Output drive slider (overrides sine wave 0–100%)
- Voltage drift slider (±5 V)
- Inject error: dropdown → `0 | 1 | 2 | 3`
- Force bypass / force fan-full toggles
- "Drop connection" button (simulates 3-second outage to test reconnect UX)

The dev panel is **not bundled** in production builds — the import is gated by `if (import.meta.env.DEV || import.meta.env.VITE_VKAMP_MOCK)`.

---

## 8. Settings Dialog

Modal centered in the window, 560 × 600 (scrollable if content overflows). Backdrop is `--color-bg` at 70% opacity with an 8px blur. The dialog itself uses `--color-surface` with a 1px `--color-border-strong` outline and `--shadow-card`. Closes on `Esc` or backdrop click *only if* there are no unsaved changes (otherwise prompts inline).

```
┌──────────────────────────────────────────────────────────────┐
│  SETUP                                                  [✕]  │
│ ──────────────────────────────────────────────────────────── │
│                                                              │
│  CONNECTION                                                  │
│   ◉ USB / Serial    ○ TCP    ○ UDP                           │
│   COM Port  [COM5            ▾]   Baud  [600  ▾]             │
│   IP        [192.168.0.55       ]   TCP [8080] UDP [8081]    │
│                                                              │
│  POWER / KOEF                                                │
│   ○ 600    ◉ 1200    ○ 2400                                  │
│                                                              │
│  VOLTAGE REFERENCE                                           │
│   ◉ 48 V   ○ 50 V   ○ 53.5 V   ○ 58.3 V                      │
│                                                              │
│  CAT PROTOCOL                                                │
│   ◉ RF              ○ Icom            ○ Yaesu                │
│   ○ Kenwood/Flex    ○ Anan/SunSDR     ○ Manual               │
│                                                              │
│  ANTENNA MAP  (per band, 0–3)                                │
│   160m [1]  80m [1]  40m [1]  30m [1]                        │
│   20m  [1]  17m [1]  12m [1]   6m [1]                        │
│                                                              │
│  OPTIONS                                                     │
│   ☐ Always on top         ☐ Fahrenheit                       │
│   ☐ Sound alerts          ☐ Show input indicator             │
│                                                              │
│  PROFILE                                                     │
│   [Import save.txt…]   [Export save.txt…]                    │
│                                                              │
│ ──────────────────────────────────────────────────────────── │
│                              [Cancel]   [Save and Apply]     │
└──────────────────────────────────────────────────────────────┘
```

**Implementation notes:**
- Each section is its own component (see §5) for clean separation and easy testing.
- The dialog opens with a *draft copy* of the settings store; nothing is persisted until "Save and Apply".
- "Save and Apply" → writes to `electron-store`, emits any required protocol commands (`51`–`54`, `61`–`66`), and triggers a transport reconnect if the connection section changed.
- Transport selector visually disables irrelevant fields (e.g. selecting USB greys out IP/TCP/UDP inputs and vice versa). Use opacity 0.4 + `pointer-events: none` rather than removing — keeps layout stable.
- Antenna map inputs use `<input type="number" min="0" max="3">` with custom step buttons styled to the palette.
- Import/Export use the Electron file dialog over IPC and parse/produce the indexed save.txt format described in PRD §5.

**Key bindings inside dialog:**
- `Esc` — Cancel
- `⌘/Ctrl-S` — Save and Apply
- `Tab` cycles fields; trapped within dialog while open.

---

## Appendix A — Visual States Summary

| State            | Color usage                                              |
|------------------|----------------------------------------------------------|
| Idle / disconnected | dot `--color-text-faint`; meters greyed (opacity 0.5) |
| Connected, OK    | dot `--color-ok`; pill text white                        |
| Transmitting     | TX pill background `--color-tx`, white text, soft glow   |
| Bypass active    | pill background `--color-bypass`, dark text              |
| Fan 100%         | pill background `--color-accent-fill`, blue border       |
| Error            | error pill `--color-error`, becomes a Reset trigger      |
| Over-limit meter | bar fill switches to `--gauge-fill-over` past 90% scale  |

---

## Appendix B — Implementation Order Recommendation

For the next teammate scaffolding the app:

1. Vite + Svelte 5 + Electron skeleton, palette CSS, fonts loaded.
2. `TesterBanner`, `HeaderBar`, `App.svelte` shell — verify ribbon is unmissable.
3. Stores (`telemetry`, `deviceState`, `settings`, `transport`).
4. Mock transport (§7) wired through preload IPC. Renderer should be fully driveable from the mock before any real driver lands.
5. `MeterBar` + `OutputMeter`, then sub-meter row.
6. `StatusPill` row, then `StatCard` row.
7. `ControlButton` footer.
8. `SettingsModal` and its sections.
9. Real transports (`serial`, `tcp`, `udp`) behind the same packet contract.
10. `DevPanel` last — it's a debugging aid, not a feature.
