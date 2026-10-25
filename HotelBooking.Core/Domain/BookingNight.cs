namespace HotelBooking.Core.Domain;

/// <summary>Одна ніч проживання: номер, дата і тариф за цю ніч.</summary>
public sealed class BookingNight
{
    public string RoomNumber { get; init; } = string.Empty;

    public DateOnly Date { get; init; }

    public decimal Rate { get; init; }
}
