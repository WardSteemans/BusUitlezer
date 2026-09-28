using TransitObd.Protocol;
using Xunit;

namespace TransitObd.Tests;

public class DtcTests
{
    [Fact]
    public void FromBytes_PowertrainCode_DecodesCorrectly()
    {
        var dtc = Dtc.FromBytes([0x03, 0x01]);

        Assert.Equal("P0301", dtc.Code);
    }

    [Fact]
    public void FromBytes_ChassisCode_DecodesCorrectly()
    {
        var dtc = Dtc.FromBytes([0x40, 0x01]);

        Assert.Equal("C0001", dtc.Code);
    }

    [Fact]
    public void FromBytes_BodyCode_DecodesCorrectly()
    {
        var dtc = Dtc.FromBytes([0x80, 0x01]);

        Assert.Equal("B0001", dtc.Code);
    }

    [Fact]
    public void FromBytes_NetworkCode_DecodesCorrectly()
    {
        var dtc = Dtc.FromBytes([0xC1, 0x00]);

        Assert.Equal("U0100", dtc.Code);
    }

    [Fact]
    public void FromBytes_WrongLength_Throws()
    {
        Assert.Throws<ArgumentException>(() => Dtc.FromBytes([0x03]));
        Assert.Throws<ArgumentException>(() => Dtc.FromBytes([0x03, 0x01, 0x00]));
    }
}
