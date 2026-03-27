# VKAmp — Task Breakdown
**Cross-platform Electron replacement for Helios DX**
**Last updated:** 2026-03-27

---

## Status Legend
- `[ ]` Not started
- `[~]` In progress
- `[x]` Complete
- `[!]` Blocked

---

## Phase 1 — Project Scaffold

- [ ] **1.1** Init repo structure
  - `src/main/` — Electron main process
  - `src/renderer/` — Svelte UI
  - `src/preload/` — IPC bridge
  - `docs/` — PRD, tasks, protocol notes
  - `assets/` — icons, sounds (extract from decompiled/resources/)

- [ ] **1.2** Set up Electron + Svelte + Vite
  - Use `electron-vite` template or manual setup
  - Configure `vite.config.js` for renderer
  - Configure `electron-builder.yml` for all 3 platforms

- [ ] **1.3** Copy over audio assets from decompiled source
  - `decompiled/resources/alert.wav`
  - `decompiled/resources/error.wav`

- [ ] **1.4** Copy over image assets
  - `current.png`, `current2.png`, `power.png`, `reverse.png`, `reverse2.png`, `input.png`

- [ ] **1.5** Set up `electron-store` for config persistence
  - Map all 19 save.txt fields to JSON config schema
  - Add import/export function for original `save.txt` format

---

## Phase 2 — Protocol Layer (`src/main/`)

- [ ] **2.1** `protocol.js` — Command constants & packet parser
  - Export all command code constants (INIT, DISCONNECT, BYPASS_ON, etc.)
  - `parseTcpPacket(str)` → `{ p3, p5, p6, p7, p8, p9, p10, p11 }`
  - `parseUdpPacket(str)` → `{ p1, p2, p4, p12 }`
  - `parseSerialPacket(str)` → auto-detect 4-field vs 8-field

- [ ] **2.2** `serial.js` — SerialPort communication
  - List available ports (`SerialPort.list()`)
  - Open port with configured baud/databits/parity/stopbits
  - Send command (newline-terminated: `command + '\n'`)
  - Emit `data` events with parsed packets
  - Handle connection handshake (send 11 → wait 500ms → send 99 → reopen → send 11)
  - Auto-reconnect on disconnect
  - Expose: `connect()`, `disconnect()`, `sendCommand(cmd)`

- [ ] **2.3** `tcp.js` — TCP client
  - Connect to configured IP:port
  - Send command (`Buffer.from(cmd, 'utf8')`)
  - Continuous receive loop (10ms sleep between reads)
  - Parse 512-byte buffer as UTF-8, split on comma
  - Emit `data` events with parsed 8-field packets
  - Auto-reconnect on disconnect
  - Expose: `connect()`, `disconnect()`, `sendCommand(cmd)`

- [ ] **2.4** `udp.js` — UDP client
  - Bind to local socket
  - Connect to configured IP:udpPort
  - Continuous receive loop (10ms sleep)
  - Parse 512-byte buffer as UTF-8, split on comma
  - Emit `data` events with parsed 4-field packets
  - Expose: `connect()`, `disconnect()`, `sendCommand(cmd)`

- [ ] **2.5** `transport.js` — Unified transport abstraction
  - Wraps serial/tcp/udp behind a single interface
  - Selects transport based on config mode (`USB` vs `LAN`)
  - For LAN mode: runs both tcp.js and udp.js simultaneously
  - Exposes: `connect()`, `disconnect()`, `sendCommand(cmd)`, events: `data`, `connected`, `disconnected`, `error`

- [ ] **2.6** `config.js` — Configuration management
  - Read/write all 19 config fields via electron-store
  - `importSaveTxt(filePath)` — parse original save.txt line-by-line
  - `exportSaveTxt(filePath)` — write save.txt in original format
  - Provide defaults for all fields

---

## Phase 3 — IPC Bridge (`src/preload/`)

- [ ] **3.1** `preload.js` — Expose safe API to renderer via `contextBridge`
  - `window.amp.connect()`
  - `window.amp.disconnect()`
  - `window.amp.sendCommand(cmd)`
  - `window.amp.onData(callback)` — real-time telemetry updates
  - `window.amp.onConnectionChange(callback)`
  - `window.amp.getConfig()`
  - `window.amp.saveConfig(config)`
  - `window.amp.listPorts()` — returns available COM ports
  - `window.amp.importSaveTxt()`
  - `window.amp.exportSaveTxt()`

