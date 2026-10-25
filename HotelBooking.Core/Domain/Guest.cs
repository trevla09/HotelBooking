namespace HotelBooking.Core.Domain;

/// <summary>Гість готелю.</summary>
public sealed class Guest
{
    public int Id { get; init; }

    public string FullName { get; init; } = string.Empty;

    public bool IsRegular { get; init; }
}
