using HotelBooking.Core.Domain;

namespace HotelBooking.Core.Services;

/// <summary>Критерії пошуку бронювань (UC-05). Незадані критерії не обмежують результат.</summary>
/// <param name="GuestNamePart">Частина імені гостя без урахування регістру або null.</param>
/// <param name="OnDate">Дата ночі, яку гість проводить у готелі, або null.</param>
/// <param name="Status">Стан бронювання або null.</param>
public sealed record BookingSearch(string? GuestNamePart, DateOnly? OnDate, BookingStatus? Status)
{
    /// <summary>Чи задано хоча б один критерій.</summary>
    public bool HasAnyCriterion =>
        !string.IsNullOrWhiteSpace(GuestNamePart) || OnDate.HasValue || Status.HasValue;
}