- [ ] **3.2** `main/ipc.js` — Register all IPC handlers
  - Wire up all preload API calls to transport/config modules
  - Forward transport `data` events → renderer via `webContents.send`

---

## Phase 4 — Main Window UI (`src/renderer/`)

- [ ] **4.1** `App.svelte` — Root component
  - Dark theme CSS variables
  - Layout: 900×371 minimum
  - Handle always-on-top via IPC
  - Subscribe to telemetry data store

- [ ] **4.2** `stores/telemetry.js` — Svelte store for live data
  - Reactive stores for: `p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12`
  - Derived stores: `outputPowerW`, `reflectedPowerW`, `currentA`, `tempDisplay`, `voltageV`, `efficiencyPct`, `inputPowerW`
  - `isConnected`, `isTransmitting`, `isBypassed`, `errorCode`, `errorText`

- [ ] **4.3** `stores/config.js` — Svelte store wrapping electron-store config

- [ ] **4.4** `components/PowerGauge.svelte` — Output power gauge
  - SVG arc or bar gauge
  - Scale: 0–776 (raw) mapped to 0–max watts display
  - Peak hold with 1500ms decay timer
  - Color: green → yellow → red based on level

- [ ] **4.5** `components/ReflectedGauge.svelte` — Reflected power gauge
  - Similar to PowerGauge
  - Peak hold with 1500ms decay timer

- [ ] **4.6** `components/CurrentGauge.svelte` — Current display
  - Numeric display: `p4 / 10` A, 1 decimal place
  - Peak hold with 1500ms decay timer

- [ ] **4.7** `components/InputPower.svelte` — Input power display
  - Numeric: `p12 * p12 / 476` (USB) or `p12 * p12 / 540` (LAN)
  - Peak hold with 1500ms decay timer

- [ ] **4.8** `components/TempDisplay.svelte` — Temperature display
  - Numeric: `p3` °C or `18 * p3 / 10 + 32` °F
  - Respects config temperature unit setting

- [ ] **4.9** `components/VoltageDisplay.svelte` — Voltage display
  - Numeric: `p5 / 10` V

- [ ] **4.10** `components/EfficiencyDisplay.svelte` — Efficiency %
  - Calculated per PRD formula (koef × inputIndicator dependent)

- [ ] **4.11** `components/StatusLabel.svelte` — Error/Status indicator
  - Shows error text when `p8 != 0`
  - Clickable → sends `"23"` (reset) command
  - Green when OK, Red when error

- [ ] **4.12** `components/AirBypassLabel.svelte` — ON AIR / ByPass control
  - Clickable label
  - Idle: shows "ByPass" — Green (not bypassed) or Magenta (bypassed)
  - Transmitting (`p9 != 0`): shows "ON AIR" in Red
  - Click when idle → toggles bypass (`"21"` / `"22"`)
  - Uses `p11` from TCP to sync bypass state from device

- [ ] **4.13** `components/FanControl.svelte` — Fan mode label
  - Clickable: toggles Fan Auto (`"46"`) / Fan 100% (`"45"`)
  - Shows current mode as label text

- [ ] **4.14** `components/AntennaButton.svelte` — Antenna selector
  - Cycles antenna 1 → 2 → 3 → 1
  - Sends `"31"` / `"32"` / `"33"` on click
  - Shows current antenna number

- [ ] **4.15** `components/BandButton.svelte` — Band selector
  - Only active when `cat == 5` (Manual mode) AND connected AND not transmitting
  - Cycles through 8 bands: 160 → 80 → 40 → 30 → 20 → 17-15 → 12-10 → 6
  - Sends `"71"`–`"78"` on change
  - Auto-selects antenna per `antennaMap[bandIndex]` from config

- [ ] **4.16** `components/VoltsButton.svelte` — Voltage mode toggle
  - Toggles Volts Normal (`"41"`) / Volts+ (`"42"`)

- [ ] **4.17** `components/ConnectButton.svelte` — Connection toggle
  - Connect/Disconnect button
  - Shows connection state

