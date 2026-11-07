using HotelBooking.Core.Abstractions;
using HotelBooking.Core.Domain;

namespace HotelBooking.Core.Storage;

/// <summary>Сховище бронювань у пам'яті на словнику.</summary>
public sealed class InMemoryBookingRepository : IBookingRepository
{
    private readonly Dictionary<int, Booking> _bookings = new();

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">Бронювання не задано.</exception>
    /// <exception cref="InvalidOperationException">Бронювання з таким Id уже є.</exception>
    public void Add(Booking booking)
    {
        ArgumentNullException.ThrowIfNull(booking);
        if (!_bookings.TryAdd(booking.Id, booking))
        {
            throw new InvalidOperationException(
                $"Бронювання № {booking.Id} уже є у сховищі.");
        }
    }

    /// <inheritdoc/>
    public Booking? GetById(int id) =>
        _bookings.TryGetValue(id, out Booking? booking) ? booking : null;

    /// <inheritdoc/>
    public IReadOnlyList<Booking> GetAll() => _bookings.Values.ToList();
}
