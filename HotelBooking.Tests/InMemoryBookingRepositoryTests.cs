using HotelBooking.Core.Domain;
using HotelBooking.Core.Storage;
using Xunit;

namespace HotelBooking.Tests;

public class InMemoryBookingRepositoryTests
{
    [Fact]
    public void Add_SameIdTwice_ThrowsInvalidOperation()
    {
        InMemoryBookingRepository repository = new();
        repository.Add(NewBooking(1));

        Assert.Throws<InvalidOperationException>(() => repository.Add(NewBooking(1)));
    }

    [Fact]
    public void GetById_UnknownId_ReturnsNull()
    {
        InMemoryBookingRepository repository = new();
        repository.Add(NewBooking(1));

        Booking? actual = repository.GetById(2);

        Assert.Null(actual);
    }

    [Fact]
    public void GetAll_TwoAdded_ReturnsBoth()
    {
        InMemoryBookingRepository repository = new();
        repository.Add(NewBooking(1));
        repository.Add(NewBooking(2));

        IReadOnlyList<Booking> all = repository.GetAll();

        Assert.Equal(2, all.Count);
        Assert.NotNull(repository.GetById(2));
    }

    private static Booking NewBooking(int id) =>
        new(id, 100, new DateTimeOffset(2026, 11, 1, 12, 0, 0, TimeSpan.Zero));
}
