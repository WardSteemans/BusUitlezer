using TransitObd.Protocol;
using Xunit;

namespace TransitObd.Tests;

public class J1979ClientTests
{
    private static J1979Client CreateClient() => new(new FakeObdLink());

    [Fact]
    public async Task ReadLiveDataAsync_ReturnsParsedValuesForRequestedPids()
    {
        var client = CreateClient();
        Pid[] pids = [Pid.EngineRpm, Pid.VehicleSpeed, Pid.CoolantTemp];

        var values = await client.ReadLiveDataAsync(pids);

        Assert.Equal(3, values.Count);

        var rpm = values.Single(v => v.Pid == Pid.EngineRpm);
        Assert.Equal(2150.0, rpm.InterpretedValue);
        Assert.Equal("rpm", rpm.Unit);

        var speed = values.Single(v => v.Pid == Pid.VehicleSpeed);
        Assert.Equal(62.0, speed.InterpretedValue);
        Assert.Equal("km/h", speed.Unit);

        var coolant = values.Single(v => v.Pid == Pid.CoolantTemp);
        Assert.Equal(89.0, coolant.InterpretedValue);
        Assert.Equal("°C", coolant.Unit);
    }

    [Fact]
    public async Task ReadStoredDtcsAsync_ReturnsTwoKnownCodes()
    {
        var client = CreateClient();

        var dtcs = await client.ReadStoredDtcsAsync();

        Assert.Equal(2, dtcs.Count);
        Assert.Contains(dtcs, d => d.Code == "P0301");
        Assert.Contains(dtcs, d => d.Code == "P0171");
    }

    [Fact]
    public async Task ReadPendingDtcsAsync_ReturnsEmpty()
    {
        var client = CreateClient();

        var dtcs = await client.ReadPendingDtcsAsync();

        Assert.Empty(dtcs);
    }

    [Fact]
    public async Task ReadPermanentDtcsAsync_ReturnsEmpty()
    {
        var client = CreateClient();

        var dtcs = await client.ReadPermanentDtcsAsync();

        Assert.Empty(dtcs);
    }

    [Fact]
    public async Task ClearDtcsAsync_CompletesSuccessfully()
    {
        var client = CreateClient();

        await client.ClearDtcsAsync();
    }

    [Fact]
    public async Task ReadVinAsync_ReturnsFakeVin()
    {
        var client = CreateClient();

        var vin = await client.ReadVinAsync();

        Assert.Equal("FAKEVINTESTM10001", vin);
    }
}
