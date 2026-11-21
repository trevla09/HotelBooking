using System.Globalization;
using HotelBooking.Core.Domain;
using HotelBooking.Core.Pricing;
using Xunit;

namespace HotelBooking.Tests;

public class SeasonalPricingPolicyTests
{
    [Theory]
    [InlineData("2026-02-10", 1000, 1000)]
    [InlineData("2026-03-31", 1000, 1000)]
    [InlineData("2026-04-01", 1000, 1200)]
    [InlineData("2026-05-31", 1000, 1200)]
    [InlineData("2026-06-01", 1000, 1500)]
    [InlineData("2026-08-31", 1000, 1500)]
    [InlineData("2026-09-01", 1000, 1200)]
    [InlineData("2026-10-31", 1000, 1200)]
    [InlineData("2026-11-01", 1000, 1000)]
    [InlineData("2026-12-23", 1000, 1000)]
    [InlineData("2026-12-24", 1000, 1800)]
    [InlineData("2027-01-07", 1000, 1800)]
    [InlineData("2027-01-08", 1000, 1000)]
    public void PriceOf_DateOfNight_AppliesSeasonFactor(string date, int rate, int expected)
    {
        SeasonalPricingPolicy policy = new();
        BookingNight night = new("101", Day(date), rate);

        decimal actual = policy.PriceOf(night);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("2026-04-15", "333.33", "400.00")]
    [InlineData("2026-06-15", "333.33", "500.00")]
    [InlineData("2026-12-25", "0.01", "0.02")]
    [InlineData("2026-07-10", "0.03", "0.05")]
    [InlineData("2026-12-25", "0", "0")]
    public void PriceOf_Fractions_RoundsToKopiyokAwayFromZero(
        string date, string rate, string expected)
    {
        SeasonalPricingPolicy policy = new();
        BookingNight night = new("101", Day(date), Money(rate));

        decimal actual = policy.PriceOf(night);

        Assert.Equal(Money(expected), actual);
    }

    [Fact]
    public void PriceOf_NullNight_Throws()
    {
        SeasonalPricingPolicy policy = new();

        Assert.Throws<ArgumentNullException>(() => policy.PriceOf(null!));
    }

    [Fact]
    public void FactorOf_UnknownSeason_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => SeasonCalendar.FactorOf((Season)99));
    }

    private static DateOnly Day(string text) => DateOnly.Parse(text, CultureInfo.InvariantCulture);

    private static decimal Money(string text) => decimal.Parse(text, CultureInfo.InvariantCulture);
}
