using HotelBooking.Core.Domain;
using HotelBooking.Core.Pricing;
using HotelBooking.Core.Reporting;
using Xunit;

namespace HotelBooking.Tests;

public class PricingAndReportTests
{
    [Fact]
    public void StandardPolicy_Night_ReturnsRate()
    {
        StandardPricingPolicy policy = new();

        decimal actual = policy.PriceOf(new BookingNight("101", new DateOnly(2026, 11, 10), 250m));

        Assert.Equal(250m, actual);
    }

    [Fact]
    public void CorporatePolicy_Night_AppliesTenPercent()
    {
        CorporatePricingPolicy policy = new();

        decimal actual = policy.PriceOf(new BookingNight("101", new DateOnly(2026, 11, 10), 250m));

        Assert.Equal(225m, actual);
    }

    [Fact]
    public void BuildCsv_OneBooking_WritesHeaderAndRow()
    {
        Booking booking = new(1, 100, new DateTimeOffset(2026, 11, 1, 12, 0, 0, TimeSpan.Zero));
        booking.AddNight(new BookingNight("101", new DateOnly(2026, 11, 10), 250m));

        string csv = BookingCsvReport.BuildCsv(new List<Booking> { booking });

        Assert.Contains("id;total;status", csv);
        Assert.Contains("1;250.00;New", csv);
    }
}
