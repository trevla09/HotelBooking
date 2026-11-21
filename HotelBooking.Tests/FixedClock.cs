using HotelBooking.Core.Abstractions;

namespace HotelBooking.Tests;

internal sealed class FixedClock : IClock
{
    public FixedClock(DateOnly today)
    {
        Now = new DateTimeOffset(today.ToDateTime(new TimeOnly(10, 0)), TimeSpan.FromHours(2));
    }

    public DateTimeOffset Now { get; }

    public DateOnly Today => DateOnly.FromDateTime(Now.DateTime);
}
