using TransitObd.Protocol;
using Xunit;

namespace TransitObd.Tests;

public class FakeObdLinkTests
{
    [Fact]
    public void RawValues_HasAnExplicitEntryForEveryPid()
    {
        foreach (Pid pid in Enum.GetValues<Pid>())
        {
            Assert.True(FakeObdLink.HasCannedValue(pid), $"No canned value for {pid} — would silently zero-fill.");
        }
    }

    // Expected values are written as the exact same expression as PidValue's formula for
    // that PID (e.g. raw * 100.0 / 255.0), not hand-typed decimals: since both sides
    // evaluate identical IEEE-754 double arithmetic, equality is exact, with no rounding
    // tolerance needed even for non-terminating fractions like x/255.
    [Theory]
    [InlineData(Pid.EngineLoad, 89 * 100.0 / 255.0, "%")]                 // raw 0x59
    [InlineData(Pid.CoolantTemp, 129.0 - 40.0, "°C")]                     // raw 0x81
    [InlineData(Pid.ShortFuelTrimBank1, (131.0 - 128.0) * 100.0 / 128.0, "%")] // raw 0x83
    [InlineData(Pid.LongFuelTrimBank1, (127.0 - 128.0) * 100.0 / 128.0, "%")]  // raw 0x7F
    [InlineData(Pid.ShortFuelTrimBank2, (130.0 - 128.0) * 100.0 / 128.0, "%")] // raw 0x82
    [InlineData(Pid.LongFuelTrimBank2, (125.0 - 128.0) * 100.0 / 128.0, "%")]  // raw 0x7D
    [InlineData(Pid.EngineRpm, 8600.0 / 4.0, "rpm")]                      // raw 0x21,0x98
    [InlineData(Pid.VehicleSpeed, 62.0, "km/h")]                          // raw 0x3E
    [InlineData(Pid.TimingAdvance, 148.0 / 2.0 - 64.0, "°")]              // raw 0x94
    [InlineData(Pid.IntakeAirTemp, 64.0 - 40.0, "°C")]                    // raw 0x40
    [InlineData(Pid.MafRate, 1250.0 / 100.0, "g/s")]                      // raw 0x04,0xE2
    [InlineData(Pid.ThrottlePosition, 46 * 100.0 / 255.0, "%")]           // raw 0x2E
    [InlineData(Pid.OxygenSensorsPresent, 3.0, null)]                     // raw 0x03
    [InlineData(Pid.O2Bank1Sensor1Voltage, 90 * 0.005, "V")]              // raw 0x5A,0x80
    [InlineData(Pid.O2Bank1Sensor2Voltage, 124 * 0.005, "V")]             // raw 0x7C,0x80
    [InlineData(Pid.EgrCommanded, 51 * 100.0 / 255.0, "%")]               // raw 0x33
    [InlineData(Pid.EgrError, (124.0 - 128.0) * 100.0 / 128.0, "%")]      // raw 0x7C
    [InlineData(Pid.FuelTankLevel, 140 * 100.0 / 255.0, "%")]             // raw 0x8C
    [InlineData(Pid.ControlModuleVoltage, 13800.0 / 1000.0, "V")]         // raw 0x35,0xE8
    [InlineData(Pid.FuelAirEquivalenceRatio, 32768.0 * 2.0 / 65536.0, "λ")] // raw 0x80,0x00,0x00,0x00
    [InlineData(Pid.AmbientAirTemp, 58.0 - 40.0, "°C")]                   // raw 0x3A
    [InlineData(Pid.RuntimeWithMilOn, 12.0, "min")]                       // raw 0x00,0x0C
    [InlineData(Pid.RuntimeSinceCodesCleared, 340.0, "min")]              // raw 0x01,0x54
    [InlineData(Pid.RelativeThrottlePosition, 13 * 100.0 / 255.0, "%")]   // raw 0x0D
    [InlineData(Pid.EngineOilTemp, 135.0 - 40.0, "°C")]                   // raw 0x87
    [InlineData(Pid.EngineFuelRate, 64.0 * 0.05, "L/h")]                  // raw 0x00,0x40
    [InlineData(Pid.DriverDemandEngineTorque, 115.0 - 125.0, "%")]        // raw 0x73
    [InlineData(Pid.ActualEngineTorque, 117.0 - 125.0, "%")]              // raw 0x75
    [InlineData(Pid.NoxReagentLevel, 204 * 100.0 / 255.0, "%")]           // raw byte offset 5 = 0xCC
    [InlineData(Pid.DieselExhaustFluidLevel, 153 * 100.0 / 255.0, "%")]   // raw byte offset 3 = 0x99
    [InlineData(Pid.DieselExhaustFluidDosing, 40.0 / 2.0, "%")]           // raw byte offset 1 = 0x28
    [InlineData(Pid.FuelPressure, 100 * 3.0, "kPa")]                     // raw 0x64
    [InlineData(Pid.IntakeManifoldPressure, 105.0, "kPa")]                // raw 0x69
    [InlineData(Pid.FuelRailPressure, 10000 * 0.079, "kPa")]              // raw 0x27,0x10
    [InlineData(Pid.FuelRailGaugePressure, 25000 * 10.0, "kPa")]          // raw 0x61,0xA8
    [InlineData(Pid.FuelRailAbsolutePressure, 20000 * 10.0, "kPa")]       // raw 0x4E,0x20
    [InlineData(Pid.CylinderFuelRate, 1600.0 / 32.0, "mg/stroke")]        // raw 0x06,0x40
    [InlineData(Pid.MaxMafRate, 80 * 10.0, "g/s")]                        // raw 0x50,0x00,0x00,0x00
    [InlineData(Pid.FuelInjectionTiming, 26880.0 / 128.0 - 210.0, "°")]  // raw 0x69,0x00
    [InlineData(Pid.BarometricPressure, 101.0, "kPa")]                    // raw 0x65
    [InlineData(Pid.CommandedEvapPurge, 51 * 100.0 / 255.0, "%")]         // raw 0x33
    [InlineData(Pid.EvapSystemVaporPressure, -400.0 / 4.0, "Pa")]         // raw 0xFE,0x70 (signed -400)
    [InlineData(Pid.AbsoluteEvapSystemVaporPressure, 20000.0 / 200.0, "kPa")] // raw 0x4E,0x20
    [InlineData(Pid.EvapSystemVaporPressureRaw, -500.0, "Pa")]            // raw 0xFE,0x0C (signed -500)
    [InlineData(Pid.O2Sensor3Voltage, 90 * 0.005, "V")]                   // raw 0x5A,0x00
    [InlineData(Pid.O2Sensor4Voltage, 100 * 0.005, "V")]                  // raw 0x64,0x00
    [InlineData(Pid.O2Sensor5Voltage, 110 * 0.005, "V")]                  // raw 0x6E,0x00
    [InlineData(Pid.O2Sensor6Voltage, 120 * 0.005, "V")]                  // raw 0x78,0x00
    [InlineData(Pid.O2Sensor7Voltage, 130 * 0.005, "V")]                  // raw 0x82,0x00
    [InlineData(Pid.O2Sensor8Voltage, 140 * 0.005, "V")]                  // raw 0x8C,0x00
    [InlineData(Pid.O2Sensor1EquivalenceRatio, 32768.0 * 2.0 / 65536.0, "λ")] // raw 0x80,0x00,0x00,0x00
    [InlineData(Pid.O2Sensor2EquivalenceRatio, 32768.0 * 2.0 / 65536.0, "λ")] // raw 0x80,0x00,0x00,0x00
    [InlineData(Pid.O2Sensor3EquivalenceRatio, 32768.0 * 2.0 / 65536.0, "λ")] // raw 0x80,0x00,0x00,0x00
    [InlineData(Pid.O2Sensor4EquivalenceRatio, 32768.0 * 2.0 / 65536.0, "λ")] // raw 0x80,0x00,0x00,0x00
    [InlineData(Pid.O2Sensor5EquivalenceRatio, 32768.0 * 2.0 / 65536.0, "λ")] // raw 0x80,0x00,0x00,0x00
    [InlineData(Pid.O2Sensor6EquivalenceRatio, 32768.0 * 2.0 / 65536.0, "λ")] // raw 0x80,0x00,0x00,0x00
    [InlineData(Pid.O2Sensor7EquivalenceRatio, 32768.0 * 2.0 / 65536.0, "λ")] // raw 0x80,0x00,0x00,0x00
    [InlineData(Pid.O2Sensor8EquivalenceRatio, 32768.0 * 2.0 / 65536.0, "λ")] // raw 0x80,0x00,0x00,0x00
    [InlineData(Pid.O2Sensor1Current, 32768.0 / 256.0 - 128.0, "mA")]     // raw 0x80,0x00,0x80,0x00
    [InlineData(Pid.O2Sensor2Current, 32768.0 / 256.0 - 128.0, "mA")]     // raw 0x80,0x00,0x80,0x00
    [InlineData(Pid.O2Sensor3Current, 32768.0 / 256.0 - 128.0, "mA")]     // raw 0x80,0x00,0x80,0x00
    [InlineData(Pid.O2Sensor4Current, 32768.0 / 256.0 - 128.0, "mA")]     // raw 0x80,0x00,0x80,0x00
    [InlineData(Pid.O2Sensor5Current, 32768.0 / 256.0 - 128.0, "mA")]     // raw 0x80,0x00,0x80,0x00
    [InlineData(Pid.O2Sensor6Current, 32768.0 / 256.0 - 128.0, "mA")]     // raw 0x80,0x00,0x80,0x00
    [InlineData(Pid.O2Sensor7Current, 32768.0 / 256.0 - 128.0, "mA")]     // raw 0x80,0x00,0x80,0x00
    [InlineData(Pid.O2Sensor8Current, 32768.0 / 256.0 - 128.0, "mA")]     // raw 0x80,0x00,0x80,0x00
    [InlineData(Pid.SecondaryO2TrimShortBank1, 96 * 100.0 / 128.0 - 100.0, "%")] // raw 0x60,0x00
    [InlineData(Pid.SecondaryO2TrimLongBank1, 96 * 100.0 / 128.0 - 100.0, "%")]  // raw 0x60,0x00
    [InlineData(Pid.SecondaryO2TrimShortBank2, 96 * 100.0 / 128.0 - 100.0, "%")] // raw 0x60,0x00
    [InlineData(Pid.SecondaryO2TrimLongBank2, 96 * 100.0 / 128.0 - 100.0, "%")]  // raw 0x60,0x00
    [InlineData(Pid.OxygenSensorsPresent4Banks, 15.0, null)]              // raw 0x0F
    [InlineData(Pid.CatalystTempBank1Sensor1, 6000.0 / 10.0 - 40.0, "°C")] // raw 0x17,0x70
    [InlineData(Pid.CatalystTempBank2Sensor1, 6000.0 / 10.0 - 40.0, "°C")] // raw 0x17,0x70
    [InlineData(Pid.CatalystTempBank1Sensor2, 6000.0 / 10.0 - 40.0, "°C")] // raw 0x17,0x70
    [InlineData(Pid.CatalystTempBank2Sensor2, 6000.0 / 10.0 - 40.0, "°C")] // raw 0x17,0x70
    [InlineData(Pid.ThrottlePositionRelative, 51 * 100.0 / 255.0, "%")]   // raw 0x33
    [InlineData(Pid.AbsoluteThrottlePositionB, 102 * 100.0 / 255.0, "%")] // raw 0x66
    [InlineData(Pid.AbsoluteThrottlePositionC, 153 * 100.0 / 255.0, "%")] // raw 0x99
    [InlineData(Pid.AcceleratorPedalPositionD, 204 * 100.0 / 255.0, "%")] // raw 0xCC
    [InlineData(Pid.AcceleratorPedalPositionE, 255 * 100.0 / 255.0, "%")] // raw 0xFF
    [InlineData(Pid.AcceleratorPedalPositionF, 0 * 100.0 / 255.0, "%")]   // raw 0x00
    [InlineData(Pid.CommandedThrottleActuator, 102 * 100.0 / 255.0, "%")] // raw 0x66
    [InlineData(Pid.HybridBatteryPackRemainingLife, 153 * 100.0 / 255.0, "%")] // raw 0x99
    [InlineData(Pid.EngineReferenceTorque, 350.0, "N·m")]                 // raw 0x01,0x5E
    [InlineData(Pid.EngineFrictionPercentTorque, 120.0 - 125.0, "%")]     // raw 0x78
    [InlineData(Pid.RuntimeSinceEngineStart, 600.0, "s")]                 // raw 0x02,0x58
    [InlineData(Pid.DistanceWithMilOn, 45.0, "km")]                       // raw 0x00,0x2D
    [InlineData(Pid.DistanceSinceCodesCleared, 320.0, "km")]              // raw 0x01,0x40
    [InlineData(Pid.WarmUpsSinceCodesCleared, 5.0, null)]                 // raw 0x05
    [InlineData(Pid.Odometer, 1000000.0 / 10.0, "km")]                    // raw 0x00,0x0F,0x42,0x40
    [InlineData(Pid.TransmissionActualGear, 3200.0 / 1000.0, "ratio")]    // raw 0x02,0x00,0x0C,0x80
    [InlineData(Pid.AuxiliaryInputStatus, 1.0, null)]                     // raw 0x01
    public async Task ReadLiveDataAsync_EveryPid_ReturnsExpectedInterpretedValue(
        Pid pid, double expectedValue, string? expectedUnit)
    {
        var client = new J1979Client(new FakeObdLink());

        var values = await client.ReadLiveDataAsync([pid]);

        var value = Assert.Single(values);
        Assert.Equal(expectedValue, value.InterpretedValue!.Value);
        Assert.Equal(expectedUnit, value.Unit);
    }

