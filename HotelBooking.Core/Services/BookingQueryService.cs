using HotelBooking.Core.Abstractions;
using HotelBooking.Core.Domain;
using HotelBooking.Core.Errors;
using HotelBooking.Core.Reporting;

namespace HotelBooking.Core.Services;

/// <summary>Запити без зміни стану: пошук бронювань (UC-05) і звіт (UC-06).</summary>
public sealed class BookingQueryService
{
    /// <summary>Кількість днів у звіті заповнюваності.</summary>
    public const int DaysInWeek = 7;

    private readonly IBookingRepository _repository;
    private readonly IHotelDirectory _directory;

    /// <summary>Створює сервіс запитів.</summary>
    /// <param name="repository">Сховище бронювань.</param>
    /// <param name="directory">Довідники номерів і гостей.</param>
    /// <exception cref="ArgumentNullException">Залежність не задано.</exception>
    public BookingQueryService(IBookingRepository repository, IHotelDirectory directory)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(directory);
        _repository = repository;
        _directory = directory;
    }

    /// <summary>Шукає бронювання, що відповідають усім заданим критеріям.</summary>
    /// <param name="criteria">Критерії; хоча б один має бути заданий.</param>
    /// <returns>Рядки, упорядковані за датою заїзду, потім за номером бронювання.</returns>
    /// <exception cref="ArgumentNullException">Критерії не задано.</exception>
    /// <exception cref="ArgumentException">Жодного критерію не задано.</exception>
    public IReadOnlyList<BookingView> Search(BookingSearch criteria)
    {
        ArgumentNullException.ThrowIfNull(criteria);
        if (!criteria.HasAnyCriterion)
        {
            throw new ArgumentException("Потрібен хоча б один критерій пошуку.", nameof(criteria));
        }

        return _repository.GetAll()
            .Where(b => b.Nights.Count > 0 && Matches(b, criteria))
            .Select(ToView)
            .OrderBy(v => v.CheckIn)
            .ThenBy(v => v.Id)
            .ToList();
    }

    /// <summary>Будує звіт заповнюваності на сім днів від першої дати.</summary>
    /// <param name="firstDay">Перша дата тижня.</param>
    /// <returns>Звіт із сімома рядками.</returns>
    /// <exception cref="DomainRuleException">У фонді немає жодного номера.</exception>
    public OccupancyReport WeeklyOccupancy(DateOnly firstDay)
    {
        int totalRooms = _directory.Rooms.Count;
        if (totalRooms == 0)
        {
            throw new DomainRuleException("hotel.rooms", "У фонді немає номерів");
        }

        List<BookingNight> nights = _repository.GetAll()
            .Where(b => b.IsActive)
            .SelectMany(b => b.Nights)
            .ToList();
        List<OccupancyDay> days = Enumerable.Range(0, DaysInWeek)
            .Select(offset => firstDay.AddDays(offset))
            .Select(day => new OccupancyDay(day, OccupiedOn(nights, day), totalRooms))
            .ToList();
        return new OccupancyReport(firstDay, days);
    }

    private static int OccupiedOn(List<BookingNight> nights, DateOnly day) =>
        nights.Where(n => n.Date == day).Select(n => n.RoomNumber).Distinct().Count();

    private bool Matches(Booking booking, BookingSearch criteria) =>
        (string.IsNullOrWhiteSpace(criteria.GuestNamePart)
            || GuestNameOf(booking).Contains(
                criteria.GuestNamePart.Trim(), StringComparison.OrdinalIgnoreCase))
        && (criteria.OnDate is null || booking.Nights.Any(n => n.Date == criteria.OnDate))
        && (criteria.Status is null || booking.Status == criteria.Status);

    private BookingView ToView(Booking booking) => new(
        booking.Id,
        GuestNameOf(booking),
        string.Join(", ", booking.Nights.Select(n => n.RoomNumber).Distinct()),
        booking.Nights.Min(n => n.Date),
        booking.Nights.Max(n => n.Date).AddDays(1),
        booking.Status);

    private string GuestNameOf(Booking booking) =>
        _directory.FindGuest(booking.GuestId)?.FullName ?? $"гість № {booking.GuestId}";
}
