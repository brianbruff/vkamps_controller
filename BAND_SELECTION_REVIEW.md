# Band Selection and Reporting Logic Review

## Executive Summary

**CRITICAL OFF-BY-ONE ERROR IDENTIFIED** in the legacy .NET application (decompiled/HeliosDX/Main.cs).

The **modern Electron frontend is CORRECT** and uses consistent 0-based indexing throughout.

---

## Protocol Specification

### Device Communication Protocol
- **Band Index (`p6`)**: 0-based values (0-7)
  - 0 = 160m
  - 1 = 80m
  - 2 = 40m
  - 3 = 30m
  - 4 = 20m
  - 5 = 17-15m
  - 6 = 12-10m
  - 7 = 6m

- **Band Commands**: ASCII strings "71" through "78"
  - "71" = Select 160m (band index 0)
  - "72" = Select 80m (band index 1)
  - "73" = Select 40m (band index 2)
  - "74" = Select 30m (band index 3)
  - "75" = Select 20m (band index 4)
  - "76" = Select 17-15m (band index 5)
  - "77" = Select 12-10m (band index 6)
  - "78" = Select 6m (band index 7)

### Command Generation Formula
```
Band Command = 71 + band_index
```
Where `band_index` is 0-7.

### Command Parsing Formula
```
band_index = second_digit_of_command - 1
```
For command "71": second digit is "1", so band_index = 1 - 1 = 0 (160m)

---

## Frontend Analysis (Electron/Svelte) ✅ CORRECT

### File: `poc/renderer/src/lib/stores/deviceState.svelte.js`

**Line 3: Band State Declaration**
```javascript
export const deviceState = $state({
  band: 3,         // 0..7 — 30m default
  ...
});
```
✅ **CORRECT**: Uses 0-based indexing (0-7)

**Line 14: Telemetry Packet Handling**
```javascript
export function applyStatePacket(d) {
  deviceState.band = d.p6 ?? deviceState.band;
  ...
}
```
✅ **CORRECT**: Directly assigns `p6` (0-7) from device to band state

---

### File: `poc/renderer/src/App.svelte`

**Lines 43-44: Band Constants**
```javascript
const BANDS = ['160m', '80m', '40m', '30m', '20m', '17–15m', '12–10m', '6m'];
const BAND_FREQ = ['1.84 MHz', '3.65 MHz', '7.10 MHz', '10.10 MHz', '14.20 MHz', '18.10 MHz', '24.94 MHz', '50.10 MHz'];
```
✅ **CORRECT**: 0-indexed arrays where index 0 = 160m, index 7 = 6m

**Lines 163-167: Band Selection Function**
```javascript
function onBandSelect(i) {
  if (!catManual) return;
  send(String(71 + i));      // Sends commands "71"-"78" for indices 0-7
  deviceState.band = i;
}
```
✅ **CORRECT**: Generates command correctly using formula `71 + i`
- For i=0 (160m): sends "71"
- For i=7 (6m): sends "78"

**Lines 269-274: Band Display**
```javascript
<BandTile
  bands={BANDS}
  activeIndex={deviceState.band}
  freq={BAND_FREQ[deviceState.band] || ''}
  enabled={catManual}
  onselect={onBandSelect} />
```
✅ **CORRECT**: Uses `deviceState.band` (0-7) directly as array index

---

### File: `src/main/protocol.js`

**Lines 26-33: Band Command Constants**
```javascript
BAND_160: '71',    // Index 0
BAND_80: '72',     // Index 1
BAND_40: '73',     // Index 2
BAND_30: '74',     // Index 3
BAND_20: '75',     // Index 4
BAND_17_15: '76',  // Index 5
BAND_12_10: '77',  // Index 6
BAND_6: '78',      // Index 7
```
✅ **CORRECT**: Commands match protocol specification

