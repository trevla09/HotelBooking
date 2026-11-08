using HotelBooking.Core.Bookings;
using Xunit;

namespace HotelBooking.Tests;

public class VolumeDiscountOfTests
{
    [Fact]
    public void VolumeDiscountOf_NegativeAmount_Throws()
    {
        decimal amount = -0.01m;

        Assert.Throws<ArgumentOutOfRangeException>(
            () => _ = BookingTotalCalculator.VolumeDiscountOf(amount));
    }

    [Fact]
    public void VolumeDiscountOf_ZeroAmount_ReturnsZero()
    {
        decimal amount = 0m;

        decimal actual = BookingTotalCalculator.VolumeDiscountOf(amount);

        Assert.Equal(0m, actual);
    }

    [Fact]
    public void VolumeDiscountOf_SmallBooking_ReturnsZero()
    {
        decimal amount = 1500m;

        decimal actual = BookingTotalCalculator.VolumeDiscountOf(amount);

        Assert.Equal(0m, actual);
    }

    [Fact]
    public void VolumeDiscountOf_ExactlyMediumThreshold_ReturnsZero()
    {
        decimal amount = 2000m;

        decimal actual = BookingTotalCalculator.VolumeDiscountOf(amount);

        Assert.Equal(0m, actual);
    }

    [Fact]
    public void VolumeDiscountOf_JustAboveMediumThreshold_AppliesFivePercent()
    {
        decimal amount = 2000.01m;

        decimal actual = BookingTotalCalculator.VolumeDiscountOf(amount);

        Assert.Equal(100.0005m, actual);
    }

    [Fact]
    public void VolumeDiscountOf_ExactlyLargeThreshold_AppliesFivePercent()
    {
        decimal amount = 5000m;

        decimal actual = BookingTotalCalculator.VolumeDiscountOf(amount);

        Assert.Equal(250m, actual);
    }

    [Fact]
    public void VolumeDiscountOf_JustAboveLargeThreshold_AppliesTenPercent()
    {
        decimal amount = 5000.01m;

        decimal actual = BookingTotalCalculator.VolumeDiscountOf(amount);

        Assert.Equal(500.001m, actual);
    }

    [Fact]
    public void VolumeDiscountOf_LargeBooking_AppliesTenPercent()
    {
        decimal amount = 8000m;

        decimal actual = BookingTotalCalculator.VolumeDiscountOf(amount);

        Assert.Equal(800m, actual);
    }
}
