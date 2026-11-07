using HotelBooking.Core.Abstractions;
using HotelBooking.Core.Domain;

namespace HotelBooking.Core.Services;

/// <summary>Прикладний сервіс бронювань. Залежності приходять у конструкторі.</summary>
public sealed class BookingService
{
    private readonly IBookingRepository _repository;
    private readonly IPricingPolicy _pricing;

    /// <summary>Створює сервіс.</summary>
    /// <param name="repository">Сховище бронювань.</param>
    /// <param name="pricing">Політика розрахунку ціни.</param>
    /// <exception cref="ArgumentNullException">Залежність не задано.</exception>
    public BookingService(IBookingRepository repository, IPricingPolicy pricing)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _pricing = pricing ?? throw new ArgumentNullException(nameof(pricing));
    }

    /// <summary>Розміщує бронювання у сховищі.</summary>
    /// <param name="booking">Бронювання.</param>
    /// <exception cref="ArgumentNullException">Бронювання не задано.</exception>
    public void Place(Booking booking)
    {
        ArgumentNullException.ThrowIfNull(booking);
        _repository.Add(booking);
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
