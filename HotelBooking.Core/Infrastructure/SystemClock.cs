using HotelBooking.Core.Abstractions;

namespace HotelBooking.Core.Infrastructure;

/// <summary>Системний годинник комп'ютера рецепції.</summary>
public sealed class SystemClock : IClock
{
    /// <inheritdoc/>
    public DateTimeOffset Now => DateTimeOffset.Now;

    /// <inheritdoc/>
    public DateOnly Today => DateOnly.FromDateTime(DateTime.Now);
}
