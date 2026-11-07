using System.Diagnostics;
using System.Globalization;
using HotelBooking.Core.Abstractions;
using HotelBooking.Core.Domain;
using HotelBooking.Core.Errors;
using Microsoft.Extensions.Logging;

namespace HotelBooking.Core.Services;

/// <summary>Прикладний сервіс бронювань. Залежності приходять у конструкторі.</summary>
public sealed class BookingService
{
    private readonly IBookingRepository _repository;
    private readonly IPricingPolicy _pricing;
    private readonly INotifier _notifier;
    private readonly ILogger<BookingService> _log;

    /// <summary>Створює сервіс.</summary>
    /// <param name="repository">Сховище бронювань.</param>
    /// <param name="pricing">Політика розрахунку ціни.</param>
    /// <param name="notifier">Сповіщувач.</param>
    /// <param name="log">Журнал сервісу.</param>
    /// <exception cref="ArgumentNullException">Залежність не задано.</exception>
    public BookingService(
        IBookingRepository repository,
        IPricingPolicy pricing,
        INotifier notifier,
        ILogger<BookingService> log)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _pricing = pricing ?? throw new ArgumentNullException(nameof(pricing));
        _notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
        _log = log ?? throw new ArgumentNullException(nameof(log));
    }

    /// <summary>Розміщує бронювання у сховищі й надсилає сповіщення.</summary>
    /// <param name="booking">Бронювання.</param>
    /// <exception cref="ArgumentNullException">Бронювання не задано.</exception>
    public void Place(Booking booking)
    {
        ArgumentNullException.ThrowIfNull(booking);
        _repository.Add(booking);
        _notifier.Notify(string.Format(
            CultureInfo.InvariantCulture,
            "Бронювання № {0} прийнято, сума {1:0.00} грн",
            booking.Id,
            TotalOf(booking)));
    }

    /// <summary>Додає перевірену ніч до нового бронювання.</summary>
    /// <param name="bookingId">Номер бронювання.</param>
    /// <param name="night">Ніч, уже перевірена на барикаді.</param>
    /// <exception cref="ArgumentNullException">Ніч не задано.</exception>
    /// <exception cref="DomainRuleException">
    /// Бронювання немає, воно не нове або ця ніч уже заброньована.</exception>
    public void AddNight(int bookingId, BookingNight night)
    {
        ArgumentNullException.ThrowIfNull(night);
        using IDisposable? scope = _log.BeginScope("Booking:{BookingId}", bookingId);
        Booking booking = Load(bookingId);
        if (booking.Status != BookingStatus.New)
        {
            throw new DomainRuleException(
                "booking.editable",
                $"Ночі додають лише до нового бронювання, № {bookingId}: {booking.Status}");
        }

        try
        {
            booking.AddNight(night);
        }
        catch (InvalidOperationException ex)
        {
            _log.LogWarning("Повторна ніч {Date} у номері {Room}", night.Date, night.RoomNumber);
            throw new DomainRuleException(
                "booking.night-unique",
                $"Ніч {night.Date:dd.MM.yyyy} у номері {night.RoomNumber} уже заброньована",
                ex);
        }

        _log.LogInformation("Додано ніч {Date} у номері {Room}", night.Date, night.RoomNumber);
    }

    /// <summary>Завантажує бронювання за номером.</summary>
    /// <param name="bookingId">Номер бронювання.</param>
    /// <returns>Знайдене бронювання.</returns>
    /// <exception cref="DomainRuleException">Бронювання немає.</exception>
    public Booking Load(int bookingId) =>
        _repository.GetById(bookingId)
            ?? throw new DomainRuleException(
                "booking.exists", $"Бронювання № {bookingId} не знайдено");

    /// <summary>Скасовує бронювання і сповіщає про причину.</summary>
    /// <param name="bookingId">Номер бронювання.</param>
    /// <param name="reason">Причина скасування.</param>
    /// <exception cref="DomainRuleException">
    /// Бронювання немає або його стан не дозволяє скасування.</exception>
    public void Cancel(int bookingId, string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        using IDisposable? scope = _log.BeginScope("Booking:{BookingId}", bookingId);
        Booking booking = Load(bookingId);
        try
        {
            booking.Cancel();
        }
        catch (InvalidOperationException ex)
        {
            _log.LogError(ex, "Скасування не виконано, стан {Status}", booking.Status);
            throw new DomainRuleException(
                "booking.cancellable",
                $"Бронювання № {bookingId} у стані {booking.Status} скасувати не можна",
                ex);
        }

        _log.LogInformation("Скасовано, причина {Reason}", reason);
        _notifier.Notify($"Бронювання № {bookingId} скасовано: {reason}");
    }

    /// <summary>Оплачує нове бронювання і підтверджує його.</summary>
    /// <param name="bookingId">Номер бронювання.</param>
    /// <param name="amount">Сплачена сума, грн.</param>
    /// <exception cref="DomainRuleException">
    /// Бронювання немає, воно не нове або сума менша за вартість.</exception>
    public void Pay(int bookingId, decimal amount)
    {
        using IDisposable? scope = _log.BeginScope("Booking:{BookingId}", bookingId);
        _log.LogDebug("Початок оплати, сума {Amount}", amount);
        Booking booking = Load(bookingId);
        if (booking.Status != BookingStatus.New)
        {
            throw new DomainRuleException(
                "booking.payable", $"Стан {booking.Status} не дозволяє оплату");
        }

        decimal total = TotalOf(booking);
        if (amount < total)
        {
            throw new DomainRuleException(
                "booking.amount", $"Сплачено {amount:0.00} грн, треба {total:0.00} грн");
        }

        if (amount > total)
        {
            _log.LogWarning("Переплата {Extra} грн", amount - total);
        }

        booking.Confirm();
        _log.LogInformation("Оплачено на {Amount} грн", amount);
    }

    /// <summary>БУЛО: у разі винятку файл лишається відкритим.</summary>
    /// <param name="path">Шлях до файлу звіту.</param>
    /// <param name="booking">Бронювання.</param>
    public void ExportReport(string path, Booking booking)
    {
        ArgumentNullException.ThrowIfNull(booking);
        var writer = new StreamWriter(path);
        writer.WriteLine($"Бронювання {booking.Id}");
        writer.WriteLine(TotalOf(booking));
        writer.Close();
    }

    /// <summary>Обчислює вартість бронювання за політикою. Не змінює стан.</summary>
    /// <param name="booking">Бронювання.</param>
    /// <returns>Вартість, грн.</returns>
    /// <exception cref="ArgumentNullException">Бронювання не задано.</exception>
    public decimal TotalOf(Booking booking)
    {
        ArgumentNullException.ThrowIfNull(booking);
        decimal total = 0m;
        foreach (BookingNight night in booking.Nights)
        {
            total += _pricing.PriceOf(night);
        }

        Debug.Assert(total >= 0m, "Вартість бронювання не може бути від'ємною (CALC-01)");
        return total;
    }
}
