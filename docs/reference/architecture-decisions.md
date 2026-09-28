# Architecture Decisions

## Why C#/.NET, not Rust (like the reference project)

The reference project, faraday (https://github.com/r1cm3d/faraday/tree/main), is written in
Rust. We are not following that choice. The user has no prior Rust experience and some C#
familiarity; Rust's ownership/async-trait model would add a steep learning curve on top of an
already-unfamiliar protocol domain. C#'s `System.IO.Ports` gives equivalent serial-port access
with far less ceremony, and its class/interface model maps cleanly onto faraday's layered
architecture (`LinkLayer` trait -> `IObdLink` interface, `CommandExecutor` -> `J1979Client`,
etc.). What we take from faraday is the architecture and protocol knowledge, not the code or
the language.

## Why J1979 first, and why it's safe to build without the vehicle

SAE J1979 (the standard OBD-II diagnostic mode set — live data, DTCs, VIN) and ISO-TP (the CAN
transport framing) are published, vendor-neutral standards. Any CAN-based OBD-II/EOBD-compliant
vehicle — including this 2019 Transit — implements them identically at the protocol level:
same request/response byte formats, same PID formulas, same DTC encoding. This is why M1 can be
built and fully unit-tested with a fake ECU link, with justified confidence it will work
unchanged against the real vehicle (module-specific quirks aside — see M2).

## What is explicitly out of scope, and why

- **UDS module-specific DIDs and as-built block layouts** (BCM/IPC-equivalent configuration,
  anything beyond standard J1979) — these are vehicle- and firmware-specific, reverse-engineered
  data in the reference project, explicitly marked there as unvalidated even for the Fusion it
  targets. We have zero validated data for this Transit. Fabricating plausible-looking values
  here is actively dangerous: a wrong UDS write can leave a module unusable ("bricked"). This
  work is deferred to M3 (research, no vehicle contact) and M4 (implementation, only once M3
  has real validated data).
- **Configuration writes (Security Access, WriteDataByIdentifier)** — deferred to M5, and only
  ever with the same mandatory-snapshot-before-write safety model the reference project uses
  (see its ADR-003), once M4 is solid.

## Reuse guidance for the reference repository

| Reference file | Reuse how |
|---|---|
| `faraday-core/src/protocol/j1979.rs` | Port formulas/algorithms directly (M1) — standard, safe |
| `faraday-core/src/transport/isotp.rs` | Reference only if/when we need to hand-roll ISO-TP; most ELM327/STN adapters handle multi-frame reassembly transparently, so this may not be needed at all for M1/M2 |
| `faraday-core/src/protocol/uds.rs` | Reference for UDS *service framing* only (M4) — the framing is standard, the DIDs used alongside it in that file are not |
| `faraday-asbuilt/*` | Do not reuse any data. Fusion-specific, unvalidated even for its own target vehicle |
| `faraday-core/src/protocol/seed_key.rs` | Do not reuse the XOR mask — it's Fusion-specific and explicitly unvalidated in the source project itself |
| `docs/adr/ADR-003-snapshot-before-write.md` | Reuse the *safety pattern* (M5), not any code |