**Line 37: Band Command Array**
```javascript
const BAND_CMDS = [CMD.BAND_160, CMD.BAND_80, CMD.BAND_40, CMD.BAND_30, CMD.BAND_20, CMD.BAND_17_15, CMD.BAND_12_10, CMD.BAND_6];
```
✅ **CORRECT**: 0-indexed array where `BAND_CMDS[0]` = '71' (160m)

**Line 80: Telemetry Comment**
```javascript
p6: parts[2],   // band index
```
✅ **CORRECT**: Documents that p6 is band index (0-7)

---

### File: `poc/main/mock.js`

**Line 102: Command Parsing**
```javascript
else if (/^7[1-8]$/.test(c)) this.band = Number(c[1]) - 1;
```
✅ **CORRECT**: Decodes band commands correctly
- Command "71": `Number('1') - 1 = 0` (160m)
- Command "78": `Number('8') - 1 = 7` (6m)

---

## Backend Analysis (.NET Legacy Application) ❌ INCORRECT

### File: `decompiled/HeliosDX/Main.cs`

#### **Lines 1557-1592: Telemetry Packet Handler** ❌ OFF-BY-ONE ERROR

```csharp
switch (p6)  // p6 is 0-based index from device (0-7)
{
case 1:      // ❌ ERROR: Should be case 0
    band_label_value.Text = "160";
    antenna = Convert.ToInt32(ant[0]);
    break;
case 2:      // ❌ ERROR: Should be case 1
    band_label_value.Text = "80";
    antenna = Convert.ToInt32(ant[1]);
    break;
case 3:      // ❌ ERROR: Should be case 2
    band_label_value.Text = "40";
    antenna = Convert.ToInt32(ant[2]);
    break;
case 4:      // ❌ ERROR: Should be case 3
    band_label_value.Text = "30";
    antenna = Convert.ToInt32(ant[3]);
    break;
case 5:      // ❌ ERROR: Should be case 4
    band_label_value.Text = "20";
    antenna = Convert.ToInt32(ant[4]);
    break;
case 6:      // ❌ ERROR: Should be case 5
    band_label_value.Text = "17-15";
    antenna = Convert.ToInt32(ant[5]);
    break;
case 7:      // ❌ ERROR: Should be case 6
    band_label_value.Text = "12-10";
    antenna = Convert.ToInt32(ant[6]);
    break;
case 8:      // ❌ ERROR: Should be case 7
    band_label_value.Text = "6";
    antenna = Convert.ToInt32(ant[7]);
    break;
}
band = p6;  // ❌ Stores 0-based value in what should be 1-based variable
```

**Problem Explanation:**
1. Device sends `p6` as 0-based index (0-7)
2. Switch statement treats `p6` as 1-based (cases 1-8)
3. When device sends `p6=0` for 160m band:
   - No case matches (missing `case 0:`)
   - Band display doesn't update
   - 160m band cannot be selected via telemetry
4. When device sends `p6=7` for 6m band:
   - No case matches (missing `case 7:`, has wrong `case 8:`)
   - 6m band display won't work correctly
5. The `ant[]` array indexing is correct (0-based) but gets wrong values due to case mismatch

**Impact:**
- Band 160m (p6=0): No case matches, won't display
- Bands 80m-12/10m (p6=1-6): Display wrong band (shows band for p6+1)
- Band 6m (p6=7): No case matches, won't display

---

#### **Lines 1999-2048: Manual Band Selection Button** ⚠️ INCONSISTENT

```csharp
private void band_button_Click(object sender, EventArgs e)
{
    if (cat != 5) return;

    band++;              // Uses 1-based incrementing
    if (band > 8)
    {
        band = 1;        // Wraps to 1, not 0
    }

    switch (band)        // Uses 1-based values (1-8)
    {
    case 1:
        band_label_value.Text = "160";
        antenna = Convert.ToInt32(ant[0]);
        break;
    case 2:
        band_label_value.Text = "80";
        antenna = Convert.ToInt32(ant[1]);
        break;
    // ... continues through case 8
    }
    // ... sends antenna commands
}
```

