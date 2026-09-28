# BusUitlezer

OBD-II / CAN diagnostics tool for a Ford Transit 2019, 2.0L EcoBlue. See `docs/planning/ROADMAP.md`
for the full plan.

## Usage

```
dotnet build BusUitlezer.sln
dotnet test BusUitlezer.sln

dotnet run -- demo               # emulator demo, no adapter needed
dotnet run -- <COM-poort> [baudrate]
```
