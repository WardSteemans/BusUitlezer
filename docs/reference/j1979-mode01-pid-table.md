# SAE J1979 Mode 01 PID Reference

Local copy of the Mode 01 ("show current data") PID table so nobody has to re-fetch and
re-verify it from Wikipedia each time coverage is extended. Covers the full hex range
`0x00`-`0xC8`.

## Source & methodology

Source: `https://en.wikipedia.org/wiki/OBD-II_PIDs`, Service 01 PID table, read from the
page's **raw wikitext** (not the rendered/summarized page) so formulas here are verbatim, not
paraphrased.

Byte-letter convention, per the source table's own footnote: "letters A, B, C, etc. represent
the first, second, third, etc. byte of the data" — i.e. `A` = first response byte
(`rawValue[0]` in this codebase), `B` = second, and so on.

Every formula below was cross-checked for internal consistency against its own stated
Min/Max (e.g. a 2-byte unsigned formula's value at raw `0xFFFF` should equal the stated max) —
this catches source transcription errors before they become bugs. Two inconsistencies were
found this way; both are called out where relevant instead of being silently "corrected."

This file is a reference snapshot, not a live mirror — if Wikipedia's table changes, this file
doesn't automatically follow. Re-verify against the source before trusting a formula here for
anything safety-relevant.

## 1. Implemented (`Protocol/Pid.cs` / `PidCatalog.cs` / `PidValue.cs`)

28 PIDs, ported in M1 (issues #1-7) plus the cherry-picked hardening pass.

| Hex | Dec | Bytes | Enum name | Description | Formula | Unit | Min | Max |
|---|---|---|---|---|---|---|---|---|
| `0x04` | 4 | 1 | `EngineLoad` | Calculated engine load | `100/255 × A` | % | 0 | 100 |
| `0x05` | 5 | 1 | `CoolantTemp` | Engine coolant temperature | `A - 40` | °C | -40 | 215 |
| `0x06` | 6 | 1 | `ShortFuelTrimBank1` | Short term fuel trim, bank 1 | `100/128 × A - 100` | % | -100 | 99.2 |
| `0x07` | 7 | 1 | `LongFuelTrimBank1` | Long term fuel trim, bank 1 | `100/128 × A - 100` | % | -100 | 99.2 |
| `0x08` | 8 | 1 | `ShortFuelTrimBank2` | Short term fuel trim, bank 2 | `100/128 × A - 100` | % | -100 | 99.2 |
| `0x09` | 9 | 1 | `LongFuelTrimBank2` | Long term fuel trim, bank 2 | `100/128 × A - 100` | % | -100 | 99.2 |
| `0x0C` | 12 | 2 | `EngineRpm` | Engine speed | `(256A+B)/4` | rpm | 0 | 16,383.75 |
| `0x0D` | 13 | 1 | `VehicleSpeed` | Vehicle speed | `A` | km/h | 0 | 255 |
| `0x0E` | 14 | 1 | `TimingAdvance` | Timing advance | `A/2 - 64` | ° before TDC | -64 | 63.5 |
| `0x0F` | 15 | 1 | `IntakeAirTemp` | Intake air temperature | `A - 40` | °C | -40 | 215 |
| `0x10` | 16 | 2 | `MafRate` | MAF air flow rate | `(256A+B)/100` | g/s | 0 | 655.35 |
| `0x11` | 17 | 1 | `ThrottlePosition` | Throttle position | `100/255 × A` | % | 0 | 100 |
| `0x13` | 19 | 1 | `OxygenSensorsPresent` | O2 sensors present (2 banks) | raw bitmask (no formula in source; `[A0..A3]`=bank1 sensors 1-4, `[A4..A7]`=bank2) | — | — | — |
| `0x14` | 20 | 2 | `O2Bank1Sensor1Voltage` | O2 sensor 1 voltage (byte B, fuel trim, not decoded) | `A/200` | V | 0 | 1.275 |
| `0x15` | 21 | 2 | `O2Bank1Sensor2Voltage` | O2 sensor 2 voltage (byte B, fuel trim, not decoded) | `A/200` | V | 0 | 1.275 |
| `0x2C` | 44 | 1 | `EgrCommanded` | Commanded EGR | `100/255 × A` | % | 0 | 100 |
| `0x2D` | 45 | 1 | `EgrError` | EGR error | `100/128 × A - 100` | % | -100 | 99.2 |
| `0x2F` | 47 | 1 | `FuelTankLevel` | Fuel tank level input | `100/255 × A` | % | 0 | 100 |
| `0x42` | 66 | 2 | `ControlModuleVoltage` | Control module voltage | `(256A+B)/1000` | V | 0 | 65.535 |
| `0x44` | 68 | 4 (only A,B decoded) | `FuelAirEquivalenceRatio` | Commanded equivalence ratio (λ) | `2/65536 × (256A+B)` | ratio | 0 | <2 |
| `0x46` | 70 | 1 | `AmbientAirTemp` | Ambient air temperature | `A - 40` | °C | -40 | 215 |
| `0x4D` | 77 | 2 | `RuntimeWithMilOn` | Time run with MIL on | `256A+B` | min | 0 | 65,535 |
| `0x4E` | 78 | 2 | `RuntimeSinceCodesCleared` | Time since trouble codes cleared | `256A+B` | min | 0 | 65,535 |
| `0x5A` | 90 | 1 | `RelativeThrottlePosition` | ⚠️ **misnamed** — this hex is actually "Relative accelerator pedal position"; formula/behavior is correct, only the name is wrong. See §3. | `100/255 × A` | % | 0 | 100 |
| `0x5C` | 92 | 1 | `EngineOilTemp` | Engine oil temperature | `A - 40` | °C | -40 | 210 (source; this codebase's doc comment currently says 215 — see §3) |
| `0x5E` | 94 | 2 | `EngineFuelRate` | Engine fuel rate | `(256A+B)/20` | L/h | 0 | 3276.75 (source table itself says 3212.75 here, which is arithmetically inconsistent with its own formula — `65535/20 = 3276.75`; this codebase already uses the correct value) |
| `0x61` | 97 | 1 | `DriverDemandEngineTorque` | Driver's demand engine - percent torque | `A - 125` | % | -125 | 130 |
| `0x62` | 98 | 1 | `ActualEngineTorque` | Actual engine - percent torque | `A - 125` | % | -125 | 130 |

## 2. Verified, tracked for implementation (GitHub issues, milestone #6 "M1.5")

58 PIDs total, formulas verified the same way as above, not yet in `Pid.cs`. Full formulas,
byte offsets, fake test fixtures, and file-by-file implementation instructions are in each
issue — this table is an index, not a substitute for reading the issue before implementing.

### Issue #10 — NOx/SCR reagent (DEF) level & dosing
| Hex | Bytes | Description | Formula | Unit |
|---|---|---|---|---|
| `0x85` | 10 (only byte F decoded) | NOx reagent system | `100/255 × F` | % |
| `0x9B` | 4 (only byte D decoded) | Diesel Exhaust Fluid Sensor Data | `100/255 × D` | % |
| `0xA5` | 4 (only byte B decoded) | Commanded Diesel Exhaust Fluid Dosing | `B/2` | % |

### Issue #11 — Fuel & intake pressure/flow
| Hex | Bytes | Description | Formula | Unit |
|---|---|---|---|---|
| `0x0A` | 1 | Fuel pressure (gauge) | `3A` | kPa |
| `0x0B` | 1 | Intake manifold absolute pressure | `A` | kPa |
| `0x22` | 2 | Fuel Rail Pressure (rel. to manifold vacuum) | `0.079(256A+B)` | kPa |
| `0x23` | 2 | Fuel Rail Gauge Pressure (diesel/GDI) | `10(256A+B)` | kPa |
| `0x33` | 1 | Absolute Barometric Pressure | `A` | kPa |
| `0x50` | 4 (only byte A; B-D reserved per source) | Max MAF sensor air flow rate | `A × 10` | g/s |
| `0x59` | 2 | Fuel rail absolute pressure | `10(256A+B)` | kPa |
| `0x5D` | 2 | Fuel injection timing | `(256A+B)/128 - 210` | ° |
| `0xA2` | 2 | Cylinder Fuel Rate | `(256A+B)/32` | mg/stroke |

### Issue #12 — EVAP system vapor pressure (needs signed-int handling for two of these)
| Hex | Bytes | Description | Formula | Unit | Signed? |
|---|---|---|---|---|---|
| `0x2E` | 1 | Commanded evaporative purge | `100/255 × A` | % | no |
| `0x32` | 2 | Evap. System Vapor Pressure | `(256A+B)/4` | Pa | **yes**, two's complement |
| `0x53` | 2 | Absolute Evap system Vapor Pressure | `(256A+B)/200` | kPa | no |
| `0x54` | 2 | Evap system vapor pressure (distinct PID from `0x32`) | `256A+B` | Pa | **yes**, two's complement |

### Issue #13 — Extended oxygen sensor PIDs (22 PIDs)
Same "keep one documented value, drop the other" convention as the existing `0x14`/`0x15`.
- **Voltage** (drop fuel-trim byte, same formula as `0x14`/`0x15`): `0x16`-`0x1B` = O2 sensors 3-8, `A/200` V.
- **Equivalence ratio / lambda** (drop voltage bytes CD): `0x24`-`0x2B` = O2 sensors 1-8, `2/65536 × (256A+B)` ratio.
- **Current** (drop lambda bytes AB, decode CD instead): `0x34`-`0x3B` = O2 sensors 1-8, `(256C+D)/256 - 128` mA.
- **Secondary trims** (keep bank A, drop bank B): `0x55` short bank1, `0x56` long bank1, `0x57` short bank2, `0x58` long bank2 — all `100/128 × A - 100` %.
- `0x1D` — O2 sensors present, 4 banks — raw bitmask passthrough, same convention as `0x13`.

### Issue #14 — Catalyst temperature banks
| Hex | Bytes | Description | Formula | Unit |
|---|---|---|---|---|
| `0x3C` | 2 | Catalyst Temp: Bank 1, Sensor 1 | `(256A+B)/10 - 40` | °C |
| `0x3D` | 2 | Catalyst Temp: Bank 2, Sensor 1 | `(256A+B)/10 - 40` | °C |
| `0x3E` | 2 | Catalyst Temp: Bank 1, Sensor 2 | `(256A+B)/10 - 40` | °C |
| `0x3F` | 2 | Catalyst Temp: Bank 2, Sensor 2 | `(256A+B)/10 - 40` | °C |

### Issue #15 — Throttle/pedal position & torque
| Hex | Bytes | Description | Formula | Unit |
|---|---|---|---|---|
| `0x45` | 1 | Relative throttle position (the genuine one — see §3) | `100/255 × A` | % |
| `0x47` | 1 | Absolute throttle position B | `100/255 × A` | % |
| `0x48` | 1 | Absolute throttle position C | `100/255 × A` | % |
| `0x49` | 1 | Accelerator pedal position D | `100/255 × A` | % |
| `0x4A` | 1 | Accelerator pedal position E | `100/255 × A` | % |
| `0x4B` | 1 | Accelerator pedal position F | `100/255 × A` | % |
| `0x4C` | 1 | Commanded throttle actuator | `100/255 × A` | % |
| `0x5B` | 1 | Hybrid battery pack remaining life (N/A on this non-hybrid diesel) | `100/255 × A` | % |
| `0x63` | 2 | Engine reference torque | `256A+B` | N·m |
| `0x8E` | 1 | Engine Friction - Percent Torque | `A - 125` | % |

### Issue #16 — Distance, runtime & misc counters
| Hex | Bytes | Description | Formula | Unit |
|---|---|---|---|---|
| `0x1E` | 1 | Auxiliary input status (raw bitmask, `A0`=PTO active) | raw passthrough | — |
| `0x1F` | 2 | Run time since engine start | `256A+B` | s |
| `0x21` | 2 | Distance traveled with MIL on | `256A+B` | km |
| `0x30` | 1 | Warm-ups since codes cleared | `A` | — |
| `0x31` | 2 | Distance traveled since codes cleared | `256A+B` | km |
| `0xA4` | 4 (only bytes C,D decoded) | Transmission Actual Gear | `(256C+D)/1000` | ratio |
| `0xA6` | 4 | Odometer (CARB-mandated MY2019+, applies to this 2019 Transit) | `(A·2²⁴+B·2¹⁶+C·2⁸+D)/10` | km |

## 3. Known documentation issues in already-shipped code

- **`Pid.RelativeThrottlePosition = 0x5A` is misnamed** (tracked in issue #17). Per the source,
  `0x5A` is "Relative accelerator pedal position"; the genuine "Relative throttle position" is
  `0x45` (added in issue #15 as `ThrottlePositionRelative` to avoid the name collision). Formula
  and wire behavior for `0x5A` are correct — only the enum member's name is wrong. Not fixed
  automatically; renaming a shipped public enum member is a deliberate, separate decision.
- **`Pid.EngineOilTemp`'s doc comment overstates its max** as 215°C; the source gives 210°C for
  this specific PID (every other `A - 40` temperature PID in this codebase is 215°C, which is
  likely where the comment got copied from). The formula itself is correct — only the
  documented upper bound is off by 5°C. Not filed as a separate issue; noted here since it's
  purely a comment fix, trivial to correct in passing next time that file is touched.

## 4. Checked and not implementable (no GitHub issue filed)

Every PID below was looked at and rejected for a specific, stated reason — none of these are
"not checked yet."

**Discovery bitmaps** (protocol infrastructure, not measurements): `0x00`, `0x20`, `0x40`,
`0x60`, `0x80`, `0xA0`, `0xC0` — "PIDs supported [$xx-$xx]".

**Enumerated / bit-encoded status** (needs an enum or struct type, not a `double` — different
design than `PidValue`): `0x01`, `0x02` (a `Dtc.cs` concern, not `PidValue`), `0x03`, `0x12`,
`0x1C`, `0x41`, `0x51`, `0x5F`, `0x7D`, `0x7E`, `0xA9`, `0xC8`.

**Multi-value / bitfield, doesn't fit one-PID-one-`double`**: `0x64` (5 named sub-values),
`0x66` (MAF dual-sensor), `0x67`/`0x68` (dual-sensor temp with a support-bit branch),
`0x78`/`0x79` (EGT banks — bit-encoded support byte + 4 independent 2-byte readings, fully
documented but structurally multi-part), `0x4F` (4 independent max-value fields, different
units each).

**No formula published in the source** (the largest bucket — checked, not guessed):
`0x65`, `0x69`-`0x6F`, `0x71`-`0x77`, `0x7A`, `0x7B`, `0x7C` (formula given only explains 2 of
9 declared bytes), `0x7F` (formula only explains 4 of 13 declared bytes), `0x81`, `0x82`,
`0x83`, `0x84` (no formula *or* unit), `0x86`, `0x87` (simple version already covered as
`0x0B`), `0x88`, `0x89`, `0x8A`, `0x8B`, `0x8C`, `0x8D` (unit/range given, formula blank),
`0x8F`, `0x90`, `0x91`, `0x92`, `0x93`, `0x94`, `0x98`, `0x99` (a different, undocumented PID
from the fully-documented `0x78`/`0x79`), `0x9A` (also N/A, hybrid-only), `0x9C`, `0x9D` (a
different PID from the already-implemented `0x5E`), `0x9E`, `0x9F`, `0xA1`, `0xA3`, `0xA7`,
`0xA8`, `0xC3` (source literally says "returns numerous data"), `0xC4`, `0xC5` (blank despite a
clear name/range), `0xC6`, `0xC7` (blank; very likely `256A+B` like every other distance PID,
but not stated, so not assumed).

**Internally inconsistent source data** (not guessed at): `0x70` "Boost pressure control" —
formula text reads `(256D+E) / 0.03125`, but the row's own stated max (`2047.96875`) is only
reachable if the real operation is `× 0.03125` (i.e. `÷ 32`), a factor of 1024 off from the
literal text. Very likely a `32`/`0.03125` transposition on Wikipedia's part, but unconfirmed
by any independent source (faraday / python-OBD / freediag don't cover this PID either) — so
left out rather than "corrected" on an inference.

**Not applicable to this vehicle, but standard and included where documented anyway**: EVAP
system PIDs (gasoline vapor-recovery, issue #12), hybrid battery (`0x5B`), wideband/multi-bank
O2 sensor arrays (issue #13) — a diesel EcoBlue is unlikely to support most of these, they're
included per J1979 standard-coverage goals, not because they're expected to return meaningful
data on this Transit.