**Problem Explanation:**
1. This function uses **1-based** band indexing (1-8)
2. Variable `band` is 1-based within this context
3. But `band = p6` in telemetry handler stores 0-based value
4. This creates an inconsistency where `band` variable has different meanings:
   - After telemetry: 0-based (0-7)
   - After button click: 1-based (1-8)

**Impact:**
- Mixed semantics for the `band` variable throughout the application
- After receiving telemetry, `band` might be 0
- Clicking band button will increment to 1, showing correct band
- But values are semantically inconsistent

---

## Root Cause Analysis

### Primary Bug: Telemetry Handler (Lines 1557-1592)

**The switch statement is off by one.** It should be:

```csharp
switch (p6)
{
case 0:  // Not case 1
    band_label_value.Text = "160";
    antenna = Convert.ToInt32(ant[0]);
    break;
case 1:  // Not case 2
    band_label_value.Text = "80";
    antenna = Convert.ToInt32(ant[1]);
    break;
// ... continue through case 7 (not case 8)
}
```

### Secondary Issue: Variable Semantics Confusion

The `band` variable has inconsistent meaning:
- **Device/Protocol**: 0-based (0-7)
- **Manual Button**: 1-based (1-8)
- **Telemetry Assignment**: Stores 0-based value from p6

**Recommended Fix:**
1. Fix the switch statement to use cases 0-7
2. Keep `band = p6` assignment (0-based)
3. Update button click handler to use 0-based indexing
4. OR: Convert p6 to 1-based when storing: `band = p6 + 1`

---

## Verification Data

### Memory Citation Verification

From repository memory:
> "Device sends 0-indexed band values (0-7 for bands 160m, 80m, 40m, 30m, 20m, 17-15m, 12-10m, 6m)"

✅ **VERIFIED**: This memory is correct and matches the code analysis.

From deviceState.svelte.js:3
```javascript
band: 3,         // 0..7 — 30m default
```

From protocol.js:80
```javascript
p6: parts[2],   // band index
```

---

## Recommendations

### For Legacy .NET Application (if still in use):

**Option 1: Fix to 0-based (Recommended)**
```csharp
// Lines 1557-1591
switch (p6)
{
case 0:  // 160m
    band_label_value.Text = "160";
    antenna = Convert.ToInt32(ant[0]);
    break;
case 1:  // 80m
    band_label_value.Text = "80";
    antenna = Convert.ToInt32(ant[1]);
    break;
// ... through case 7 for 6m
}
band = p6;  // Keep 0-based
```

**Option 2: Convert to 1-based (Alternative)**
```csharp
// Line 1592
band = p6 + 1;  // Convert 0-based p6 to 1-based band

// Keep switch statement as cases 1-8
// This maintains consistency with manual button
```

### For Modern Electron Frontend:

✅ **NO CHANGES NEEDED** - The frontend is correctly implemented with consistent 0-based indexing throughout.

---

## Test Recommendations

If fixing the legacy application:

1. **Test telemetry reception for all bands:**
   - Send p6=0, verify 160m displays
   - Send p6=1, verify 80m displays
   - Send p6=2, verify 40m displays
   - Send p6=3, verify 30m displays
   - Send p6=4, verify 20m displays
   - Send p6=5, verify 17-15m displays
   - Send p6=6, verify 12-10m displays
   - Send p6=7, verify 6m displays

2. **Test manual band button cycling:**
   - Verify band cycles correctly after telemetry update
   - Verify antenna selection matches band

3. **Test band command transmission:**
   - Verify sending "71" selects 160m on device
   - Verify sending "78" selects 6m on device

---

## Conclusion

The **off-by-one error** is definitively located in:
- **File**: `decompiled/HeliosDX/Main.cs`
- **Lines**: 1557-1591
- **Issue**: Switch statement uses cases 1-8 instead of 0-7 for 0-based p6 values

The **modern Electron frontend** is correctly implemented and requires no changes.

The root cause is a mismatch between the device protocol (0-based) and the legacy application's switch statement (1-based cases), resulting in all band displays being off by one and the first/last bands having no matching case.
