using HotelBooking.Core.Abstractions;

namespace HotelBooking.Bench;

/// <summary>Сповіщувач, що нічого не робить: консоль не впливає на час.</summary>
internal sealed class SilentNotifier : INotifier
{
    /// <inheritdoc/>
    public void Notify(string message)
    {
    }
}
