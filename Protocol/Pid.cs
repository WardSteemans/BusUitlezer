namespace TransitObd.Protocol;

/// <summary>
/// SAE J1979 Mode 01 Parameter IDs. Values and byte lengths are the published OBD-II
/// standard (SAE J1979 / ISO 15031-5), ported from the reference implementation's
/// <c>Pid</c> impl in faraday-core/src/protocol/j1979.rs (r1cm3d/faraday) — vendor-neutral
/// and valid for any CAN-based OBD-II/EOBD vehicle, not Transit-specific.
/// </summary>
public enum Pid : byte
{
    /// <summary>Calculated engine load (0-100 %).</summary>
    EngineLoad = 0x04,
    /// <summary>Engine coolant temperature (-40 to +215 degC).</summary>
    CoolantTemp = 0x05,
    /// <summary>Short-term fuel trim, bank 1 (-100 to +99.2 %).</summary>
    ShortFuelTrimBank1 = 0x06,
    /// <summary>Long-term fuel trim, bank 1 (-100 to +99.2 %).</summary>
    LongFuelTrimBank1 = 0x07,
    /// <summary>Short-term fuel trim, bank 2 (-100 to +99.2 %).</summary>
    ShortFuelTrimBank2 = 0x08,
    /// <summary>Long-term fuel trim, bank 2 (-100 to +99.2 %).</summary>
    LongFuelTrimBank2 = 0x09,
    /// <summary>Engine speed (0-16383.75 RPM).</summary>
    EngineRpm = 0x0C,
    /// <summary>Vehicle speed (0-255 km/h).</summary>
    VehicleSpeed = 0x0D,
    /// <summary>Ignition timing advance relative to TDC (-64 to +63.5 deg).</summary>
    TimingAdvance = 0x0E,
    /// <summary>Intake air temperature (-40 to +215 degC).</summary>
    IntakeAirTemp = 0x0F,
    /// <summary>Mass air-flow rate (0-655.35 g/s).</summary>
    MafRate = 0x10,
    /// <summary>Absolute throttle position (0-100 %).</summary>
    ThrottlePosition = 0x11,
    /// <summary>Oxygen sensors present (bitmask).</summary>
    OxygenSensorsPresent = 0x13,
    /// <summary>O2 sensor voltage - bank 1, sensor 1 (0-1.275 V).</summary>
    O2Bank1Sensor1Voltage = 0x14,
    /// <summary>O2 sensor voltage - bank 1, sensor 2 (0-1.275 V).</summary>
    O2Bank1Sensor2Voltage = 0x15,
    /// <summary>Commanded EGR valve position (0-100 %).</summary>
    EgrCommanded = 0x2C,
    /// <summary>EGR error (-100 to +99.2 %).</summary>
    EgrError = 0x2D,
    /// <summary>Fuel tank level input (0-100 %).</summary>
    FuelTankLevel = 0x2F,
    /// <summary>Control module supply voltage (0-65.535 V).</summary>
    ControlModuleVoltage = 0x42,
    /// <summary>Commanded equivalence ratio (0-2).</summary>
    FuelAirEquivalenceRatio = 0x44,
    /// <summary>Ambient air temperature (-40 to +215 degC).</summary>
    AmbientAirTemp = 0x46,
    /// <summary>Time run with MIL on (minutes).</summary>
    RuntimeWithMilOn = 0x4D,
    /// <summary>Time since diagnostic trouble codes were cleared (minutes).</summary>
    RuntimeSinceCodesCleared = 0x4E,
    /// <summary>Relative throttle position (0-100 %).</summary>
    RelativeThrottlePosition = 0x5A,
    /// <summary>Engine oil temperature (-40 to +215 degC).</summary>
    EngineOilTemp = 0x5C,
    /// <summary>Engine fuel rate (0-3276.75 L/h).</summary>
    EngineFuelRate = 0x5E,
    /// <summary>Driver's demand engine torque (-125 to +130 %).</summary>
    DriverDemandEngineTorque = 0x61,
    /// <summary>Actual engine torque (-125 to +130 %).</summary>
    ActualEngineTorque = 0x62,
    /// <summary>NOx reagent/SCR system — reagent level (0-100 %), decoded from byte offset 5
    /// of a 10-byte response; the other 9 bytes are undocumented.</summary>
    NoxReagentLevel = 0x85,
    /// <summary>Diesel exhaust fluid (DEF) sensor — level (0-100 %), decoded from byte offset 3
    /// of a 4-byte response; the other 3 bytes are undocumented.</summary>
    DieselExhaustFluidLevel = 0x9B,
    /// <summary>Commanded diesel exhaust fluid dosing rate (0-127.5 %), decoded from byte
    /// offset 1 of a 4-byte response; the other 3 bytes are undocumented.</summary>
    DieselExhaustFluidDosing = 0xA5,
}
