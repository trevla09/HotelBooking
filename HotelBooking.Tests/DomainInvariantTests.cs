using HotelBooking.Core.Abstractions;
using HotelBooking.Core.Domain;
using HotelBooking.Core.Pricing;
using HotelBooking.Core.Services;
using HotelBooking.Core.Storage;
using Xunit;

namespace HotelBooking.Tests;

public class DomainInvariantTests
{
    [Fact]
    public void BookingNight_WithNegativeRate_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => _ = new BookingNight("101", new DateOnly(2026, 11, 10), -1m));
    }

    [Fact]
    public void Booking_AfterConfirm_RejectsNewNights()
    {
        Booking booking = NewBooking(1);
        booking.AddNight(new BookingNight("101", new DateOnly(2026, 11, 10), 250m));
        booking.Confirm();

        Assert.Throws<InvalidOperationException>(
            () => booking.AddNight(new BookingNight("101", new DateOnly(2026, 11, 11), 250m)));
    }

    [Fact]
    public void TotalOf_WithTenPercentDiscount_Returns450()
    {
        BookingService service = new(
            new InMemoryBookingRepository(),
            new DiscountPricingPolicy(0.10m),
            new SilentNotifier());
        Booking booking = NewBooking(2);
        booking.AddNight(new BookingNight("101", new DateOnly(2026, 11, 10), 250m));
        booking.AddNight(new BookingNight("101", new DateOnly(2026, 11, 11), 250m));

        Assert.Equal(450m, service.TotalOf(booking));
    }

    private static Booking NewBooking(int id) =>
        new(id, 100, new DateTimeOffset(2026, 11, 1, 12, 0, 0, TimeSpan.Zero));

    private sealed class SilentNotifier : INotifier
    {
        public void Notify(string message)
        {
        }
    }
}
