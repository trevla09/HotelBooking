namespace HotelBooking.Core.Domain;

/// <summary>Номер у фонді готелю.</summary>
public sealed class Room
{
    public string Number { get; init; } = string.Empty;

    public string Category { get; init; } = string.Empty;

    public int Capacity { get; init; }

    public decimal BaseRate { get; init; }
}
