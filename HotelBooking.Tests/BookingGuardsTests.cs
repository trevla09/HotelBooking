using HotelBooking.Core.Bookings;
using HotelBooking.Core.Legacy;
using Xunit;

namespace HotelBooking.Tests;

public class BookingGuardsTests
{
    [Fact]
    public void Calculate_NoLines_ThrowsArgument()
    {
        Assert.Throws<ArgumentException>(() =>
            BookingGuards.EnsureLinesValid(new List<BookingLine>()));
    }

    [Fact]
    public void Calculate_ZeroNights_ThrowsRange()
    {
        BookingLine line = new()
        {
            RoomNumber = "101",
            Nights = 0,
            NightlyRate = 10m,
        };

        Assert.Throws<ArgumentOutOfRangeException>(
            () => BookingGuards.EnsureLineValid(line));
    }
}
