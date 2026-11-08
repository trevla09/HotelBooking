using HotelBooking.Core.Abstractions;
using HotelBooking.Core.Domain;

namespace HotelBooking.Tests;

internal sealed class FakeBookingRepository : IBookingRepository
{
    private readonly Dictionary<int, Booking> _items = new();

    public int AddCalls { get; private set; }

    public void Add(Booking booking)
    {
        _items[booking.Id] = booking;
        AddCalls++;
    }

    public Booking? GetById(int id) =>
        _items.TryGetValue(id, out Booking? found) ? found : null;

    public IReadOnlyList<Booking> GetAll() => _items.Values.ToList();
}
