using HotelBooking.Core.Abstractions;

namespace HotelBooking.Tests;

internal sealed class SpyNotifier : INotifier
{
    private readonly List<string> _messages = new();

    public IReadOnlyList<string> Messages => _messages;

    public void Notify(string message) => _messages.Add(message);
}
