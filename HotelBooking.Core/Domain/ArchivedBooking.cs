namespace HotelBooking.Core.Domain;

/// <summary>Архівне бронювання. Не є бронюванням, а лише показує його дані.</summary>
public sealed class ArchivedBooking
{
    private readonly Booking _booking;

    /// <summary>Створює архівний запис бронювання.</summary>
    /// <param name="booking">Бронювання, яке архівується.</param>
    /// <param name="archivedAt">Дата архівування.</param>
    /// <exception cref="ArgumentNullException">Бронювання не задано.</exception>
    public ArchivedBooking(Booking booking, DateOnly archivedAt)
    {
        ArgumentNullException.ThrowIfNull(booking);
        _booking = booking;
        ArchivedAt = archivedAt;
    }

    /// <summary>Дата архівування.</summary>
    public DateOnly ArchivedAt { get; }

    /// <summary>Ідентифікатор бронювання.</summary>
    public int Id => _booking.Id;

    /// <summary>Стан бронювання на момент архівування.</summary>
    public BookingStatus Status => _booking.Status;

    /// <summary>Ночі бронювання лише для читання.</summary>
    public IReadOnlyList<BookingNight> Nights => _booking.Nights;
}
