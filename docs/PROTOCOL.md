# Helios DX Protocol — Quick Reference Card
**Target firmware:** v3.0.0.0 (Helios DX.exe, 557KB)

---

## Transport Modes

| Mode | Setting | Details |
|------|---------|---------|
| USB/Serial | `array[4] = "USB"` | COM port, baud 600/1200/2400, 8N1, newline-terminated |
| LAN TCP | `array[4] = "LAN"` | TCP stream to `IP:tcpPort`, UTF-8, 512-byte buffer |
| LAN UDP | `array[4] = "LAN"` | UDP dgram to `IP:udpPort`, UTF-8, 512-byte buffer (runs alongside TCP) |

---

## Handshake Sequence
```
→ send "11\n"
  wait 500ms
→ send "99\n"
  reopen connection
→ send "11\n"
  begin polling
```
On exit: `→ send "99\n"` then close.

---

## Commands (ASCII, newline-terminated for serial)

```
"11"  connect/init          "99"  disconnect/reset
"21"  bypass ON             "22"  bypass OFF
"23"  reset/clear error
"31"  antenna 1             "32"  antenna 2
"33"  antenna 3
"41"  volts normal          "42"  volts+
"44"  sleep/standby
"45"  fan 100%              "46"  fan auto
"51"  voltage → 48V         "52"  voltage → 50V
"53"  voltage → 53.5V       "54"  voltage → 58.3V
"61"  CAT: RF               "62"  CAT: Icom
"63"  CAT: Yaesu            "64"  CAT: Kenwood/Flex
"65"  CAT: Anan/SunSDR      "66"  CAT: Manual
"71"  band: 160m            "72"  band: 80m
"73"  band: 40m             "74"  band: 30m
"75"  band: 20m             "76"  band: 17-15m
"77"  band: 12-10m          "78"  band: 6m
```

---

## Incoming Packets

### TCP (8 fields):
```
p3,p5,p6,p7,p8,p9,p10,p11

p3  = temperature raw  → °C display; °F = 18*p3/10 + 32
p5  = voltage (×10)    → display = p5/10 V
p6  = band index (1-8: 1=160m, 2=80m, 3=40m, 4=30m, 5=20m, 6=17-15m, 7=12-10m, 8=6m)
p7  = antenna (1/2/3)
p8  = error code (0=OK, 1=input pwr, 2=power, 3=REF)
p9  = device status (0=OK, !0=error/offline)
p10 = reserved
p11 = bypass state (1=bypassed)
```

### UDP (4 fields):
```
p1,p2,p4,p12

p1  = output power (0–776 scale)
p2  = reflected power
p4  = current (×10)    → display = p4/10 A
p12 = input power raw
```

### Serial: alternates between 8-field (TCP-style) and 4-field (UDP-style) lines.

---

## Derived Values

```
Input power display:
  USB mode: p12 * p12 / 476
  LAN mode: p12 * p12 / 540

Efficiency % (koef from config, raw values):
  koef=600,  !input: p1²/952 * 100 / (p5/10 * (p4*66/100)  / 10)
  koef=600,   input: p1²/952 * 100 / (p5/10 * (p4*102/100) / 10)
  koef=1200, !input: p1²/476 * 100 / (p5/10 * (p4*132/100) / 10)
  koef=1200,  input: p1²/476 * 100 / (p5/10 * (p4*204/100) / 10)
  koef=2400, !input: p1²/238 * 100 / (p5/10 * (p4*264/100) / 10)
  koef=2400,  input: p1²/238 * 100 / (p5/10 * (p4*408/100) / 10)
```

---

## Timing
```
Serial write/read timeout : 200ms
TCP/UDP poll sleep        : 10ms
Gauge peak-hold decay     : 1500ms
Handshake delay           : 500ms
```

---

## Config File (save.txt) — Line Index Map
```
[0]  COM port           [10] temp unit (C/F)
[1]  baud rate          [11] antenna number
[2]  window X           [12] volts mode
[3]  window Y           [13] koef (600/1200/2400)
[4]  mode (USB/LAN)     [14] voltage (48/50/53.5/58.3)
[5]  LAN IP             [15] CAT index (0–5)
[6]  TCP port           [16] antenna map (8 CSV values 0–3)
[7]  UDP port           [17] sound (True/False)
[8]  always on top      [18] input indicator (True/False)
[9]  bypass state
```
