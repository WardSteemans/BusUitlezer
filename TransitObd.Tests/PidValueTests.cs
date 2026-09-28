using TransitObd.Protocol;
using Xunit;

namespace TransitObd.Tests;

public class PidValueTests
{
    [Fact]
    public void EngineRpm_TwoByteResponse_IsDividedByFour()
    {
        // raw = 0x0C 0xEC = 3308 -> 3308 / 4 = 827 rpm
        var value = new PidValue(Pid.EngineRpm, [0x0C, 0xEC]);

        Assert.Equal(827.0, value.InterpretedValue);
        Assert.Equal("rpm", value.Unit);
    }

    [Fact]
    public void VehicleSpeed_SingleByteResponse_IsRawValueInKmh()
    {
        var value = new PidValue(Pid.VehicleSpeed, [0x64]); // 100 (0x64) km/h

        Assert.Equal(100.0, value.InterpretedValue);
        Assert.Equal("km/h", value.Unit);
    }

    [Fact]
    public void CoolantTemp_SingleByteResponse_SubtractsForty()
    {
        var value = new PidValue(Pid.CoolantTemp, [0x5A]); // 90 (0x5A) - 40 = 50 degC

        Assert.Equal(50.0, value.InterpretedValue);
        Assert.Equal("°C", value.Unit);
    }

    [Fact]
    public void ControlModuleVoltage_TwoByteResponse_IsDividedByOneThousand()
    {
        // raw = 0x35 0xE8 = 13800 -> 13800 / 1000 = 13.8 V
        var value = new PidValue(Pid.ControlModuleVoltage, [0x35, 0xE8]);

        Assert.Equal(13.8, value.InterpretedValue);
        Assert.Equal("V", value.Unit);
    }

    [Fact]
    public void ThrottlePosition_SingleByteResponse_IsScaledToPercent()
    {
        var value = new PidValue(Pid.ThrottlePosition, [0xFF]); // 255 -> 100 %

        Assert.Equal(100.0, value.InterpretedValue);
        Assert.Equal("%", value.Unit);
    }

    [Fact]
    public void EmptyRawValue_ReturnsNullInterpretation()
    {
        var value = new PidValue(Pid.EngineRpm, []);

        Assert.Null(value.InterpretedValue);
        Assert.Null(value.Unit);
    }
}
