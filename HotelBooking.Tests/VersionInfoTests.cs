using HotelBooking.Core;
using Xunit;

namespace HotelBooking.Tests;

public class VersionInfoTests
{
    [Fact]
    public void Current_FromBuildProps_ReturnsSemanticVersion()
    {
        string version = VersionInfo.Current();

        Assert.Equal("0.8.0", version);
    }
}
