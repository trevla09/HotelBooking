using HotelBooking.Core.Domain;

namespace HotelBooking.Core.Bookings;

/// <summary>Запит на розрахунок вартості бронювання.</summary>
/// <param name="BookingId">Номер бронювання.</param>
/// <param name="Guest">Гість, який оплачує бронювання.</param>
/// <param name="Lines">Рядки бронювання.</param>
/// <param name="CreatedAt">Дата створення бронювання.</param>
/// <param name="CouponCode">Код купона або null.</param>
/// <param name="Status">Поточний стан бронювання.</param>
/// <param name="TransferPrice">Вартість трансферу, грн.</param>
public sealed record TotalRequest(
    int BookingId,
    GuestInfo Guest,
    IReadOnlyList<BookingLine> Lines,
    DateOnly CreatedAt,
    string? CouponCode,
    BookingStatus Status,
    decimal TransferPrice);
