using HotelBooking.Core.Domain;
using HotelBooking.Core.Errors;
using HotelBooking.Core.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HotelBooking.Tests;

public class PayAsResultTests
{
    [Fact]
    public void PayAsResult_LessThanTotal_FailsWithAmountCode()
    {
        FakeBookingRepository repository = new();
        BookingService service = NewService(repository);
        Booking booking = NewBookingWithNight(5);
        repository.Add(booking);

        Result<decimal> result = service.PayAsResult(5, 249.99m);

        Assert.False(result.IsSuccess);
        Assert.Equal("booking.amount", result.Code);
        Assert.Equal(BookingStatus.New, booking.Status);
    }

    [Theory]
    [InlineData(250)]
    [InlineData(300)]
    public void PayAsResult_EnoughAmount_ConfirmsAndReturnsTotal(double amount)
    {
        FakeBookingRepository repository = new();
        BookingService service = NewService(repository);
        Booking booking = NewBookingWithNight(6);
        repository.Add(booking);

        Result<decimal> result = service.PayAsResult(6, (decimal)amount);

        Assert.True(result.IsSuccess);
        Assert.Equal(250m, result.Value);
        Assert.Equal(string.Empty, result.Code);
        Assert.Equal(BookingStatus.Confirmed, booking.Status);
    }

    [Fact]
    public void PayAsResult_UnknownId_FailsWithExistsCode()
    {
        BookingService service = NewService(new FakeBookingRepository());

        Result<decimal> result = service.PayAsResult(42, 250m);

        Assert.False(result.IsSuccess);
        Assert.Equal("booking.exists", result.Code);
    }

    [Fact]
    public void PayAsResult_ConfirmedBooking_FailsWithPayableCode()
    {
        FakeBookingRepository repository = new();
        BookingService service = NewService(repository);
        Booking booking = NewBookingWithNight(8);
        booking.Confirm();
        repository.Add(booking);

        Result<decimal> result = service.PayAsResult(8, 250m);

        Assert.False(result.IsSuccess);
        Assert.Equal("booking.payable", result.Code);
    }

    [Fact]
    public void Result_FailWithoutCode_UsesInputCode()
    {
        Result<int> result = Result<int>.Fail("Тариф - число");

        Assert.Equal("input", result.Code);
    }

    private static BookingService NewService(FakeBookingRepository repository) =>
        new(
            repository,
            new StubPricingPolicy(250m),
            new SpyNotifier(),
            NullLogger<BookingService>.Instance);

    private static Booking NewBookingWithNight(int id)
    {
        Booking booking = new(
            id, 100, new DateTimeOffset(2026, 11, 1, 12, 0, 0, TimeSpan.Zero));
        booking.AddNight(
            new BookingNight("101", new DateOnly(2026, 11, 10), 250m));
        return booking;
    }
}
