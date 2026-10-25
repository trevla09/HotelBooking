namespace HotelBooking.Core.Domain;


public sealed class Room
{
    public string Number { get; init; } = string.Empty;

    public string Category { get; init; } = string.Empty;

    public int Capacity { get; init; }

    public decimal BaseRate { get; init; }
}
