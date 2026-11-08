using HotelBooking.Core.Domain;
using HotelBooking.Core.Errors;
using HotelBooking.Core.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HotelBooking.Tests;

public class BookingServiceTests
{
    [Fact]
    public void TotalOf_TwoNights_SumsPricesFromPolicy()
    {
        BookingService service = NewService(new FakeBookingRepository(), 250m, new SpyNotifier());
        Booking booking = NewBooking(7);
        booking.AddNight(new BookingNight("101", new DateOnly(2026, 11, 10), 100m));
        booking.AddNight(new BookingNight("204", new DateOnly(2026, 11, 10), 900m));

        decimal actual = service.TotalOf(booking);

        Assert.Equal(500m, actual);
    }

    [Fact]
    public void Place_Booking_StoresInRepositoryOnce()
    {
        FakeBookingRepository repository = new();
        BookingService service = NewService(repository, 1m, new SpyNotifier());
        Booking booking = NewBooking(1);

        service.Place(booking);

        Assert.Same(booking, repository.GetById(1));
        Assert.Equal(1, repository.AddCalls);
    }

    [Fact]
    public void Load_UnknownId_ThrowsDomainRule()
    {
        BookingService service = NewService(new FakeBookingRepository(), 1m, new SpyNotifier());

        DomainRuleException ex = Assert.Throws<DomainRuleException>(
            () => _ = service.Load(42));

        Assert.Equal("booking.exists", ex.Rule);
    }

    [Fact]
    public void Place_BookingWithNights_NotifiesWithTotal()
    {
        SpyNotifier notifier = new();
        BookingService service = NewService(new FakeBookingRepository(), 250m, notifier);
        Booking booking = NewBooking(3);
        booking.AddNight(new BookingNight("101", new DateOnly(2026, 11, 10), 100m));
        booking.AddNight(new BookingNight("101", new DateOnly(2026, 11, 11), 100m));

        service.Place(booking);

        string message = Assert.Single(notifier.Messages);
        Assert.Contains("сума 500.00 грн", message);
    }

    private static BookingService NewService(
        FakeBookingRepository repository, decimal price, SpyNotifier notifier) =>
        new(
            repository,
            new StubPricingPolicy(price),
            notifier,
            NullLogger<BookingService>.Instance);

    private static Booking NewBooking(int id) =>
        new(id, 100, new DateTimeOffset(2026, 11, 1, 12, 0, 0, TimeSpan.Zero));
}
