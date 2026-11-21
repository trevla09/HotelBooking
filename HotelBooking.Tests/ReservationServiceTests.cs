using HotelBooking.Core.Domain;
using HotelBooking.Core.Errors;
using HotelBooking.Core.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HotelBooking.Tests;

public class ReservationServiceTests
{
    private readonly FakeBookingRepository _repository = new();
    private readonly ReservationService _service;

    public ReservationServiceTests()
    {
        _service = new ReservationService(
            _repository,
            FakeHotelDirectory.WithThreeRooms(),
            new FixedClock(Today),
            NullLogger<ReservationService>.Instance);
    }

    private static DateOnly Today => new(2026, 3, 1);

    [Fact]
    public void Reserve_FreeRoom_CreatesNewBooking()
    {
        Booking booking = _service.Reserve(Request("101", 10, 3));

        Assert.Equal(BookingStatus.New, booking.Status);
        Assert.Equal(3, booking.Nights.Count);
        Assert.Equal(3000m, booking.Total());
        Assert.Same(booking, _repository.GetById(booking.Id));
        Assert.Equal(1, _repository.SaveCalls);
    }

    [Fact]
    public void Reserve_RoomBusy_ThrowsRoomBusy()
    {
        _service.Reserve(Request("101", 10, 3));

        DomainRuleException ex = Assert.Throws<DomainRuleException>(
            () => _service.Reserve(Request("101", 12, 2)));

        Assert.Equal("room.busy", ex.Rule);
        Assert.Single(_repository.GetAll());
    }

    [Fact]
    public void Reserve_CheckInOnOtherCheckOutDay_Allowed()
    {
        _service.Reserve(Request("101", 10, 3));

        Booking second = _service.Reserve(Request("101", 13, 2));

        Assert.Equal(2, second.Id);
        Assert.Equal(new DateOnly(2026, 3, 14), second.Nights[0].Date);
    }

    [Fact]
    public void Reserve_AfterCancel_RoomFreeAgain()
    {
        Booking first = _service.Reserve(Request("101", 10, 3));
        first.Cancel();

        Booking second = _service.Reserve(Request("101", 11, 1));

        Assert.Equal(BookingStatus.New, second.Status);
    }

    [Fact]
    public void Reserve_DateInPast_ThrowsStayPast()
    {
        DomainRuleException ex = Assert.Throws<DomainRuleException>(
            () => _service.Reserve(new ReserveRequest(
                1, "101", StayPeriod.FromNights(Today.AddDays(-1), 2))));

        Assert.Equal("stay.past", ex.Rule);
    }

    [Theory]
    [InlineData(30, true)]
    [InlineData(31, false)]
    public void Reserve_NightsLimit_AllowsUpToThirty(int nights, bool allowed)
    {
        if (allowed)
        {
            Assert.Equal(nights, _service.Reserve(Request("201", 1, nights)).Nights.Count);
            return;
        }

        DomainRuleException ex = Assert.Throws<DomainRuleException>(
            () => _service.Reserve(Request("201", 1, nights)));
        Assert.Equal("stay.too-long", ex.Rule);
    }

    [Fact]
    public void Reserve_UnknownRoom_ThrowsRoomExists()
    {
        DomainRuleException ex = Assert.Throws<DomainRuleException>(
            () => _service.Reserve(Request("999", 10, 1)));

        Assert.Equal("room.exists", ex.Rule);
    }

    [Fact]
    public void Reserve_UnknownGuest_ThrowsGuestExists()
    {
        DomainRuleException ex = Assert.Throws<DomainRuleException>(
            () => _service.Reserve(new ReserveRequest(
                77, "101", StayPeriod.FromNights(Today.AddDays(5), 1))));

        Assert.Equal("guest.exists", ex.Rule);
    }

    [Fact]
    public void FreeRooms_OneRoomBooked_ReturnsOthersInOrder()
    {
        _service.Reserve(Request("102", 10, 2));

        IReadOnlyList<Room> free = _service.FreeRooms(
            StayPeriod.FromNights(Today.AddDays(10), 1));

        Assert.Equal("101,201", string.Join(",", free.Select(r => r.Number)));
    }

    private static ReserveRequest Request(string room, int daysFromToday, int nights) =>
        new(1, room, StayPeriod.FromNights(Today.AddDays(daysFromToday), nights));
}
