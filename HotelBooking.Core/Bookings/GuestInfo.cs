namespace HotelBooking.Core.Bookings;

/// <summary>Дані гостя, який оплачує бронювання.</summary>
/// <param name="Name">Ім'я гостя.</param>
/// <param name="Email">Пошта гостя.</param>
/// <param name="IsRegular">Чи є гість постійним.</param>
public sealed record GuestInfo(
    string Name,
    string Email,
    bool IsRegular);
