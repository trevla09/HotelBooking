using HotelBooking.Core.Domain;

namespace HotelBooking.Core.Services;

/// <summary>Рядок результату пошуку бронювань для показу адміністратору.</summary>
/// <param name="Id">Номер бронювання.</param>
/// <param name="GuestName">Ім'я гостя.</param>
/// <param name="Rooms">Номери кімнат через кому.</param>
/// <param name="CheckIn">Дата заїзду.</param>
/// <param name="CheckOut">Дата виїзду.</param>
/// <param name="Status">Стан бронювання.</param>
public sealed record BookingView(
    int Id,
    string GuestName,
    string Rooms,
    DateOnly CheckIn,
    DateOnly CheckOut,
    BookingStatus Status);
