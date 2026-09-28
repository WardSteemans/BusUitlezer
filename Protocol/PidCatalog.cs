namespace TransitObd.Protocol;

/// <summary>
/// Expected Mode 01 response data length per <see cref="Pid"/>, ported from the reference
/// implementation's <c>get_pid_data_length</c> in faraday-core/src/protocol/j1979.rs.
/// </summary>
public static class PidCatalog
{
    public static int GetExpectedByteLength(Pid pid) => pid switch
    {
        Pid.EngineLoad
            or Pid.CoolantTemp
            or Pid.VehicleSpeed
            or Pid.IntakeAirTemp
            or Pid.ThrottlePosition
            or Pid.FuelTankLevel
            or Pid.AmbientAirTemp
            or Pid.EngineOilTemp
            or Pid.ShortFuelTrimBank1
            or Pid.LongFuelTrimBank1
            or Pid.ShortFuelTrimBank2
            or Pid.LongFuelTrimBank2
            or Pid.TimingAdvance
            or Pid.OxygenSensorsPresent
            or Pid.EgrCommanded
            or Pid.EgrError
            or Pid.RelativeThrottlePosition
            or Pid.DriverDemandEngineTorque
            or Pid.ActualEngineTorque
            or Pid.FuelPressure
            or Pid.IntakeManifoldPressure
            or Pid.BarometricPressure
            or Pid.CommandedEvapPurge => 1,

        Pid.EngineRpm
            or Pid.MafRate
            or Pid.ControlModuleVoltage
            or Pid.O2Bank1Sensor1Voltage
            or Pid.O2Bank1Sensor2Voltage
            or Pid.RuntimeWithMilOn
            or Pid.RuntimeSinceCodesCleared
            or Pid.EngineFuelRate
            or Pid.FuelRailPressure
            or Pid.FuelRailGaugePressure
            or Pid.FuelRailAbsolutePressure
            or Pid.CylinderFuelRate
            or Pid.FuelInjectionTiming
            or Pid.EvapSystemVaporPressure
            or Pid.AbsoluteEvapSystemVaporPressure
            or Pid.EvapSystemVaporPressureRaw => 2,

        Pid.FuelAirEquivalenceRatio
            or Pid.MaxMafRate => 4,

        Pid.NoxReagentLevel => 10,

        Pid.DieselExhaustFluidLevel
            or Pid.DieselExhaustFluidDosing => 4,

        _ => 1,
    };
}
