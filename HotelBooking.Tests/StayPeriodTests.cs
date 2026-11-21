using System.Globalization;
using HotelBooking.Core.Domain;
using Xunit;

namespace HotelBooking.Tests;

public class StayPeriodTests
{
    [Theory]
    [InlineData("2026-03-13", "2026-03-15", false)]
    [InlineData("2026-03-12", "2026-03-14", true)]
    [InlineData("2026-03-08", "2026-03-10", false)]
    [InlineData("2026-03-11", "2026-03-12", true)]
    [InlineData("2026-03-09", "2026-03-14", true)]
    public void Overlaps_WithMarch10To13_MatchesDecisionTable(
        string checkIn, string checkOut, bool expected)
    {
        StayPeriod existing = new(new DateOnly(2026, 3, 10), new DateOnly(2026, 3, 13));
        StayPeriod candidate = new(Day(checkIn), Day(checkOut));

        bool actual = existing.Overlaps(candidate);

        Assert.Equal(expected, actual);
        Assert.Equal(expected, candidate.Overlaps(existing));
    }

    [Theory]
    [InlineData("2026-03-10", true)]
    [InlineData("2026-03-12", true)]
    [InlineData("2026-03-13", false)]
    [InlineData("2026-03-09", false)]
    public void Contains_Date_IncludesCheckInExcludesCheckOut(string date, bool expected)
    {
        StayPeriod period = new(new DateOnly(2026, 3, 10), new DateOnly(2026, 3, 13));

        Assert.Equal(expected, period.Contains(Day(date)));
    }

    [Fact]
    public void FromNights_ThreeNights_ListsThreeDates()
    {
        StayPeriod period = StayPeriod.FromNights(new DateOnly(2026, 12, 30), 3);

        Assert.Equal(3, period.Nights);
        Assert.Equal(new DateOnly(2027, 1, 2), period.CheckOut);
        Assert.Equal(
            new[] { new DateOnly(2026, 12, 30), new(2026, 12, 31), new(2027, 1, 1) },
            period.EachNight());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void FromNights_NotPositive_Throws(int nights)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => StayPeriod.FromNights(new DateOnly(2026, 3, 10), nights));
    }

    [Fact]
    public void Ctor_CheckOutSameDay_Throws()
    {
        DateOnly day = new(2026, 3, 10);

        Assert.Throws<ArgumentException>(() => new StayPeriod(day, day));
    }

    private static DateOnly Day(string text) => DateOnly.Parse(text, CultureInfo.InvariantCulture);
}