- [ ] **4.18** `components/SetupButton.svelte` — Opens setup modal

- [ ] **4.19** Sound alerts
  - Play `alert.wav` on connect/disconnect
  - Play `error.wav` on error state (p8 != 0)
  - Respect sound enabled config flag

---

## Phase 5 — Setup Dialog UI

- [ ] **5.1** `Setup.svelte` — Modal setup dialog
  - Panel: Connection mode radio (USB / LAN)
    - USB: COM port dropdown (populated from `listPorts()`)
    - LAN: IP address text input
  - Panel: Power/Koef radio (600 / 1200 / 2400)
  - Panel: Voltage radio (48 / 50 / 53.5 / 58.3)
  - Panel: CAT protocol radio (RF / Icom / Yaesu / Kenwood+Flex / Anan+SunSDR / Manual)
  - Panel: Antenna map — 8 inputs, labels: 160 / 80 / 40 / 30 / 20 / 17-15 / 12-10 / 6
    - Each input: max 1 char, values 0–3 only
  - Panel: Options checkboxes
    - Always On Top
    - Fahrenheit
    - Sound alerts
    - Input indicator
  - "Save and Exit" button
  - Validation: connection mode + com port or IP must be set; all 8 antenna fields required

- [ ] **5.2** TCP/UDP port inputs (in LAN section of setup)
  - Separate TCP port and UDP port fields

---

## Phase 6 — Packaging & Distribution

- [ ] **6.1** Configure `electron-builder.yml`
  - Windows: NSIS installer + portable exe
  - macOS: DMG + universal binary (Intel + Apple Silicon)
  - Linux: AppImage + deb

- [ ] **6.2** Set app icon
  - Use `zr.ico` from decompiled resources (or create new icon)

- [ ] **6.3** Code signing (optional, for distribution)
  - Windows: code signing certificate
  - macOS: Apple Developer ID

- [ ] **6.4** GitHub Actions CI/CD
  - Build all 3 platforms on push to main
  - Upload artifacts to GitHub Releases

---

## Phase 7 — Testing & Validation

- [ ] **7.1** Protocol unit tests
  - Test `parseTcpPacket()` with sample data
  - Test `parseUdpPacket()` with sample data
  - Test `parseSerialPacket()` 4-field vs 8-field detection
  - Test efficiency formula for all 6 koef/inputIndicator combinations

- [ ] **7.2** Mock amplifier device (for testing without hardware)
  - Node.js script that opens a TCP/UDP server
  - Sends fake telemetry packets on a timer
  - Responds to connect/disconnect handshake

- [ ] **7.3** Hardware integration test
  - Connect to real amplifier over USB
  - Connect over LAN (TCP + UDP)
  - Verify all control commands trigger correct response
  - Verify all telemetry fields display correctly

- [ ] **7.4** Cross-platform smoke tests
  - Run on Windows 10/11
  - Run on macOS (Intel)
  - Run on Ubuntu 22.04

---

## Reference Files

| File | Description |
|------|-------------|
| `docs/PRD.md` | Full product requirements (this project) |
| `docs/TASKS.md` | This file |
| `docs/PROTOCOL.md` | Protocol quick-reference card |
| `decompiled/HeliosDX/Main.cs` | Original decompiled main form (v3.0.0.0) |
| `decompiled/HeliosDX/Setup.cs` | Original decompiled setup form |
| `decompiled/resources/` | Original audio/image assets |
| `Original/Helios DX.exe` | Original binary (keep for reference) |

---

## Notes & Decisions

- **Config format**: Use `electron-store` (JSON) natively. Keep save.txt import/export for migration.
- **Serial newline**: Always append `\n` to serial commands (v3 firmware requirement).
- **Bypass state**: Driven by `p11` from TCP packet (v3 behavior), not client-side only.
- **Band commands**: Always send `"71"`–`"78"` on band change (NOT just UI update).
- **CAT value**: Store raw index 0–5 in config; send raw value to device (no +61 offset).
- **Sleep command**: Keep `"44"` in protocol layer even though original UI removed the button — expose as optional feature.
- **Window size**: Target 900×371 minimum; allow resizing.
- **Peak hold timers**: 1500ms decay for all four gauges (op, rp, cr, in).
