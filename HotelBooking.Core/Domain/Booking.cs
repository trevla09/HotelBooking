namespace HotelBooking.Core.Domain;

/// <summary>Бронювання номера гостем.</summary>
public sealed class Booking
{
    private readonly List<BookingNight> _nights = new();

    /// <summary>Ідентифікатор бронювання.</summary>
    public int Id { get; init; }

    /// <summary>Ідентифікатор гостя.</summary>
    public int GuestId { get; init; }

    /// <summary>Поточний стан бронювання.</summary>
    public BookingStatus Status { get; set; } = BookingStatus.New;

    /// <summary>Момент створення бронювання.</summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>Ночі, що входять до бронювання.</summary>
    public IReadOnlyList<BookingNight> Nights => _nights;

    /// <summary>Додає ніч до бронювання.</summary>
    /// <param name="night">Ніч, яку треба додати.</param>
    public void AddNight(BookingNight night) => _nights.Add(night);

    /// <summary>Обчислює вартість бронювання.</summary>
    /// <returns>Сума тарифів усіх ночей, грн.</returns>
    public decimal Total()
    {
        decimal sum = 0m;
        foreach (BookingNight night in _nights)
        {
            sum += night.Rate;
        }

        return sum;
    }
}
