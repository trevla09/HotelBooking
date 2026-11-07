using System.Diagnostics;
using System.Globalization;
using HotelBooking.Core.Abstractions;
using HotelBooking.Core.Domain;
using HotelBooking.Core.Errors;

namespace HotelBooking.Core.Services;

/// <summary>Прикладний сервіс бронювань. Залежності приходять у конструкторі.</summary>
public sealed partial class BookingService
{
    private readonly IBookingRepository _repository;
    private readonly IPricingPolicy _pricing;
    private readonly INotifier _notifier;

    /// <summary>Створює сервіс.</summary>
    /// <param name="repository">Сховище бронювань.</param>
    /// <param name="pricing">Політика розрахунку ціни.</param>
    /// <param name="notifier">Сповіщувач.</param>
    /// <exception cref="ArgumentNullException">Залежність не задано.</exception>
    public BookingService(
        IBookingRepository repository,
        IPricingPolicy pricing,
        INotifier notifier)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _pricing = pricing ?? throw new ArgumentNullException(nameof(pricing));
        _notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
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
        Booking booking = _repository.GetById(bookingId)
            ?? throw new DomainRuleException(
                "booking.exists", $"Бронювання № {bookingId} не знайдено");
        if (booking.Status != BookingStatus.New)
        {
            throw new DomainRuleException(
                "booking.editable",
                $"Ночі додають лише до нового бронювання, а № {bookingId} у стані {booking.Status}");
        }

        try
        {
            booking.AddNight(night);
        }
        catch (InvalidOperationException ex)
        {
            throw new DomainRuleException(
                "booking.night-unique",
                $"Ніч {night.Date:dd.MM.yyyy} у номері {night.RoomNumber} уже заброньована",
                ex);
        }
    }

    /// <summary>Оплачує нове бронювання і підтверджує його.</summary>
    /// <param name="bookingId">Номер бронювання.</param>
    /// <param name="amount">Сплачена сума, грн.</param>
    /// <exception cref="DomainRuleException">
    /// Бронювання немає, воно не нове або сума менша за вартість.</exception>
    public void Pay(int bookingId, decimal amount)
    {
        Booking booking = _repository.GetById(bookingId)
            ?? throw new DomainRuleException(
                "booking.exists", $"Бронювання № {bookingId} не знайдено");
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

        booking.Confirm();
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
