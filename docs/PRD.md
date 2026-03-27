# Product Requirements Document
## Helios DX — Cross-Platform Electron Replacement
**Project:** VKAmp
**Source App:** Helios DX v3.0.0.0 by RZ1ZR
**Author:** Reverse-engineered from `Original/Helios DX.exe` (.NET 4.8 WinForms)
**Date:** 2026-03-27
**Status:** Planning
**UI Design Reference:** `docs/newDesign.webp`

---

## 1. Overview

VKAmp is a cross-platform desktop replacement for the proprietary **Helios DX** Windows-only application. Helios DX is a remote control panel for an amateur radio HF/VHF power amplifier (RZ1ZR brand). The original app is a Windows-only .NET 4.8 WinForms application.

The replacement will be built with **Electron + Svelte**, targeting Windows, macOS, and Linux.

---

## 2. Goals

- Full feature parity with Helios DX v3.0.0.0
- Cross-platform: Windows, macOS, Linux
- Modern UI with real-time gauges and indicators
- Support all three transport modes: USB/Serial, TCP, UDP
- Maintain backward compatibility with the existing amplifier firmware protocol

---

## 3. Target Hardware

The app controls a **remote RF power amplifier** with the following characteristics:
- Multi-band HF/VHF amplifier (160m – 6m amateur radio bands)
- Up to 4 selectable antenna ports per band
- Temperature monitoring, power/current/voltage telemetry
- Bypass mode, cooling fan control, sleep/standby
- CAT (Computer Aided Transceiver) integration with major radio brands

---

## 4. Communication Protocol

### 4.1 Transport Layer

The app supports three transport modes, selectable in settings:

#### USB/Serial Mode
- Interface: `SerialPort` (COM port, e.g. COM5)
- Baud Rate: 600 / 1200 / 2400 (user-configurable)
- Data Bits: 8, Parity: None, Stop Bits: 1, Handshake: None
- DTR: disabled
- Write Timeout: 200ms, Read Timeout: 200ms
- **Commands MUST be newline-terminated** (`\n`) — v3.0.0 firmware requirement

#### TCP Mode (LAN)
- Protocol: TCP/IP (`SocketType.Stream`)
- Default IP: `192.168.0.55`
- Port: configurable (save.txt array[6])
- Encoding: UTF-8
- Receive buffer: 512 bytes
- Polling: continuous loop with 10ms sleep

#### UDP Mode (LAN)
- Protocol: UDP (`SocketType.Dgram`)
- Default IP: `192.168.0.55`
- Port: configurable (save.txt array[7], separate from TCP port)
- Encoding: UTF-8
- Receive buffer: 512 bytes
- Polling: continuous loop with 10ms sleep

### 4.2 Connection Handshake (all transports)
```
1. Open connection
2. Send "11"         → initialize
3. Wait 500ms
4. Send "99"         → reset
5. Re-open connection
6. Send "11"         → initialize again
7. Begin polling loop
```

On disconnect/exit: Send `"99"` before closing.

### 4.3 Command Codes (all sent as ASCII strings)

| Command | Description |
|---------|-------------|
| `"11"`  | Initialize / Connect |
| `"99"`  | Disconnect / Reset |
| `"21"`  | Bypass ON |
| `"22"`  | Bypass OFF |
| `"23"`  | Reset / Clear Error |
| `"31"`  | Select Antenna 1 |
| `"32"`  | Select Antenna 2 |
| `"33"`  | Select Antenna 3 |
| `"34"`  | Select Antenna 4 |
| `"41"`  | Volts Normal |
| `"42"`  | Volts+ (boost) |
| `"44"`  | Sleep / Standby |
| `"45"`  | Fan 100% |
| `"46"`  | Fan Auto |
| `"51"`  | Voltage config: 48V |
| `"52"`  | Voltage config: 50V |
| `"53"`  | Voltage config: 53.5V |
| `"54"`  | Voltage config: 58.3V |
| `"61"`–`"66"` | CAT protocol select (1=RF, 2=Icom, 3=Yaesu, 4=Kenwood/Flex, 5=Anan/SunSDR, 6=Manual) |
| `"71"`  | Band change: 160m |
| `"72"`  | Band change: 80m |
| `"73"`  | Band change: 40m |
| `"74"`  | Band change: 30m |
| `"75"`  | Band change: 20m |
| `"76"`  | Band change: 17-15m |
| `"77"`  | Band change: 12-10m |
| `"78"`  | Band change: 6m |

