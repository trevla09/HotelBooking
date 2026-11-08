using HotelBooking.Core.Domain;
using Xunit;

namespace HotelBooking.Tests;

public class BookingTests
{
    [Fact]
    public void Constructor_ZeroId_ThrowsOutOfRange()
    {
        DateTimeOffset createdAt = new(2026, 11, 1, 12, 0, 0, TimeSpan.Zero);

        ArgumentOutOfRangeException ex = Assert.Throws<ArgumentOutOfRangeException>(
            () => _ = new Booking(0, 100, createdAt));

        Assert.Equal("id", ex.ParamName);
    }

    [Fact]
    public void Confirm_NoNights_ThrowsInvalidOperation()
    {
        Booking booking = NewBooking(1);

        Assert.Throws<InvalidOperationException>(() => booking.Confirm());
    }

    [Fact]
    public void AddNight_SameNightTwice_ThrowsInvalidOperation()
    {
        Booking booking = NewBooking(2);
        booking.AddNight(Night("101", 10, 250m));

        Assert.Throws<InvalidOperationException>(() => booking.AddNight(Night("101", 10, 300m)));
    }

    [Fact]
    public void CheckOut_AfterCheckIn_SetsCheckedOut()
    {
        Booking booking = NewBooking(3);
        booking.AddNight(Night("101", 10, 250m));
        booking.Confirm();
        booking.CheckIn();

        booking.CheckOut();

        Assert.Equal(BookingStatus.CheckedOut, booking.Status);
    }

    [Fact]
    public void CheckOut_ConfirmedBooking_ThrowsInvalidOperation()
    {
        Booking booking = NewBooking(4);
        booking.AddNight(Night("101", 10, 250m));
        booking.Confirm();

        Assert.Throws<InvalidOperationException>(() => booking.CheckOut());
    }

    [Fact]
    public void Cancel_NewBooking_SetsCancelled()
    {
        Booking booking = NewBooking(5);

        booking.Cancel();

        Assert.Equal(BookingStatus.Cancelled, booking.Status);
    }

    [Fact]
    public void Total_TwoNights_SumsRates()
    {
        Booking booking = NewBooking(6);
        booking.AddNight(Night("101", 10, 250m));
        booking.AddNight(Night("101", 11, 300m));

        decimal actual = booking.Total();

        Assert.Equal(550m, actual);
    }

    [Fact]
    public void ArchivedBooking_WrapsBooking_ExposesItsData()
    {
        Booking booking = NewBooking(7);
        booking.AddNight(Night("204", 12, 400m));
        DateOnly archivedAt = new(2026, 12, 1);

        ArchivedBooking archived = new(booking, archivedAt);

        Assert.Equal(7, archived.Id);
        Assert.Equal(BookingStatus.New, archived.Status);
        Assert.Single(archived.Nights);
        Assert.Equal(archivedAt, archived.ArchivedAt);
    }

    [Fact]
    public void CheckOutDate_Night_IsNextDay()
    {
        BookingNight night = Night("101", 30, 250m);

        DateOnly actual = night.CheckOutDate;

        Assert.Equal(new DateOnly(2026, 12, 1), actual);
    }

    private static Booking NewBooking(int id) =>
        new(id, 100, new DateTimeOffset(2026, 11, 1, 12, 0, 0, TimeSpan.Zero));

    private static BookingNight Night(string room, int day, decimal rate) =>
        new(room, new DateOnly(2026, 11, day), rate);
}
