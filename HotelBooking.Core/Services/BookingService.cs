using System.Globalization;
using HotelBooking.Core.Abstractions;
using HotelBooking.Core.Domain;

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

        return total;
    }
}
