using HotelBooking.Core.Abstractions;

namespace HotelBooking.App;

/// <summary>Сповіщувач, що пише в консоль (замість справжньої пошти).</summary>
internal sealed class ConsoleNotifier : INotifier
{
    /// <inheritdoc/>
    public void Notify(string message) => Console.WriteLine(message);
}
