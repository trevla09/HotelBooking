namespace HotelBooking.Core.Domain;


public sealed class BookingNight
{
    public string RoomNumber { get; init; } = string.Empty;

    public DateOnly Date { get; init; }

    public decimal Rate { get; init; }
}