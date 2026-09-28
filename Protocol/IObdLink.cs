namespace TransitObd.Protocol;

/// <summary>
/// Transport-agnostic OBD-II link: send raw Mode/PID request bytes, get the raw response
/// bytes back. Implemented by both a real serial-port adapter (M2) and
/// <see cref="FakeObdLink"/> (M1), so <see cref="J1979Client"/> never needs to know which
/// one it's talking to.
/// </summary>
public interface IObdLink
{
    Task<byte[]> SendRequestAsync(byte[] request, CancellationToken cancellationToken = default);
}
