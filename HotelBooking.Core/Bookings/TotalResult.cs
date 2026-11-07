using HotelBooking.Core.Domain;

namespace HotelBooking.Core.Bookings;

/// <summary>Результат розрахунку вартості бронювання.</summary>
/// <param name="Total">Підсумкова вартість, грн.</param>
/// <param name="Status">Наступний стан бронювання.</param>
public sealed record TotalResult(
    decimal Total,
    BookingStatus Status);
