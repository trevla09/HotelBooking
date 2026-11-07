using HotelBooking.Core.Domain;
using HotelBooking.Core.Legacy;
using Xunit;

namespace HotelBooking.Tests;

public class LegacyProcessorTests
{
    [Fact]
    public void Process_RegularGuest_Returns1292()
    {
        List<BookingLine> lines = new()
        {
            new BookingLine { RoomNumber = "101", Nights = 12, NightlyRate = 100m },
            new BookingLine { RoomNumber = "204", Nights = 1, NightlyRate = 250m },
        };

        decimal total = LegacyProcessor.Process(
            1, "Іваненко", "i@ex.com", true, lines,
            new DateOnly(2026, 3, 10), string.Empty, BookingStatus.New, false,
            60m, out BookingStatus next);

        Assert.Equal(1292.70m, total);
        Assert.Equal(BookingStatus.Confirmed, next);
    }

    [Fact]
    public void Process_SmallBooking_AddsTransfer()
    {
        List<BookingLine> lines = new()
        {
            new BookingLine { RoomNumber = "101", Nights = 2, NightlyRate = 200m },
        };

        decimal total = LegacyProcessor.Process(
            2, "Петренко", "p@ex.com", false, lines,
            new DateOnly(2026, 3, 10), "SALE10", BookingStatus.Confirmed,
            false, 60m, out BookingStatus next);

        Assert.Equal(420m, total);
        Assert.Equal(BookingStatus.CheckedIn, next);
    }
}