### 4.4 Incoming Data Packets

#### TCP Packet (8 comma-separated integers)
```
p3,p5,p6,p7,p8,p9,p10,p11
```
| Field | Description | Notes |
|-------|-------------|-------|
| `p3`  | Temperature | °C raw; display = `18 * p3 / 10 + 32` for °F |
| `p5`  | Voltage (tenths) | divide by 10 for display |
| `p6`  | Band indicator | integer band index |
| `p7`  | Antenna selection | 1, 2, or 3 |
| `p8`  | Error/Status code | see error table below |
| `p9`  | Device status | 0 = OK, non-zero = error/offline |
| `p10` | Reserved | additional status |
| `p11` | Bypass state feedback | 1 = bypassed (v3 uses this to drive bypass state) |

#### UDP Packet (4 comma-separated integers)
```
p1,p2,p4,p12
```
| Field | Description | Notes |
|-------|-------------|-------|
| `p1`  | Output Power | scaled 0–776 |
| `p2`  | Reflected Power | scaled 0–`sizeind1` |
| `p4`  | Current (tenths) | divide by 10 for display in Amps |
| `p12` | Input Power | raw value |

#### Serial Packet (alternating 4-field / 8-field CSV)
- 8-field line: same as TCP packet (`p3,p5,p6,p7,p8,p9,p10,p11`)
- 4-field line: same as UDP packet (`p1,p2,p4,p12`)

### 4.5 Error / Status Codes (p8 field)

| Code | Meaning | Indicator Color |
|------|---------|-----------------|
| `0`  | OK | Green |
| `1`  | Error: Input Power | Red |
| `2`  | Error: Power | Red |
| `3`  | Error: REF (Reflected) | Red |

### 4.6 Polling / Timing
| Parameter | Value |
|-----------|-------|
| TCP/UDP receive loop sleep | 10ms |
| Gauge hold-down timer | 1500ms (op, rp, cr, in timers) |
| Serial write timeout | 200ms |
| Serial read timeout | 200ms |
| Handshake delay | 500ms |

---

## 5. Configuration / Persistence

Config is stored in a plain text file (`save.txt`) — one value per line:

| Index | Key | Values | Default |
|-------|-----|--------|---------|
| 0 | COM Port | e.g. `"COM5"` | `"COM5"` |
| 1 | Baud Rate | `"600"`, `"1200"`, `"2400"` | `"600"` |
| 2 | Window X position | integer string | `"0"` |
| 3 | Window Y position | integer string | `"0"` |
| 4 | Connection Mode | `"USB"` or `"LAN"` | `"USB"` |
| 5 | LAN IP Address | e.g. `"192.168.0.55"` | `"192.168.0.55"` |
| 6 | TCP Port | port number string | `"8080"` |
| 7 | UDP Port | port number string | `"8081"` |
| 8 | Always On Top | `"true"` / `"false"` | `"false"` |
| 9 | Bypass state | `"true"` / `"false"` | `"false"` |
| 10 | Temperature Unit | `"C"` or `"F"` | `"C"` |
| 11 | Antenna number (current) | `"1"`–`"4"` | `"1"` |
| 12 | Volts mode | `"true"` / `"false"` | `"false"` |
| 13 | Power/Koef | `"600"`, `"1200"`, `"2400"` | `"600"` |
| 14 | Voltage Reference | `"48"`, `"50"`, `"53.5"`, `"58.3"` | `"48"` |
| 15 | CAT Protocol | `"0"`–`"5"` | `"0"` |
| 16 | Antenna Map | 8 comma-separated values 0–3 | `"1,1,1,1,1,1,1,1"` |
| 17 | Sound Alerts | `"True"` / `"False"` | `"False"` |
| 18 | Input Indicator | `"True"` / `"False"` | `"False"` |

In the Electron app, `electron-store` (JSON) should be used instead of save.txt, but the app should also be able to **import/export** the original save.txt format for compatibility.

---

## 6. CAT Protocol Modes

| Index | Protocol | Target Radios |
|-------|----------|---------------|
| 0 | RF (default) | RF-only mode |
| 1 | Icom | Icom transceivers |
| 2 | Yaesu | Yaesu transceivers |
| 3 | Kenwood / Flex | Kenwood & FlexRadio |
| 4 | Anan / SunSDR | Anan & SunSDR devices |
| 5 | Manual | Manual band switching |

---

## 7. UI Requirements

### 7.1 Main Window (900×371 equivalent)
Dark theme. Key display elements:

