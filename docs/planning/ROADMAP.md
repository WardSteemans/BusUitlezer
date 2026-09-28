# BusUitlezer — Roadmap

OBD-II / CAN diagnostics tool for a **Ford Transit 2019, 2.0L EcoBlue** (European market),
built in C#/.NET.

## Reference project

[faraday](https://github.com/r1cm3d/faraday/tree/main) — a Rust CLI for the same problem
domain (ELM327/STN adapter, ISO-TP, J1979, UDS, as-built configuration), built for a
**Ford Fusion 2017 SEL**. It is our architectural and protocol reference, not something we
port wholesale. See `reference/architecture-decisions.md` for exactly what is and isn't
safe to reuse from it, and why.

Short version: J1979 (standard OBD-II) and ISO-TP are vendor-neutral, published standards —
that logic in `faraday-core/src/protocol/j1979.rs` and `transport/isotp.rs` applies unchanged
to any CAN-based OBD-II vehicle, including this Transit. UDS DIDs and as-built block layouts
in `faraday-asbuilt` and `protocol/uds.rs`/`seed_key.rs` are Fusion-specific, unvalidated,
reverse-engineered data — do not reuse those values for the Transit without independent
validation; a wrong write can brick a module.

## Milestones

### M1 — Emulator + J1979 protocol layer (hardware-independent)

**Status: current priority.**

Build the standard OBD-II (SAE J1979 / EOBD) read layer — DTC decoding, live PID parsing,
VIN read — plus a fake ECU link so the whole thing is testable without an adapter or the
vehicle. See `milestones/M1-emulator-and-j1979-layer.md`.

Why first: this is the only layer we can build with full confidence right now, since J1979 is
a standard and needs no Transit-specific data. It also gives M2 something solid to plug real
hardware into, instead of debugging protocol logic and hardware quirks at the same time.

### M2 — Hardware validation

Once the OBD-II adapter and the van are both available: confirm the real adapter's AT-command
behavior, baud rate, and timing against what M1 assumed; fix whatever doesn't match. Confirm
J1979 PID/DTC reads work end-to-end against the real ECU.

### M3 — Transit-specific data research

Gather real, validated data for anything beyond standard J1979: CAN IDs for modules other than
the engine ECU (BCM equivalent, IPC, etc.), and as-built block layouts — from community sources
(FORScan/forum documentation for this Transit generation) or from the user's own bus captures.
No fabricated data. This milestone produces the inputs M4 needs; it does not touch the vehicle.

### M4 — UDS + module-specific reads

Only once M3 has real validated data: extend the protocol layer with UDS (ISO 14229) reads for
non-engine modules, following faraday's `protocol::uds` as a reference implementation (this part
IS safely reusable — UDS service framing is a standard, only the DIDs/addresses are vehicle
data).

### M5 — As-built writes (later, high-risk)

Configuration writes via UDS Security Access + WriteDataByIdentifier. Mandatory snapshot before
any write, dry-run by default — same safety model as faraday's ADR-003
(`docs/adr/ADR-003-snapshot-before-write.md` in the reference repo). Not started before M4 is
solid and validated on real hardware.