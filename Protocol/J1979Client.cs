using System.Text;

namespace TransitObd.Protocol;

/// <summary>
/// High-level SAE J1979 (OBD-II) operations: live PID data, stored/pending/permanent DTCs,
/// clear DTCs, VIN. Ported from the reference implementation's <c>J1979</c> impl in
/// faraday-core/src/protocol/j1979.rs. Talks to whatever <see cref="IObdLink"/> it is given
/// — a <see cref="FakeObdLink"/> in M1, a real serial adapter in M2 — without knowing which.
/// </summary>
public sealed class J1979Client(IObdLink link)
{
    public async Task<IReadOnlyList<PidValue>> ReadLiveDataAsync(
        IReadOnlyList<Pid> pids, CancellationToken cancellationToken = default)
    {
        var request = new List<byte> { 0x01 };
        request.AddRange(pids.Select(p => (byte)p));

        byte[] response = await link.SendRequestAsync([.. request], cancellationToken);
        return ParseMode01Response(response, pids);
    }

    public async Task<IReadOnlyList<Dtc>> ReadStoredDtcsAsync(CancellationToken cancellationToken = default)
    {
        byte[] response = await link.SendRequestAsync([0x03], cancellationToken);
        return ParseDtcResponse(response);
    }

    public async Task ClearDtcsAsync(CancellationToken cancellationToken = default)
    {
        byte[] response = await link.SendRequestAsync([0x04], cancellationToken);
        if (response.Length == 0 || response[0] != 0x44)
        {
            throw new InvalidOperationException("Failed to clear DTCs");
        }
    }

    public async Task<IReadOnlyList<Dtc>> ReadPendingDtcsAsync(CancellationToken cancellationToken = default)
    {
        byte[] response = await link.SendRequestAsync([0x07], cancellationToken);
        return ParseDtcResponse(response);
    }

    public async Task<string> ReadVinAsync(CancellationToken cancellationToken = default)
    {
        byte[] response = await link.SendRequestAsync([0x09, 0x02], cancellationToken);

        if (response.Length < 3 || response[0] != 0x49 || response[1] != 0x02)
        {
            throw new InvalidOperationException("Invalid VIN response");
        }

        string vin = Encoding.UTF8.GetString(response, 3, response.Length - 3);
        return vin.TrimEnd('\0');
    }

    public async Task<IReadOnlyList<Dtc>> ReadPermanentDtcsAsync(CancellationToken cancellationToken = default)
    {
        byte[] response = await link.SendRequestAsync([0x0A], cancellationToken);
        return ParseDtcResponse(response);
    }

    private static IReadOnlyList<PidValue> ParseMode01Response(byte[] response, IReadOnlyList<Pid> pids)
    {
        if (response.Length < 2 || response[0] != 0x41)
        {
            throw new InvalidOperationException("Invalid Mode 01 response");
        }

        var values = new List<PidValue>();
        int offset = 1;

        foreach (Pid pid in pids)
        {
            if (offset >= response.Length)
            {
                break;
            }

            if (response[offset] == (byte)pid)
            {
                offset += 1;
                int dataLength = PidCatalog.GetExpectedByteLength(pid);

                if (offset + dataLength <= response.Length)
                {
                    byte[] rawValue = response[offset..(offset + dataLength)];
                    values.Add(new PidValue(pid, rawValue));
                    offset += dataLength;
                }
                else
                {
                    break;
                }
            }
            else
            {
                offset += 1;
            }
        }

        return values;
    }

    private static IReadOnlyList<Dtc> ParseDtcResponse(byte[] response)
    {
        if (response.Length < 2)
        {
            throw new InvalidOperationException("Invalid DTC response");
        }

        int numDtcs = response[1];
        var dtcs = new List<Dtc>();

        int offset = 2;
        for (int i = 0; i < numDtcs; i++)
        {
            if (offset + 2 > response.Length)
            {
                break;
            }

            dtcs.Add(Dtc.FromBytes(response[offset..(offset + 2)]));
            offset += 2;
        }

        return dtcs;
    }
}
