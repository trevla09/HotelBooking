using HotelBooking.Core.Bookings;
using Xunit;

namespace HotelBooking.Tests;

public class VolumeDiscountOfTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(1500, 0)]
    [InlineData(2000, 0)]
    [InlineData(2000.01, 100.0005)]
    [InlineData(5000, 250)]
    [InlineData(5000.01, 500.001)]
    [InlineData(8000, 800)]
    public void VolumeDiscountOf_Boundaries_MatchesTable(double amount, double expected)
    {
        decimal value = (decimal)amount;

        decimal actual = BookingTotalCalculator.VolumeDiscountOf(value);

        Assert.Equal((decimal)expected, actual);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-1000)]
    public void VolumeDiscountOf_NegativeAmount_ThrowsOutOfRange(double amount)
    {
        decimal value = (decimal)amount;

        ArgumentOutOfRangeException ex = Assert.Throws<ArgumentOutOfRangeException>(
            () => _ = BookingTotalCalculator.VolumeDiscountOf(value));

        Assert.Equal("amount", ex.ParamName);
    }
}
