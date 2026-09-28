using TransitObd.Protocol;
using Xunit;

namespace TransitObd.Tests;

/// <summary>
/// Negative-path tests: malformed / unexpected responses that a real ECU or adapter could
/// plausibly send, which FakeObdLink (by design, as a well-behaved fake) never produces.
/// Uses StubObdLink to inject arbitrary bytes directly.
/// </summary>
public class J1979ClientErrorTests
{
    [Fact]
    public async Task ReadLiveDataAsync_NegativeResponseInsteadOfMode01Echo_Throws()
    {
        var client = new J1979Client(new StubObdLink([0x7F, 0x01, 0x12])); // negative response (0x7F)

        await Assert.ThrowsAsync<InvalidOperationException>(() => client.ReadLiveDataAsync([Pid.EngineRpm]));
    }

    [Fact]
    public async Task ReadLiveDataAsync_TruncatedResponse_Throws()
    {
        var client = new J1979Client(new StubObdLink([0x41])); // header only, no PID/data

        await Assert.ThrowsAsync<InvalidOperationException>(() => client.ReadLiveDataAsync([Pid.EngineRpm]));
    }

    [Fact]
    public async Task ReadLiveDataAsync_DataShorterThanExpectedLength_SkipsThatPid()
    {
        // RPM (0x0C) needs 2 data bytes but only 1 follows -> parser stops without throwing,
        // mirroring the reference implementation's behaviour.
        var client = new J1979Client(new StubObdLink([0x41, 0x0C, 0x21]));

        var values = await client.ReadLiveDataAsync([Pid.EngineRpm]);

        Assert.Empty(values);
    }

    [Fact]
    public async Task ReadStoredDtcsAsync_TruncatedResponse_Throws()
    {
        var client = new J1979Client(new StubObdLink([0x43])); // missing DTC count byte

        await Assert.ThrowsAsync<InvalidOperationException>(() => client.ReadStoredDtcsAsync());
    }

    [Fact]
    public async Task ReadStoredDtcsAsync_CountExceedsAvailableBytes_StopsEarlyWithoutThrowing()
    {
        // Count says 3 DTCs but only one 2-byte pair actually follows.
        var client = new J1979Client(new StubObdLink([0x43, 0x03, 0x03, 0x01]));

        var dtcs = await client.ReadStoredDtcsAsync();

        var dtc = Assert.Single(dtcs);
        Assert.Equal("P0301", dtc.Code);
    }

    [Fact]
    public async Task ClearDtcsAsync_NegativeResponse_Throws()
    {
        var client = new J1979Client(new StubObdLink([0x7F, 0x04, 0x31]));

        await Assert.ThrowsAsync<InvalidOperationException>(() => client.ClearDtcsAsync());
    }

    [Fact]
    public async Task ClearDtcsAsync_EmptyResponse_Throws()
    {
        var client = new J1979Client(new StubObdLink([]));

        await Assert.ThrowsAsync<InvalidOperationException>(() => client.ClearDtcsAsync());
    }

    [Fact]
    public async Task ReadVinAsync_NegativeResponseInsteadOfMode09Echo_Throws()
    {
        var client = new J1979Client(new StubObdLink([0x7F, 0x09, 0x12]));

        await Assert.ThrowsAsync<InvalidOperationException>(() => client.ReadVinAsync());
    }

    [Fact]
    public async Task ReadVinAsync_TooShortResponse_Throws()
    {
        var client = new J1979Client(new StubObdLink([0x49, 0x02]));

        await Assert.ThrowsAsync<InvalidOperationException>(() => client.ReadVinAsync());
    }

    [Fact]
    public async Task ReadPendingDtcsAsync_TruncatedResponse_Throws()
    {
        var client = new J1979Client(new StubObdLink([0x47]));

        await Assert.ThrowsAsync<InvalidOperationException>(() => client.ReadPendingDtcsAsync());
    }

    [Fact]
    public async Task ReadPermanentDtcsAsync_TruncatedResponse_Throws()
    {
        var client = new J1979Client(new StubObdLink([0x4A]));

        await Assert.ThrowsAsync<InvalidOperationException>(() => client.ReadPermanentDtcsAsync());
    }
}
