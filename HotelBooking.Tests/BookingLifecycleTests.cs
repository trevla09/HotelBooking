using HotelBooking.Core.Domain;
using HotelBooking.Core.Errors;
using HotelBooking.Core.Pricing;
using HotelBooking.Core.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HotelBooking.Tests;

public class BookingLifecycleTests
{
    private readonly FakeBookingRepository _repository = new();
    private readonly BookingService _service;

    public BookingLifecycleTests()
    {
        _service = new BookingService(
            _repository,
            new SeasonalPricingPolicy(),
            new SpyNotifier(),
            NullLogger<BookingService>.Instance);
    }

    [Fact]
    public void Pay_ExactAmount_ConfirmsBooking()
    {
        Booking booking = PlaceJuneBooking(1);

        _service.Pay(1, 3000m);

        Assert.Equal(BookingStatus.Confirmed, booking.Status);
    }

    [Fact]
    public void Pay_NotEnough_ThrowsAndKeepsNew()
    {
        Booking booking = PlaceJuneBooking(1);

        DomainRuleException ex = Assert.Throws<DomainRuleException>(
            () => _service.Pay(1, 2999.99m));

        Assert.Equal("booking.amount", ex.Rule);
        Assert.Equal(BookingStatus.New, booking.Status);
    }

    [Fact]
    public void CheckIn_ConfirmedBooking_BecomesCheckedIn()
    {
        Booking booking = PlaceJuneBooking(1);
        _service.Pay(1, 3000m);

        _service.CheckIn(1);

        Assert.Equal(BookingStatus.CheckedIn, booking.Status);
    }

    [Fact]
    public void CheckIn_NewBooking_ThrowsCheckInRule()
    {
        PlaceJuneBooking(1);

        DomainRuleException ex = Assert.Throws<DomainRuleException>(() => _service.CheckIn(1));

        Assert.Equal("booking.check-in", ex.Rule);
        Assert.Contains("спершу оплатіть", ex.Message);
    }

    [Fact]
    public void CheckIn_CancelledBooking_ThrowsCheckInRule()
    {
        PlaceJuneBooking(1);
        _service.Cancel(1, "гість не приїде");

        DomainRuleException ex = Assert.Throws<DomainRuleException>(() => _service.CheckIn(1));

        Assert.Equal("booking.check-in", ex.Rule);
        Assert.Contains("неможливе", ex.Message);
    }

    [Fact]
    public void CheckOut_CheckedIn_BecomesCheckedOutAndReturnsTotal()
    {
        Booking booking = PlaceJuneBooking(1);
        _service.Pay(1, 3000m);
        _service.CheckIn(1);

        decimal total = _service.CheckOut(1);

        Assert.Equal(BookingStatus.CheckedOut, booking.Status);
        Assert.Equal(3000m, total);
    }

    [Fact]
    public void CheckOut_Confirmed_ThrowsCheckOutRule()
    {
        PlaceJuneBooking(1);
        _service.Pay(1, 3000m);

        DomainRuleException ex = Assert.Throws<DomainRuleException>(() => _service.CheckOut(1));

        Assert.Equal("booking.check-out", ex.Rule);
    }

    [Fact]
    public void Cancel_CheckedIn_ThrowsCancellable()
    {
        PlaceJuneBooking(1);
        _service.Pay(1, 3000m);
        _service.CheckIn(1);

        DomainRuleException ex = Assert.Throws<DomainRuleException>(
            () => _service.Cancel(1, "передумав"));

        Assert.Equal("booking.cancellable", ex.Rule);
    }

    [Fact]
    public void TotalOf_StayAcrossSeasons_SumsEachNightFactor()
    {
        Booking booking = new(5, 1, DateTimeOffset.UnixEpoch);
        booking.AddNight(new BookingNight("101", new DateOnly(2026, 5, 31), 1000m));
        booking.AddNight(new BookingNight("101", new DateOnly(2026, 6, 1), 1000m));

        Assert.Equal(2700m, _service.TotalOf(booking));
    }

    [Fact]
    public void Lifecycle_EachTransition_SavesRepository()
    {
        PlaceJuneBooking(1);
        _service.Pay(1, 3000m);
        _service.CheckIn(1);
        _service.CheckOut(1);

        Assert.Equal(4, _repository.SaveCalls);
    }

    private Booking PlaceJuneBooking(int id)
    {
        Booking booking = new(id, 1, DateTimeOffset.UnixEpoch);
        booking.AddNight(new BookingNight("101", new DateOnly(2026, 6, 10), 1000m));
        booking.AddNight(new BookingNight("101", new DateOnly(2026, 6, 11), 1000m));
        _service.Place(booking);
        return booking;
    }
}
