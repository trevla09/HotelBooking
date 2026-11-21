using HotelBooking.Core.Domain;
using HotelBooking.Core.Errors;
using HotelBooking.Core.Reporting;
using HotelBooking.Core.Services;
using Xunit;

namespace HotelBooking.Tests;

public class BookingQueryServiceTests
{
    private readonly FakeBookingRepository _repository = new();
    private readonly FakeHotelDirectory _directory = FakeHotelDirectory.WithThreeRooms();
    private readonly BookingQueryService _queries;

    public BookingQueryServiceTests()
    {
        _queries = new BookingQueryService(_repository, _directory);
    }

    [Fact]
    public void Search_NoMatches_ReturnsEmpty()
    {
        Add(1, guestId: 1, room: "101", day: 10, nights: 2);

        IReadOnlyList<BookingView> found = _queries.Search(
            new BookingSearch("Шевчук", null, null));

        Assert.Empty(found);
    }

    [Fact]
    public void Search_ByNameAndStatus_ReturnsOne()
    {
        Add(1, guestId: 1, room: "101", day: 10, nights: 2);
        Booking confirmed = Add(2, guestId: 3, room: "102", day: 10, nights: 2);
        confirmed.Confirm();
        Add(3, guestId: 3, room: "201", day: 20, nights: 1);

        BookingView view = Assert.Single(_queries.Search(
            new BookingSearch("коваль", null, BookingStatus.Confirmed)));

        Assert.Equal(2, view.Id);
        Assert.Equal("Олег Ковальчук", view.GuestName);
        Assert.Equal(new DateOnly(2026, 3, 12), view.CheckOut);
    }

    [Fact]
    public void Search_ByDate_OrderedByCheckInThenId()
    {
        Add(3, guestId: 1, room: "201", day: 11, nights: 3);
        Add(2, guestId: 2, room: "102", day: 9, nights: 5);
        Add(1, guestId: 3, room: "101", day: 11, nights: 1);
        Add(4, guestId: 1, room: "101", day: 15, nights: 1);

        IReadOnlyList<BookingView> found = _queries.Search(
            new BookingSearch(null, new DateOnly(2026, 3, 11), null));

        Assert.Equal("2,1,3", string.Join(",", found.Select(v => v.Id)));
    }

    [Fact]
    public void Search_NoCriteria_ThrowsArgument()
    {
        Assert.Throws<ArgumentException>(
            () => _queries.Search(new BookingSearch(" ", null, null)));
    }

    [Fact]
    public void WeeklyOccupancy_NoBookings_AllZero()
    {
        OccupancyReport report = _queries.WeeklyOccupancy(new DateOnly(2026, 3, 9));

        Assert.Equal(BookingQueryService.DaysInWeek, report.Days.Count);
        Assert.All(report.Days, d => Assert.Equal(0m, d.Percent));
        Assert.Equal(0m, report.AveragePercent);
    }

    [Fact]
    public void WeeklyOccupancy_ThreeNightStay_CountsOnlyNights()
    {
        Add(1, guestId: 1, room: "101", day: 10, nights: 3);

        OccupancyReport report = _queries.WeeklyOccupancy(new DateOnly(2026, 3, 9));

        Assert.Equal("0,1,1,1,0,0,0", string.Join(",", report.Days.Select(d => d.OccupiedRooms)));
        Assert.Equal(33.3m, report.Days[1].Percent);
        Assert.Equal(14.3m, report.AveragePercent);
    }

    [Fact]
    public void WeeklyOccupancy_CancelledBooking_NotCounted()
    {
        Booking cancelled = Add(1, guestId: 1, room: "101", day: 10, nights: 2);
        cancelled.Cancel();
        Add(2, guestId: 2, room: "102", day: 10, nights: 1);

        OccupancyReport report = _queries.WeeklyOccupancy(new DateOnly(2026, 3, 10));

        Assert.Equal(1, report.Days[0].OccupiedRooms);
        Assert.Equal(0, report.Days[1].OccupiedRooms);
    }

    [Fact]
    public void WeeklyOccupancy_EmptyHotel_ThrowsHotelRooms()
    {
        BookingQueryService queries = new(_repository, new FakeHotelDirectory());

        DomainRuleException ex = Assert.Throws<DomainRuleException>(
            () => queries.WeeklyOccupancy(new DateOnly(2026, 3, 9)));

        Assert.Equal("hotel.rooms", ex.Rule);
    }

    private Booking Add(int id, int guestId, string room, int day, int nights)
    {
        Booking booking = new(id, guestId, DateTimeOffset.UnixEpoch);
        StayPeriod period = StayPeriod.FromNights(new DateOnly(2026, 3, day), nights);
        foreach (DateOnly date in period.EachNight())
        {
            booking.AddNight(new BookingNight(room, date, 1000m));
        }

        _repository.Add(booking);
        return booking;
    }
}