    [Fact]
    public async Task SendRequestAsync_Mode04_ReturnsSuccessByte()
    {
        var link = new FakeObdLink();

        byte[] response = await link.SendRequestAsync([0x04]);

        Assert.Equal([0x44], response);
    }

    [Fact]
    public async Task SendRequestAsync_Mode07_ReturnsZeroDtcs()
    {
        var link = new FakeObdLink();

        byte[] response = await link.SendRequestAsync([0x07]);

        Assert.Equal([0x47, 0x00], response);
    }

    [Fact]
    public async Task SendRequestAsync_Mode0A_ReturnsZeroDtcs()
    {
        var link = new FakeObdLink();

        byte[] response = await link.SendRequestAsync([0x0A]);

        Assert.Equal([0x4A, 0x00], response);
    }

    [Fact]
    public async Task SendRequestAsync_UnsupportedMode_Throws()
    {
        var link = new FakeObdLink();

        await Assert.ThrowsAsync<NotSupportedException>(() => link.SendRequestAsync([0x02]));
    }

    [Fact]
    public async Task SendRequestAsync_Mode09WrongPid_Throws()
    {
        var link = new FakeObdLink();

        await Assert.ThrowsAsync<NotSupportedException>(() => link.SendRequestAsync([0x09, 0x01]));
    }

    [Fact]
    public async Task SendRequestAsync_EmptyRequest_Throws()
    {
        var link = new FakeObdLink();

        await Assert.ThrowsAsync<ArgumentException>(() => link.SendRequestAsync([]));
    }
}
