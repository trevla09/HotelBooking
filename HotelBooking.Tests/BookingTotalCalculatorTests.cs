using HotelBooking.Core.Bookings;
using HotelBooking.Core.Domain;
using Xunit;

namespace HotelBooking.Tests;

public class BookingTotalCalculatorTests
{
    [Fact]
    public void Calculate_RegularGuest_Returns1292()
    {
        List<BookingLine> lines = new()
        {
            new() { RoomNumber = "101", Nights = 12, NightlyRate = 100m },
            new() { RoomNumber = "204", Nights = 1, NightlyRate = 250m },
        };
        TotalRequest request = new(
            BookingId: 1,
            Guest: new GuestInfo("Іваненко", "i@ex.com", true),
            Lines: lines,
            CreatedAt: new DateOnly(2026, 3, 10),
            CouponCode: null,
            Status: BookingStatus.New,
            TransferPrice: 60m);

        TotalResult result = BookingTotalCalculator.Calculate(request);

        Assert.Equal(1292.70m, result.Total);
        Assert.Equal(BookingStatus.Confirmed, result.Status);
    }

    [Fact]
    public void Calculate_SmallBooking_AddsTransfer()
    {
        List<BookingLine> lines = new()
        {
            new() { RoomNumber = "101", Nights = 2, NightlyRate = 200m },
        };
        TotalRequest request = new(
            BookingId: 2,
            Guest: new GuestInfo("Петренко", "p@ex.com", false),
            Lines: lines,
            CreatedAt: new DateOnly(2026, 3, 10),
            CouponCode: "SALE10",
            Status: BookingStatus.Confirmed,
            TransferPrice: 60m);

        TotalResult result = BookingTotalCalculator.Calculate(request);

        Assert.Equal(420m, result.Total);
        Assert.Equal(BookingStatus.CheckedIn, result.Status);
    }
}