| Element | Description |
|---------|-------------|
| **Output Power gauge** (`p1`) | Bar/arc gauge, 0–776 scale |
| **Reflected Power gauge** (`p2`) | Bar/arc gauge |
| **Current display** (`p4`) | Numeric, tenths (e.g. `12.5 A`) |
| **Input Power display** (`p12`) | Numeric |
| **Temperature display** (`p3`) | Numeric, °C or °F selectable |
| **Voltage display** (`p5`) | Numeric, tenths (e.g. `48.0 V`) |
| **Band label** | Current band (160m, 80m … 6m) |
| **Antenna indicator** | Current antenna number (1–4) |
| **Status/Error label** | Shows error text, clickable to reset |
| **ON AIR / ByPass label** | Clickable — toggles bypass, shows TX state |
| **Fan label** | Clickable — toggles Fan Auto / Fan 100% |
| **Band button** | Cycles through bands (only in Manual/CAT=5 mode) |
| **Antenna button** | Cycles through antennas 1–3 |
| **Volts button** | Toggles Volts / Volts+ |
| **Setup label/button** | Opens Setup dialog |
| **Connect indicator** | Shows connected/disconnected state |

### 7.2 Indicator Colors
| State | Color |
|-------|-------|
| Connected / OK | Green |
| Transmitting (ON AIR) | Red |
| Bypass active | Magenta |
| Error | Red |
| Offline / disconnected | Gray |

### 7.3 Setup Dialog
Separate modal window with panels:
- **Connection**: USB (COM port selector) or LAN (IP address input)
- **Power/Koef**: Radio buttons 600 / 1200 / 2400
- **Voltage**: Radio buttons 48 / 50 / 53.5 / 58.3
- **CAT**: Radio buttons for 6 protocol options
- **Antenna Map**: 8 text inputs (one per band), values 0–3, labeled 160/80/40/30/20/17-15/12-10/6
- **Options**: Always on top checkbox, Fahrenheit checkbox, Sound alerts checkbox, Input indicator checkbox
- **Save and Exit** button

### 7.4 Efficiency Display
Calculated from received data. Formula depends on koef (baud rate) and inputIndicator setting:

| Koef | inputIndicator | Formula |
|------|----------------|---------|
| 600 | false | `p1*p1/952 * 100 / (p5/10 * (p4*66/100) / 10)` |
| 600 | true | `p1*p1/952 * 100 / (p5/10 * (p4*102/100) / 10)` |
| 1200 | false | `p1*p1/476 * 100 / (p5/10 * (p4*132/100) / 10)` |
| 1200 | true | `p1*p1/476 * 100 / (p5/10 * (p4*204/100) / 10)` |
| 2400 | false | `p1*p1/238 * 100 / (p5/10 * (p4*264/100) / 10)` |
| 2400 | true | `p1*p1/238 * 100 / (p5/10 * (p4*408/100) / 10)` |

Input power display: `p12 * p12 / 476` (USB) or `p12 * p12 / 540` (LAN/UDP)

---

## 8. Tech Stack

| Layer | Technology |
|-------|-----------|
| Shell | Electron (latest stable) |
| UI Framework | Svelte 5 |
| Build tool | Vite |
| Serial port | `serialport` npm package |
| TCP/UDP | Node.js built-in `net` / `dgram` |
| Config storage | `electron-store` (JSON) |
| Packaging | `electron-builder` |
| Styling | CSS custom properties (dark theme) |
| Gauges | SVG or Canvas (no heavy charting lib needed) |

### Key npm packages
```json
{
  "serialport": "^12.x",
  "electron-store": "^8.x",
  "electron-builder": "^24.x",
  "vite": "^5.x",
  "svelte": "^5.x"
}
```

---

## 9. Non-Functional Requirements

| Requirement | Target |
|-------------|--------|
| Platforms | Windows 10+, macOS 12+, Ubuntu 22+ |
| Bundle size | < 200MB installed |
| CPU usage (idle) | < 5% |
| Latency (command → response) | < 100ms perceived |
| Auto-reconnect | Yes — retry on disconnect |
| Window: Always on Top | Optional (configurable) |
| Sound alerts | Optional (configurable), .wav files embedded |
| save.txt import | Yes — for migration from original app |

---

## 10. Out of Scope (v1)

- Mobile (iOS/Android)
- Cloud/remote access beyond LAN
- Firmware updates to the amplifier
- CAT passthrough to radio software (e.g. WSJT-X, FT8CN)
- Logging / QSO recording
