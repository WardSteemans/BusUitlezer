using System.Diagnostics;

namespace TransitObd.Protocol;

/// <summary>
/// A decoded Diagnostic Trouble Code. Decoding is ported verbatim from the reference
/// implementation's <c>Dtc::from_bytes</c> in faraday-core/src/protocol/j1979.rs — the
/// standard SAE J1979 2-byte DTC encoding.
/// </summary>
public sealed record Dtc(string Code, string Description)
{
    public static Dtc FromBytes(IReadOnlyList<byte> bytes)
    {
        if (bytes.Count != 2)
        {
            throw new ArgumentException("DTC must be 2 bytes", nameof(bytes));
        }

        byte firstByte = bytes[0];
        byte secondByte = bytes[1];

        char prefix = ((firstByte & 0xC0) >> 6) switch
        {
            0 => 'P',
            1 => 'C',
            2 => 'B',
            3 => 'U',
            _ => throw new UnreachableException(),
        };

        int firstDigit = (firstByte & 0x30) >> 4;
        int secondDigit = firstByte & 0x0F;
        byte thirdFourthDigits = secondByte;

        string code = $"{prefix}{firstDigit:X1}{secondDigit:X1}{thirdFourthDigits:X2}";
        string description = $"Diagnostic Trouble Code {code}";

        return new Dtc(code, description);
    }
}
