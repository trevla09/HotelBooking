using HotelBooking.Core.Domain;

namespace HotelBooking.Core.Abstractions;

/// <summary>Сховище бронювань. Відповідає лише за зберігання.</summary>
public interface IBookingRepository
{
    /// <summary>Додає бронювання до сховища.</summary>
    /// <param name="booking">Бронювання.</param>
    void Add(Booking booking);

    /// <summary>Шукає бронювання за ідентифікатором.</summary>
    /// <param name="id">Ідентифікатор бронювання.</param>
    /// <returns>Бронювання або null, якщо його немає.</returns>
    Booking? GetById(int id);

    /// <summary>Повертає всі бронювання.</summary>
    /// <returns>Знімок усіх бронювань.</returns>
    IReadOnlyList<Booking> GetAll();
}
