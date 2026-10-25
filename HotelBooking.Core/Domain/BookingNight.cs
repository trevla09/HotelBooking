namespace HotelBooking.Core.Domain;

/// <summary>Одна ніч проживання: номер, дата і тариф за цю ніч.</summary>
public sealed class BookingNight
{
    /// <summary>Номер кімнати на цю ніч.</summary>
    public string RoomNumber { get; init; } = string.Empty;

    /// <summary>Дата ночі проживання.</summary>
    public DateOnly Date { get; init; }

    /// <summary>Тариф за цю ніч, грн.</summary>
    public decimal Rate { get; init; }
}
