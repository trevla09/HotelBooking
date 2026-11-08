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

    [Fact]
    public void Pay_LessThanTotal_ThrowsAmountRule()
    {
        FakeBookingRepository repository = new();
        BookingService service = NewService(repository, 250m, new SpyNotifier());
        Booking booking = NewBooking(5);
        booking.AddNight(new BookingNight("101", new DateOnly(2026, 11, 10), 250m));
        repository.Add(booking);

        DomainRuleException ex = Assert.Throws<DomainRuleException>(
            () => service.Pay(5, 249.99m));

        Assert.Equal("booking.amount", ex.Rule);
        Assert.Equal(BookingStatus.New, booking.Status);
    }

    [Theory]
    [InlineData(250)]
    [InlineData(300)]
    public void Pay_EnoughAmount_ConfirmsBooking(double amount)
    {
        FakeBookingRepository repository = new();
        BookingService service = NewService(repository, 250m, new SpyNotifier());
        Booking booking = NewBooking(6);
        booking.AddNight(new BookingNight("101", new DateOnly(2026, 11, 10), 250m));
        repository.Add(booking);

        service.Pay(6, (decimal)amount);

        Assert.Equal(BookingStatus.Confirmed, booking.Status);
    }

    [Fact]
    public void Pay_ConfirmedBooking_ThrowsPayableRule()
    {
        FakeBookingRepository repository = new();
        BookingService service = NewService(repository, 250m, new SpyNotifier());
        Booking booking = NewBooking(8);
        booking.AddNight(new BookingNight("101", new DateOnly(2026, 11, 10), 250m));
        booking.Confirm();
        repository.Add(booking);

        DomainRuleException ex = Assert.Throws<DomainRuleException>(
            () => service.Pay(8, 250m));

        Assert.Equal("booking.payable", ex.Rule);
    }

    [Fact]
    public void Cancel_NewBooking_NotifiesWithReason()
    {
        FakeBookingRepository repository = new();
        SpyNotifier notifier = new();
        BookingService service = NewService(repository, 250m, notifier);
        repository.Add(NewBooking(9));

        service.Cancel(9, "гість передумав");

        string message = Assert.Single(notifier.Messages);
        Assert.Contains("скасовано: гість передумав", message);
    }

    [Fact]
    public void Cancel_CheckedInBooking_ThrowsCancellableRule()
    {
        FakeBookingRepository repository = new();
        BookingService service = NewService(repository, 250m, new SpyNotifier());
        Booking booking = NewBooking(10);
        booking.AddNight(new BookingNight("101", new DateOnly(2026, 11, 10), 250m));
        booking.Confirm();
        booking.CheckIn();
        repository.Add(booking);

        DomainRuleException ex = Assert.Throws<DomainRuleException>(
            () => service.Cancel(10, "запізно"));

        Assert.Equal("booking.cancellable", ex.Rule);
    }

    [Fact]
    public void AddNight_NewBooking_AddsNight()
    {
        FakeBookingRepository repository = new();
        BookingService service = NewService(repository, 250m, new SpyNotifier());
        Booking booking = NewBooking(11);
        repository.Add(booking);

        service.AddNight(11, new BookingNight("101", new DateOnly(2026, 11, 10), 250m));

        Assert.Single(booking.Nights);
    }

    [Fact]
    public void AddNight_ConfirmedBooking_ThrowsEditableRule()
    {
        FakeBookingRepository repository = new();
        BookingService service = NewService(repository, 250m, new SpyNotifier());
        Booking booking = NewBooking(12);
        booking.AddNight(new BookingNight("101", new DateOnly(2026, 11, 10), 250m));
        booking.Confirm();
        repository.Add(booking);
        BookingNight night = new("101", new DateOnly(2026, 11, 11), 250m);

        DomainRuleException ex = Assert.Throws<DomainRuleException>(
            () => service.AddNight(12, night));

        Assert.Equal("booking.editable", ex.Rule);
    }

    [Fact]
    public void AddNight_SameNightTwice_ThrowsNightUniqueRule()
    {
        FakeBookingRepository repository = new();
        BookingService service = NewService(repository, 250m, new SpyNotifier());
        Booking booking = NewBooking(13);
        booking.AddNight(new BookingNight("101", new DateOnly(2026, 11, 10), 250m));
        repository.Add(booking);
        BookingNight night = new("101", new DateOnly(2026, 11, 10), 250m);

        DomainRuleException ex = Assert.Throws<DomainRuleException>(
            () => service.AddNight(13, night));

        Assert.Equal("booking.night-unique", ex.Rule);
    }

    [Fact]
    public void Constructor_NullRepository_ThrowsArgumentNull()
    {
        StubPricingPolicy pricing = new(1m);
        SpyNotifier notifier = new();
        NullLogger<BookingService> log = NullLogger<BookingService>.Instance;

        ArgumentNullException ex = Assert.Throws<ArgumentNullException>(
            () => _ = new BookingService(null!, pricing, notifier, log));

        Assert.Equal("repository", ex.ParamName);
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
