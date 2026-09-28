namespace TransitObd.Protocol;

/// <summary>
/// In-memory <see cref="IObdLink"/> emulator. Replaces the physical adapter + vehicle for
/// development and testing (M1). All response values below are arbitrary, hand-picked test
/// fixtures — not measurements from a real vehicle, and not Transit-specific data.
/// </summary>
public sealed class FakeObdLink : IObdLink
{
    private static readonly IReadOnlyDictionary<Pid, byte[]> RawValues = new Dictionary<Pid, byte[]>
    {
        [Pid.EngineLoad] = [0x59],                       // ~34.9 %
        [Pid.CoolantTemp] = [0x81],                      // 89 degC
        [Pid.ShortFuelTrimBank1] = [0x83],                // ~2.34 %
        [Pid.LongFuelTrimBank1] = [0x7F],                 // ~-0.78 %
        [Pid.ShortFuelTrimBank2] = [0x82],                // ~1.56 %
        [Pid.LongFuelTrimBank2] = [0x7D],                 // ~-2.34 %
        [Pid.EngineRpm] = [0x21, 0x98],                  // 2150 rpm
        [Pid.VehicleSpeed] = [0x3E],                      // 62 km/h
        [Pid.TimingAdvance] = [0x94],                     // 10 deg
        [Pid.IntakeAirTemp] = [0x40],                     // 24 degC
        [Pid.MafRate] = [0x04, 0xE2],                    // 12.5 g/s
        [Pid.ThrottlePosition] = [0x2E],                  // ~18 %
        [Pid.OxygenSensorsPresent] = [0x03],              // sensors 1 & 2 present (bitmask)
        [Pid.O2Bank1Sensor1Voltage] = [0x5A, 0x80],      // 0.45 V
        [Pid.O2Bank1Sensor2Voltage] = [0x7C, 0x80],      // 0.62 V
        [Pid.EgrCommanded] = [0x33],                      // 20.0 %
        [Pid.EgrError] = [0x7C],                          // ~-3.125 %
        [Pid.FuelTankLevel] = [0x8C],                     // ~54.9 %
        [Pid.ControlModuleVoltage] = [0x35, 0xE8],       // 13.800 V
        [Pid.FuelAirEquivalenceRatio] = [0x80, 0x00, 0x00, 0x00], // lambda 1.0
        [Pid.AmbientAirTemp] = [0x3A],                    // 18 degC
        [Pid.RuntimeWithMilOn] = [0x00, 0x0C],           // 12 min
        [Pid.RuntimeSinceCodesCleared] = [0x01, 0x54],   // 340 min
        [Pid.RelativeThrottlePosition] = [0x0D],          // ~5.1 %
        [Pid.EngineOilTemp] = [0x87],                     // 95 degC
        [Pid.EngineFuelRate] = [0x00, 0x40],             // 3.2 L/h
        [Pid.DriverDemandEngineTorque] = [0x73],          // -10 %
        [Pid.ActualEngineTorque] = [0x75],                // -8 %
        [Pid.NoxReagentLevel] = [0x00, 0x00, 0x00, 0x00, 0x00, 0xCC, 0x00, 0x00, 0x00, 0x00], // 80.0 %
        [Pid.DieselExhaustFluidLevel] = [0x00, 0x00, 0x00, 0x99], // 60.0 %
        [Pid.DieselExhaustFluidDosing] = [0x01, 0x28, 0x00, 0x00], // 20.0 %
        [Pid.FuelPressure] = [0x64],                      // 300 kPa
        [Pid.IntakeManifoldPressure] = [0x69],             // 105 kPa
        [Pid.FuelRailPressure] = [0x27, 0x10],            // 790.0 kPa
        [Pid.FuelRailGaugePressure] = [0x61, 0xA8],       // 250000 kPa
        [Pid.FuelRailAbsolutePressure] = [0x4E, 0x20],    // 200000 kPa
        [Pid.CylinderFuelRate] = [0x06, 0x40],            // 50.0 mg/stroke
        [Pid.MaxMafRate] = [0x50, 0x00, 0x00, 0x00],      // 800.0 g/s
        [Pid.FuelInjectionTiming] = [0x69, 0x00],         // 0.0 deg
        [Pid.BarometricPressure] = [0x65],                 // 101 kPa
        [Pid.CommandedEvapPurge] = [0x33],                 // 20.0 %
        [Pid.EvapSystemVaporPressure] = [0xFE, 0x70],     // -100.0 Pa
        [Pid.AbsoluteEvapSystemVaporPressure] = [0x4E, 0x20], // 100.0 kPa
        [Pid.EvapSystemVaporPressureRaw] = [0xFE, 0x0C],  // -500.0 Pa
        [Pid.O2Sensor3Voltage] = [0x5A, 0x00],            // 0.450 V
        [Pid.O2Sensor4Voltage] = [0x64, 0x00],            // 0.500 V
        [Pid.O2Sensor5Voltage] = [0x6E, 0x00],            // 0.550 V
        [Pid.O2Sensor6Voltage] = [0x78, 0x00],            // 0.600 V
        [Pid.O2Sensor7Voltage] = [0x82, 0x00],            // 0.650 V
        [Pid.O2Sensor8Voltage] = [0x8C, 0x00],            // 0.700 V
        [Pid.O2Sensor1EquivalenceRatio] = [0x80, 0x00, 0x00, 0x00], // lambda 1.0
        [Pid.O2Sensor2EquivalenceRatio] = [0x80, 0x00, 0x00, 0x00], // lambda 1.0
        [Pid.O2Sensor3EquivalenceRatio] = [0x80, 0x00, 0x00, 0x00], // lambda 1.0
        [Pid.O2Sensor4EquivalenceRatio] = [0x80, 0x00, 0x00, 0x00], // lambda 1.0
        [Pid.O2Sensor5EquivalenceRatio] = [0x80, 0x00, 0x00, 0x00], // lambda 1.0
        [Pid.O2Sensor6EquivalenceRatio] = [0x80, 0x00, 0x00, 0x00], // lambda 1.0
        [Pid.O2Sensor7EquivalenceRatio] = [0x80, 0x00, 0x00, 0x00], // lambda 1.0
        [Pid.O2Sensor8EquivalenceRatio] = [0x80, 0x00, 0x00, 0x00], // lambda 1.0
        [Pid.O2Sensor1Current] = [0x80, 0x00, 0x80, 0x00], // 0.000 mA
        [Pid.O2Sensor2Current] = [0x80, 0x00, 0x80, 0x00], // 0.000 mA
        [Pid.O2Sensor3Current] = [0x80, 0x00, 0x80, 0x00], // 0.000 mA
        [Pid.O2Sensor4Current] = [0x80, 0x00, 0x80, 0x00], // 0.000 mA
        [Pid.O2Sensor5Current] = [0x80, 0x00, 0x80, 0x00], // 0.000 mA
        [Pid.O2Sensor6Current] = [0x80, 0x00, 0x80, 0x00], // 0.000 mA
        [Pid.O2Sensor7Current] = [0x80, 0x00, 0x80, 0x00], // 0.000 mA
        [Pid.O2Sensor8Current] = [0x80, 0x00, 0x80, 0x00], // 0.000 mA
        [Pid.SecondaryO2TrimShortBank1] = [0x60, 0x00],   // -25.0 %
        [Pid.SecondaryO2TrimLongBank1] = [0x60, 0x00],    // -25.0 %
        [Pid.SecondaryO2TrimShortBank2] = [0x60, 0x00],   // -25.0 %
        [Pid.SecondaryO2TrimLongBank2] = [0x60, 0x00],    // -25.0 %
        [Pid.OxygenSensorsPresent4Banks] = [0x0F],         // 15.0 (raw bitmask)
    };

