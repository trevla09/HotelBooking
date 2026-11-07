namespace HotelBooking.Core.Legacy;

/// <summary>Рядок бронювання: номер, кількість ночей і тариф за ніч.</summary>
public sealed class BookingLine
{
    /// <summary>Номер кімнати.</summary>
    public string RoomNumber { get; set; } = string.Empty;

    /// <summary>Кількість ночей.</summary>
    public int Nights { get; set; }

    /// <summary>Тариф за одну ніч, грн.</summary>
    public decimal NightlyRate { get; set; }
}
