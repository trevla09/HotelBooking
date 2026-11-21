using HotelBooking.Core.Abstractions;
using HotelBooking.Core.Domain;
using HotelBooking.Core.Errors;
using Microsoft.Extensions.Logging;

namespace HotelBooking.Core.Services;

/// <summary>Сценарій UC-01 «Забронювати номер»: перевірка вільного номера і створення бронювання.</summary>
public sealed class ReservationService
{
    /// <summary>Найбільша кількість ночей в одному бронюванні.</summary>
    public const int MaxNights = 30;

    private readonly IBookingRepository _repository;
    private readonly IHotelDirectory _directory;
    private readonly IClock _clock;
    private readonly ILogger<ReservationService> _log;

    /// <summary>Створює сервіс бронювання.</summary>
    /// <param name="repository">Сховище бронювань.</param>
    /// <param name="directory">Довідники номерів і гостей.</param>
    /// <param name="clock">Годинник, що задає «сьогодні».</param>
    /// <param name="log">Журнал сервісу.</param>
    /// <exception cref="ArgumentNullException">Залежність не задано.</exception>
    public ReservationService(
        IBookingRepository repository,
        IHotelDirectory directory,
        IClock clock,
        ILogger<ReservationService> log)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(directory);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(log);
        _repository = repository;
        _directory = directory;
        _clock = clock;
        _log = log;
    }

    /// <summary>Бронює номер на період і зберігає нове бронювання у стані New.</summary>
    /// <param name="request">Заявка, перевірена на барикаді.</param>
    /// <returns>Створене бронювання з однією ніччю на кожну дату періоду.</returns>
    /// <exception cref="ArgumentNullException">Заявку не задано.</exception>
    /// <exception cref="DomainRuleException">
    /// Немає гостя чи номера, дата в минулому, забагато ночей або номер зайнятий.</exception>
    public Booking Reserve(ReserveRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        using IDisposable? scope = _log.BeginScope("UC-01 Room:{Room}", request.RoomNumber);
        _log.LogInformation(
            "UC-01 початок: {CheckIn:dd.MM.yyyy}, ночей {Nights}",
            request.Period.CheckIn,
            request.Period.Nights);
        Room room = _directory.FindRoom(request.RoomNumber)
            ?? throw Reject("room.exists", $"Номера {request.RoomNumber} немає у фонді");
        if (_directory.FindGuest(request.GuestId) is null)
        {
            throw Reject("guest.exists", $"Гостя № {request.GuestId} немає в картотеці");
        }

        EnsurePeriodAllowed(request.Period);
        if (!IsFree(room.Number, request.Period))
        {
            throw Reject("room.busy", $"Номер {room.Number} зайнятий у цей період");
        }

        Booking booking = CreateBooking(request.GuestId, room, request.Period);
        _log.LogInformation("UC-01 завершено: бронювання {BookingId}", booking.Id);
        return booking;
    }

    /// <summary>Перевіряє, чи вільний номер на кожну ніч періоду.</summary>
    /// <param name="roomNumber">Номер кімнати.</param>
    /// <param name="period">Період [заїзд; виїзд).</param>
    /// <returns>true, якщо жодне нескасоване бронювання не займає номер у ці ночі.</returns>
    /// <exception cref="ArgumentNullException">Період не задано.</exception>
    public bool IsFree(string roomNumber, StayPeriod period)
    {
        ArgumentNullException.ThrowIfNull(period);
        return !_repository.GetAll().Any(b => b.Occupies(roomNumber, period));
    }

    /// <summary>Добирає номери, вільні на весь період (альтернатива 4а сценарію UC-01).</summary>
    /// <param name="period">Період [заїзд; виїзд).</param>
    /// <returns>Вільні номери, упорядковані за номером кімнати.</returns>
    /// <exception cref="ArgumentNullException">Період не задано.</exception>
    public IReadOnlyList<Room> FreeRooms(StayPeriod period)
    {
        ArgumentNullException.ThrowIfNull(period);
        return _directory.Rooms.Where(r => IsFree(r.Number, period)).ToList();
    }

    private void EnsurePeriodAllowed(StayPeriod period)
    {
        if (period.CheckIn < _clock.Today)
        {
            throw Reject("stay.past", $"Дата заїзду {period.CheckIn:dd.MM.yyyy} вже минула");
        }

        if (period.Nights > MaxNights)
        {
            throw Reject("stay.too-long", $"Ночей {period.Nights}, найбільше {MaxNights}");
        }
    }

    private Booking CreateBooking(int guestId, Room room, StayPeriod period)
    {
        Booking booking = new(_repository.NextId(), guestId, _clock.Now);
        foreach (DateOnly date in period.EachNight())
        {
            booking.AddNight(new BookingNight(room.Number, date, room.BaseRate));
        }

        _repository.Add(booking);
        _repository.Save();
        return booking;
    }

    private DomainRuleException Reject(string rule, string message)
    {
        _log.LogWarning("UC-01 відхилено, правило {Rule}: {Message}", rule, message);
        return new DomainRuleException(rule, message);
    }
}