    /// <summary>True if <paramref name="pid"/> has an explicit canned value (as opposed to
    /// falling back to a zero-filled placeholder). Used to assert full PID coverage in tests.</summary>
    internal static bool HasCannedValue(Pid pid) => RawValues.ContainsKey(pid);

    // Two well-known, publicly documented generic OBD-II codes used purely as test
    // fixtures: P0301 (cylinder 1 misfire detected), P0171 (system too lean, bank 1).
    private static readonly byte[][] StoredDtcBytes = [[0x03, 0x01], [0x01, 0x71]];

    private const string FakeVin = "FAKEVINTESTM10001";

    public Task<byte[]> SendRequestAsync(byte[] request, CancellationToken cancellationToken = default)
    {
        if (request.Length == 0)
        {
            throw new ArgumentException("Request must not be empty", nameof(request));
        }

        byte[] response = request[0] switch
        {
            0x01 => BuildMode01Response(request),
            0x03 => BuildDtcResponse(0x43, StoredDtcBytes),
            0x04 => [0x44],
            0x07 => BuildDtcResponse(0x47, []),
            0x09 => BuildMode09Response(request),
            0x0A => BuildDtcResponse(0x4A, []),
            _ => throw new NotSupportedException($"FakeObdLink does not support mode 0x{request[0]:X2}"),
        };

        return Task.FromResult(response);
    }

    private static byte[] BuildMode01Response(byte[] request)
    {
        var response = new List<byte> { 0x41 };

        for (int i = 1; i < request.Length; i++)
        {
            var pid = (Pid)request[i];
            response.Add((byte)pid);

            byte[] rawValue = RawValues.TryGetValue(pid, out byte[]? canned)
                ? canned
                : new byte[PidCatalog.GetExpectedByteLength(pid)];

            response.AddRange(rawValue);
        }

        return [.. response];
    }

    private static byte[] BuildDtcResponse(byte responseMode, byte[][] dtcBytePairs)
    {
        var response = new List<byte> { responseMode, (byte)dtcBytePairs.Length };
        foreach (byte[] pair in dtcBytePairs)
        {
            response.AddRange(pair);
        }

        return [.. response];
    }

    private static byte[] BuildMode09Response(byte[] request)
    {
        if (request.Length < 2 || request[1] != 0x02)
        {
            throw new NotSupportedException("FakeObdLink only supports Mode 09 PID 02 (VIN)");
        }

        var response = new List<byte> { 0x49, 0x02, 0x01 };
        response.AddRange(System.Text.Encoding.ASCII.GetBytes(FakeVin));
        return [.. response];
    }
}
