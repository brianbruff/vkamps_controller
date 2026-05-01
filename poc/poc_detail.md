# VKAmp POC — Button & Message Audit

Every interactive surface in the renderer, what it does, the IPC channel it hits, and the protocol byte it puts on the wire (if any). All paths verified end-to-end against `App.svelte` → `preload/preload.js` → `main/ipc.js` → `main/transport.js`.

---

## 1. Header

| Element | What it does | Wired? | Path |
|---|---|---|---|
| **DIAG** button (`HeaderBar.svelte:36`) | Opens the full-screen DiagnosticsView | ✅ | `App.onDiagnostics` → `diagOpen=true` (no IPC) |
| **POC** corner badge | Static label only — not interactive | n/a | — |
| **IP input field** (LAN modes only, `ConnectControl.svelte:125`) | Buffers a draft IP. Persists to `lanIp` only when you click **Connect**. | ✅ | committed via `amp:saveConfig` |
| **Mode/transport label** ("USB" / "TCP" / "UDP" / "MOCK") | Static — read-only display of `settings.mode` | n/a | — |
| **Connect / Disconnect** button (`ConnectControl.svelte:137`) | Toggles connection. Spinner + "Connecting…" while in flight. Opens Settings if no `comPort` (USB) or no `lanIp` (LAN). | ✅ | `amp:connect` / `amp:disconnect` |

---

## 2. Status pills (`App.svelte:230–245`)

| Pill | Click behavior | Cmd | Wired? |
|---|---|---|---|
| **STATUS OK / ERR \*** | When `errorCode > 0`, click sends Reset; otherwise inert | `23` | ✅ |
| **ON AIR / STANDBY / BYPASS** | Toggles bypass (mirror of footer ByPass) | `21` / `22` | ✅ |
| **FAN AUTO / FAN 100%** | Toggles fan mode | `45` / `46` | ✅ |

---

## 3. Stat cards (`App.svelte:248–289`)

| Card | Type | Click behavior | Cmd | Wired? |
|---|---|---|---|---|
| **ANTENNA** | cycle | 1 → 2 → 3 → 4 → 1 | `31` / `32` / `33` / `34` | ✅ |
| **BAND** | cycle (only when CAT = Manual) | next of 160/80/40/30/20/17–15/12–10/6 | `71`…`78` | ✅ — gated on `settings.cat === 5` |
| **SWR** | readonly | — | — | n/a |
| **EFFICIENCY** | readonly | — | — | n/a |
| **VOLTS** | toggle | flips between Normal / Volts+ | `41` / `42` | ✅ |
| **TEMP** | readonly | — | — | n/a |

> ⚠️ Known limitation: ANTENNA cycles raw 1→4 and does **not** consult the per-band antenna map in settings. Listed as a deferred TODO in the design.

---

## 4. Footer (`App.svelte:292–297`)

| Button | Cmd | Wired? | Notes |
|---|---|---|---|
| **Reset** | `23` | ✅ | Also clears the local "asleep" latch |
| **Sleep** | `44` | ✅ | Visual latch for 6 s OR until next state packet arrives |
| **ByPass** | `21` / `22` | ✅ | Same handler as ON AIR pill |
| **Setup** | — | ✅ | Opens SettingsModal (no IPC) |

---

## 5. Settings modal (`SettingsModal.svelte`)

| Control | Persisted via | Side-effect on Save | Wired? |
|---|---|---|---|
| ✕ / **Cancel** | n/a | discards the draft | ✅ |
| **Save and Apply** | `amp:saveConfig` (always) | fires `41/42`, `51`–`54`, `61`–`66`, `setAlwaysOnTop`, and a disconnect+reconnect if any connection field changed | ✅ |
| Connection radios (USB / TCP / UDP) | Save | reconnect if changed | ✅ |
| COM port `<select>` | Save | populated from `amp:listPorts` on modal open | ✅ |
| Baud `<select>` | Save | reconnect if changed | ✅ |
| IP / TCP port / UDP port | Save | reconnect if changed | ✅ |
| Koef radios (600/1200/2400) | Save | rescales gauges (no protocol cmd — derive only) | ✅ |
| Voltage radios (48/50/53.5/58.3) | Save | sends `51`/`52`/`53`/`54` if changed | ✅ |
| CAT radios (RF/Icom/Yaesu/Kenwood/Anan/Manual) | Save | sends `61`+index if changed | ✅ |
| Antenna map (8 inputs) | Save | persisted only — no protocol cmd | ✅ |
| Always on top checkbox | Save | calls `amp:setAlwaysOnTop(v)` if changed | ✅ |
| Fahrenheit checkbox | Save | display-only | ✅ |
| Sound alerts checkbox | Save | display-only (no audio wired yet) | ⚠️ stored but inert |
| Input indicator checkbox | Save | affects efficiency formula path | ✅ |
| Verbose RX logging checkbox | Save | toggles per-packet RX diag entries on transports | ✅ |
| **Import save.txt…** | `amp:importSaveTxt` | reads indexed-line format and overwrites all settings | ✅ |
| **Export save.txt…** | `amp:exportSaveTxt` | writes current settings in original format | ✅ |

