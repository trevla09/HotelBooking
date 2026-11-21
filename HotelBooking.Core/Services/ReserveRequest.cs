using HotelBooking.Core.Domain;

namespace HotelBooking.Core.Services;

/// <summary>Перевірена на барикаді заявка на бронювання (UC-01).</summary>
/// <param name="GuestId">Ідентифікатор гостя з картотеки.</param>
/// <param name="RoomNumber">Номер кімнати.</param>
/// <param name="Period">Період проживання [заїзд; виїзд).</param>
public sealed record ReserveRequest(int GuestId, string RoomNumber, StayPeriod Period);
