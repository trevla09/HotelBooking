namespace HotelBooking.Core.Domain;

/// <summary>Гість готелю.</summary>
public sealed class Guest
{
    /// <summary>Ідентифікатор гостя.</summary>
    public int Id { get; init; }

    /// <summary>Повне ім'я гостя.</summary>
    public string FullName { get; init; } = string.Empty;

    /// <summary>Ознака постійного гостя.</summary>
    public bool IsRegular { get; init; }
}