---

## 6. Diagnostics view (`DiagnosticsView.svelte`)

| Control | Behavior | IPC | Wired? |
|---|---|---|---|
| **Clear** | Empties the main-process ring buffer + local view | `diag:clear` | ✅ |
| **Save log…** | Opens save dialog (Desktop → Documents → home), writes plain-text dump | `diag:save` | ✅ |
| ✕ close | Closes overlay; Esc also closes | — | ✅ |
| Level filter dropdown | Filters local view by level | — | ✅ |
| Category filter dropdown | Filters local view by category | — | ✅ |
| Search input | Live substring match on message + stringified detail | — | ✅ |
| Auto-scroll checkbox | Pins scroll to bottom on new entries; auto-unchecks if user scrolls up | — | ✅ |
| **↓ Bottom** button (only when auto-scroll off) | Re-enables auto-scroll and pins to bottom | — | ✅ |

---

## 7. Messages we listen for (renderer ← main)

All subscribed in `App.svelte:onMount` and `DiagnosticsView`.

| Channel | Payload | Handler |
|---|---|---|
| `amp:data` | `{ type: 'tcp'\|'udp', data: { p1,p2,p4,p12 } or { p3,p5,p6,p7,p8,p9,p10,p11 } }` | `applyMeterPacket` / `applyStatePacket` + `applyStateMeters`, also clears Sleep latch |
| `amp:connection` | `boolean` | `setStatus('open' \| 'closed')` |
| `amp:error` | `string` (transport error message) | `setError(msg)` |
| `diag:entry` | `{ ts, level, category, message, detail? }` | DiagnosticsView appends to its rolling buffer |
| `diag:cleared` | (no payload) | DiagnosticsView empties its local view |

---

## 8. Messages we send (renderer → main IPC handlers in `main/ipc.js`)

| Channel | Used by | Main-process effect |
|---|---|---|
| `amp:connect` | Connect button, Settings post-save, IP-change reconnect | reads config, opens TCP+UDP or Serial, fires post-connect CAT + voltage cmds |
| `amp:disconnect` | Disconnect button, Settings post-save | closes whichever transports are open |
| `amp:sendCommand` | every footer/pill/card/Settings click that maps to a protocol cmd | logs `cmd NN (NAME)`, calls `transport.sendCommand(cmd)` |
| `amp:getConfig` | settings store on load, settings modal on open, transport on every connect | returns the persisted config |
| `amp:saveConfig` | settings store, ConnectControl on Connect (commits draft IP) | writes via electron-store; logs which keys changed |
| `amp:listPorts` | Settings modal `loadPorts()` | enumerates serial ports (returns `[]` for mock) |
| `amp:importSaveTxt` / `amp:exportSaveTxt` | Profile section in Settings | open dialog + read/write indexed-line format |
| `amp:setAlwaysOnTop` | App.onMount, Settings save | calls `BrowserWindow.setAlwaysOnTop` |
| `amp:isMock` | App.onMount | tells the renderer if it's a mock build |
| `amp:mock` | DevPanel only — present only in mock builds | mock-transport knobs (drive, error inject, drop conn, etc.) |
| `amp:getProtocol` | currently unused by the renderer | exposes CMD/BANDS/etc. constants — dead reference |
| `diag:getAll` | DiagnosticsView open | backfills the rolling buffer |
| `diag:clear` | Clear button | `diag.clear()` + emits `diag:cleared` |
| `diag:save` | Save log… button | dumps to user-chosen path |
| `diag:logUi` | ConnectControl, DevPanel — anywhere the renderer wants a diag entry | writes a `category=ui` entry to the bus |

---

## 9. Issues / gaps worth noting

1. **Sound alerts checkbox** is stored but no audio is hooked up.
2. **Antenna cycling** ignores the per-band antenna map — always 1→3.
3. **`amp:getProtocol`** IPC is exposed in preload but no renderer code calls it. Dead weight, harmless.
4. **`amp:sendCommand`** is fire-and-forget on the renderer side (no `await`). That's intentional — telemetry confirms state change — but it means a failed send (e.g., transport closed) doesn't surface to the user except via the diag log.
5. **No-op cmds when not connected**: footer/pill/card clicks send commands even with no transport open; `transport.sendCommand` silently no-ops. Intentional but the user gets no feedback that the click "didn't take." A small improvement would be to disable interactive controls when `transportState.status !== 'open'`.
