namespace TransitObd.Protocol;

/// <summary>
/// A decoded OBD-II PID measurement with optional engineering-unit interpretation.
/// Formulas are ported verbatim from the reference implementation's
/// <c>PidValue::interpret_value</c> in faraday-core/src/protocol/j1979.rs — standard SAE
/// J1979 formulas, identical across CAN-based OBD-II/EOBD vehicles.
/// </summary>
public sealed class PidValue
{
    public Pid Pid { get; }
    public byte[] RawValue { get; }
    public double? InterpretedValue { get; }
    public string? Unit { get; }

    public PidValue(Pid pid, byte[] rawValue)
    {
        Pid = pid;
        RawValue = rawValue;
        (InterpretedValue, Unit) = Interpret(pid, rawValue);
    }

    public static (double? Value, string? Unit) Interpret(Pid pid, IReadOnlyList<byte> rawValue)
    {
        switch (pid)
        {
            case Pid.EngineLoad:
            case Pid.FuelTankLevel:
                return rawValue.Count > 0
                    ? (rawValue[0] * 100.0 / 255.0, "%")
                    : (null, null);

            case Pid.CoolantTemp:
            case Pid.IntakeAirTemp:
            case Pid.AmbientAirTemp:
            case Pid.EngineOilTemp:
                return rawValue.Count > 0
                    ? (rawValue[0] - 40.0, "°C")
                    : (null, null);

            case Pid.ShortFuelTrimBank1:
            case Pid.LongFuelTrimBank1:
            case Pid.ShortFuelTrimBank2:
            case Pid.LongFuelTrimBank2:
            case Pid.EgrError:
                return rawValue.Count > 0
                    ? ((rawValue[0] - 128.0) * 100.0 / 128.0, "%")
                    : (null, null);

            case Pid.EngineRpm:
                return rawValue.Count >= 2
                    ? (((rawValue[0] << 8) | rawValue[1]) / 4.0, "rpm")
                    : (null, null);

            case Pid.VehicleSpeed:
                return rawValue.Count > 0
                    ? ((double)rawValue[0], "km/h")
                    : (null, null);

            case Pid.TimingAdvance:
                return rawValue.Count > 0
                    ? (rawValue[0] / 2.0 - 64.0, "°")
                    : (null, null);

            case Pid.ThrottlePosition:
            case Pid.RelativeThrottlePosition:
            case Pid.EgrCommanded:
                return rawValue.Count > 0
                    ? (rawValue[0] * 100.0 / 255.0, "%")
                    : (null, null);

            case Pid.OxygenSensorsPresent:
                return rawValue.Count > 0
                    ? ((double)rawValue[0], null)
                    : (null, null);

            case Pid.O2Bank1Sensor1Voltage:
            case Pid.O2Bank1Sensor2Voltage:
                return rawValue.Count >= 2
                    ? (rawValue[0] * 0.005, "V")
                    : (null, null);

            case Pid.MafRate:
                return rawValue.Count >= 2
                    ? (((rawValue[0] << 8) | rawValue[1]) / 100.0, "g/s")
                    : (null, null);

            case Pid.FuelAirEquivalenceRatio:
                return rawValue.Count >= 4
                    ? (((rawValue[0] << 8) | rawValue[1]) * 2.0 / 65536.0, "λ")
                    : (null, null);

            case Pid.ControlModuleVoltage:
                return rawValue.Count >= 2
                    ? (((rawValue[0] << 8) | rawValue[1]) / 1000.0, "V")
                    : (null, null);

            case Pid.RuntimeWithMilOn:
            case Pid.RuntimeSinceCodesCleared:
                return rawValue.Count >= 2
                    ? ((double)((rawValue[0] << 8) | rawValue[1]), "min")
                    : (null, null);

            case Pid.EngineFuelRate:
                return rawValue.Count >= 2
                    ? (((rawValue[0] << 8) | rawValue[1]) * 0.05, "L/h")
                    : (null, null);

            case Pid.DriverDemandEngineTorque:
            case Pid.ActualEngineTorque:
                return rawValue.Count > 0
                    ? (rawValue[0] - 125.0, "%")
                    : (null, null);

            case Pid.NoxReagentLevel:
                return rawValue.Count >= 10
                    ? (rawValue[5] * 100.0 / 255.0, "%")
                    : (null, null);

            case Pid.DieselExhaustFluidLevel:
                return rawValue.Count >= 4
                    ? (rawValue[3] * 100.0 / 255.0, "%")
                    : (null, null);

            case Pid.DieselExhaustFluidDosing:
                return rawValue.Count >= 4
                    ? (rawValue[1] / 2.0, "%")
                    : (null, null);

            case Pid.FuelPressure:
                return rawValue.Count > 0
                    ? (rawValue[0] * 3.0, "kPa")
                    : (null, null);

            case Pid.IntakeManifoldPressure:
            case Pid.BarometricPressure:
                return rawValue.Count > 0
                    ? ((double)rawValue[0], "kPa")
                    : (null, null);

            case Pid.FuelRailPressure:
                return rawValue.Count >= 2
                    ? (((rawValue[0] << 8) | rawValue[1]) * 0.079, "kPa")
                    : (null, null);

            case Pid.FuelRailGaugePressure:
            case Pid.FuelRailAbsolutePressure:
                return rawValue.Count >= 2
                    ? (((rawValue[0] << 8) | rawValue[1]) * 10.0, "kPa")
                    : (null, null);

            case Pid.CylinderFuelRate:
                return rawValue.Count >= 2
                    ? (((rawValue[0] << 8) | rawValue[1]) / 32.0, "mg/stroke")
                    : (null, null);

            case Pid.MaxMafRate:
                return rawValue.Count >= 4
                    ? (rawValue[0] * 10.0, "g/s")
                    : (null, null);

            case Pid.FuelInjectionTiming:
                return rawValue.Count >= 2
                    ? (((rawValue[0] << 8) | rawValue[1]) / 128.0 - 210.0, "°")
                    : (null, null);

            case Pid.CommandedEvapPurge:
                return rawValue.Count > 0
                    ? (rawValue[0] * 100.0 / 255.0, "%")
                    : (null, null);

            case Pid.EvapSystemVaporPressure:
                return rawValue.Count >= 2
                    ? ((short)((rawValue[0] << 8) | rawValue[1]) / 4.0, "Pa")
                    : (null, null);

            case Pid.AbsoluteEvapSystemVaporPressure:
                return rawValue.Count >= 2
                    ? (((rawValue[0] << 8) | rawValue[1]) / 200.0, "kPa")
                    : (null, null);

            case Pid.EvapSystemVaporPressureRaw:
                return rawValue.Count >= 2
                    ? ((double)(short)((rawValue[0] << 8) | rawValue[1]), "Pa")
                    : (null, null);

            default:
                return (null, null);
        }
    }
}
