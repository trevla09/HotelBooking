using HotelBooking.Core.Services;

namespace HotelBooking.App;

/// <summary>Сервіси, якими користується меню. Збирає їх композиційний корінь.</summary>
/// <param name="Reservations">UC-01: пошук вільних номерів і бронювання.</param>
/// <param name="Bookings">UC-02..UC-04: оплата, заселення, скасування, виселення.</param>
/// <param name="Queries">UC-05 і UC-06: пошук і звіт.</param>
internal sealed record MenuServices(
    ReservationService Reservations,
    BookingService Bookings,
    BookingQueryService Queries);
