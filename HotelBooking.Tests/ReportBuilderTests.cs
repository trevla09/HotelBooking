using HotelBooking.Core.Reporting;
using Xunit;

namespace HotelBooking.Tests;

public class ReportBuilderTests
{
    [Fact]
    public void Build_TwoRows_StartsWithUpperTitle()
    {
        ReportBuilder builder = new() { Title = "Заїзди" };
        string[] rows = ["101", "204"];
        string nl = Environment.NewLine;

        string text = builder.Build(rows);

        Assert.Equal($"ЗАЇЗДИ{nl}101{nl}204{nl}", text);
    }

    [Fact]
    public void Build_NullRows_ThrowsArgumentNull()
    {
        ReportBuilder builder = new();

        Assert.Throws<ArgumentNullException>(() => _ = builder.Build(null!));
    }
}
