# M1 — Emulator + J1979 Protocol Layer

**Status:** current priority
**Depends on:** nothing (hardware-independent by design)
**Blocks:** M2 (hardware validation)

## Goal

Implement standard OBD-II (SAE J1979 / EOBD) read operations — live PID data, DTC read/clear,
VIN — plus a fake ECU link, so the entire read path can be built and unit-tested without an
OBD-II adapter or access to the vehicle. This is safe to build now because J1979 is a published,
vendor-neutral standard (SAE J1979 / ISO 15031) — its message formats and formulas are identical
across CAN-based OBD-II/EOBD vehicles, not specific to Ford or to this Transit.

## Reference implementation

`faraday-core/src/protocol/j1979.rs` in https://github.com/r1cm3d/faraday/tree/main is a working
Rust implementation of this exact protocol layer, already verified byte-for-byte against the
SAE J1979 spec. Use it as the source of truth for formulas and message formats when porting to
C#. Do not reference any other file in that repo for this milestone — `faraday-asbuilt`,
`protocol/uds.rs`, and `protocol/seed_key.rs` are Ford Fusion-specific and out of scope here.

## Stories

### 1. `Pid` catalog

Define the set of Mode 01 PIDs to support (at minimum: engine RPM `0x0C`, vehicle speed `0x0D`,
coolant temp `0x05`, intake air temp `0x0F`, throttle position `0x11`, fuel tank level `0x2F`,
control module voltage `0x42` — extend from the reference file's `Pid` impl as needed) and the
expected response data length per PID (1, 2, or 4 bytes — see the reference file's
`get_pid_data_length`).

**Acceptance:** a `Pid` enum/type exists with the chosen PIDs and a lookup for expected byte
length per PID.

### 2. `PidValue` — Mode 01 formula interpreter

Port the physical-unit interpretation formulas from the reference file's
`PidValue::interpret_value` (RPM, speed, temperatures, throttle %, fuel trim %, etc.) into a C#
equivalent that takes a `Pid` and raw response bytes and returns a scaled value + unit.

**Acceptance:** unit tests with hand-computed expected values for at least RPM, speed, and one
temperature PID (formulas are in the reference file; do not guess them, transcribe exactly).

### 3. `Dtc` — DTC decoder

Port the 2-byte → `P0301`-style code decoder from the reference file's `Dtc::from_bytes`
(category from top 2 bits, first digit from next 2 bits, second digit from low nibble, third/
fourth digits from the second byte).

**Acceptance:** unit tests against known DTC byte pairs, including at least one P-code, one
C-code (verify the category-prefix bit mapping is correct: 00=P, 01=C, 10=B, 11=U), and a
rejection test for a wrong-length input.

### 4. `IObdLink` abstraction

Define an interface that both a real serial-port adapter and a fake in-memory ECU can implement
— something like "send these request bytes, get these response bytes back" — so the protocol
layer above it never needs to know whether it's talking to real hardware or a test double.

**Acceptance:** interface compiles and is used by story 6 without any hardware-specific type
leaking through it.

### 5. `FakeObdLink` — emulator

Implement `IObdLink` with canned responses for: Mode 01 requests for the PIDs chosen in story 1,
Mode 03 (stored DTCs) returning at least two known codes, and Mode 09 PID 02 (VIN) returning a
plausible fake VIN. This is the emulator — it replaces the physical adapter + vehicle entirely
for development and testing.

**Acceptance:** every story-6 operation can be exercised end-to-end against this fake link with
no hardware attached.

### 6. `J1979Client` — high-level operations

Port the request/response orchestration from the reference file's `J1979` impl: build the Mode
01/03/04/07/09/0A request bytes, call `IObdLink`, parse the response using stories 2 and 3.
Operations: read live data for a set of PIDs, read stored/pending/permanent DTCs, clear DTCs,
read VIN.

**Acceptance:** integration-style tests exercise `J1979Client` against `FakeObdLink` (not real
hardware) and assert correctly parsed, human-readable results.

### 7. Wire it into the existing console app

Extend the existing `transit-obd` console app (the already-working `SerialPort` AT-command code)
with a `demo` mode that runs `J1979Client` against `FakeObdLink` and prints the results, and a
real mode that runs it against the serial adapter once hardware is available (M2).

**Acceptance:** `dotnet run -- demo` prints plausible RPM/speed/coolant values and at least one
decoded DTC, with no adapter connected.

## Definition of done for M1

- `dotnet test` passes (stories 2, 3, 6 covered by unit/integration tests).
- `dotnet run -- demo` runs end-to-end with no hardware attached.
- No Transit-specific, non-standard data was introduced anywhere in this milestone — everything
  is either a J1979 standard formula or an arbitrarily-chosen fake/test value clearly labeled
  as such.