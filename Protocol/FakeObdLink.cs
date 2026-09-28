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
        [Pid.EngineRpm] = [0x21, 0x98],            // 2150 rpm
        [Pid.VehicleSpeed] = [0x3E],                // 62 km/h
        [Pid.CoolantTemp] = [0x81],                 // 89 degC
        [Pid.IntakeAirTemp] = [0x40],                // 24 degC
        [Pid.ThrottlePosition] = [0x2E],             // ~18 %
        [Pid.FuelTankLevel] = [0x8C],                // ~55 %
        [Pid.ControlModuleVoltage] = [0x35, 0xE8],   // 13.800 V
    };

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
