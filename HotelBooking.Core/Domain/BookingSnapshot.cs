namespace HotelBooking.Core.Domain;

/// <summary>Знімок стану бронювання для збереження у файл і відновлення.</summary>
/// <param name="Id">Ідентифікатор бронювання.</param>
/// <param name="GuestId">Ідентифікатор гостя.</param>
/// <param name="CreatedAt">Момент створення.</param>
/// <param name="Status">Стан на момент знімка.</param>
/// <param name="Nights">Ночі бронювання.</param>
public sealed record BookingSnapshot(
    int Id,
    int GuestId,
    DateTimeOffset CreatedAt,
    BookingStatus Status,
    IReadOnlyList<BookingNight> Nights);
