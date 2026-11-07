using System.Globalization;
using HotelBooking.Core.Bookings;
using HotelBooking.Core.Domain;
using HotelBooking.Core.Reporting;
using Xunit;

namespace HotelBooking.Tests;

public class BookingReportTests
{
    [Fact]
    public void TextOf_ContainsTotalLine()
    {
        TotalRequest request = BuildSampleRequest();
        TotalResult result = BookingTotalCalculator.Calculate(request);

        string text = BookingReport.TextOf(request, result);

        Assert.Contains("Разом: 1292.70", text);
    }

    [Fact]
    public void Print_WritesReportToOutput()
    {
        using StringWriter writer = new(CultureInfo.InvariantCulture);
        BookingReportPrinter printer = new(writer);

        printer.Print("Разом: 10.00");

        Assert.Contains("10.00", writer.ToString());
    }

    private static TotalRequest BuildSampleRequest()
    {
        List<BookingLine> lines = new()
        {
            new() { RoomNumber = "101", Nights = 12, NightlyRate = 100m },
            new() { RoomNumber = "204", Nights = 1, NightlyRate = 250m },
        };
        return new TotalRequest(
            BookingId: 1,
            Guest: new GuestInfo("Іваненко", "i@ex.com", true),
            Lines: lines,
            CreatedAt: new DateOnly(2026, 3, 10),
            CouponCode: null,
            Status: BookingStatus.New,
            TransferPrice: 60m);
    }
}
