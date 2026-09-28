using TransitObd.Protocol;

namespace TransitObd.Tests;

/// <summary>
/// Minimal <see cref="IObdLink"/> test double that always returns a fixed response,
/// regardless of the request. Used to drive J1979Client's error-handling paths with
/// malformed/unexpected bytes that FakeObdLink, by design, never produces.
/// </summary>
internal sealed class StubObdLink(byte[] response) : IObdLink
{
    public Task<byte[]> SendRequestAsync(byte[] request, CancellationToken cancellationToken = default)
        => Task.FromResult(response);
}
