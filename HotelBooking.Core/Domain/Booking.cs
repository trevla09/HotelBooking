namespace HotelBooking.Core.Domain;

/// <summary>Бронювання номера гостем. Агрегат: стан змінюють лише методи-переходи.</summary>
public sealed class Booking
{
    private readonly List<BookingNight> _nights = new();

    /// <summary>Створює нове бронювання у стані <see cref="BookingStatus.New"/>.</summary>
    /// <param name="id">Ідентифікатор бронювання, більший за нуль.</param>
    /// <param name="guestId">Ідентифікатор гостя, більший за нуль.</param>
    /// <param name="createdAt">Момент створення бронювання.</param>
    /// <exception cref="ArgumentOutOfRangeException">Ідентифікатор не більший за нуль.</exception>
    public Booking(int id, int guestId, DateTimeOffset createdAt)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(guestId);

        Id = id;
        GuestId = guestId;
        CreatedAt = createdAt;
        Status = BookingStatus.New;
    }

    /// <summary>Ідентифікатор бронювання.</summary>
    public int Id { get; }

    /// <summary>Ідентифікатор гостя.</summary>
    public int GuestId { get; }

    /// <summary>Момент створення бронювання.</summary>
    public DateTimeOffset CreatedAt { get; }

    /// <summary>Поточний стан бронювання.</summary>
    public BookingStatus Status { get; private set; }

    /// <summary>Ночі, що входять до бронювання. Додати ніч можна лише через AddNight.</summary>
    public IReadOnlyList<BookingNight> Nights => _nights;

    /// <summary>Додає ніч до нового бронювання.</summary>
    /// <param name="night">Ніч, яку треба додати.</param>
    /// <exception cref="ArgumentNullException">Ніч не задано.</exception>
    /// <exception cref="InvalidOperationException">
    /// Бронювання не нове або ця ніч у цьому номері вже є.</exception>
    public void AddNight(BookingNight night)
    {
        ArgumentNullException.ThrowIfNull(night);
        EnsureStatus(BookingStatus.New, "Ночі додають лише до нового бронювання.");
        if (_nights.Exists(n => n.Date == night.Date && n.RoomNumber == night.RoomNumber))
        {
            throw new InvalidOperationException("Ця ніч у цьому номері вже заброньована.");
        }

        _nights.Add(night);
    }

    /// <summary>Підтверджує бронювання: New → Confirmed.</summary>
    /// <exception cref="InvalidOperationException">Перехід заборонено або ночей немає.</exception>
    public void Confirm()
    {
        EnsureStatus(BookingStatus.New, "Підтвердити можна лише нове бронювання.");
        if (_nights.Count == 0)
        {
            throw new InvalidOperationException("Бронювання не містить жодної ночі.");
        }

        Status = BookingStatus.Confirmed;
    }

    /// <summary>Заселяє гостя: Confirmed → CheckedIn.</summary>
    /// <exception cref="InvalidOperationException">Перехід заборонено.</exception>
    public void CheckIn()
    {
        EnsureStatus(BookingStatus.Confirmed, "Заселити можна лише підтверджене бронювання.");
        Status = BookingStatus.CheckedIn;
    }

    /// <summary>Виселяє гостя: CheckedIn → CheckedOut.</summary>
    /// <exception cref="InvalidOperationException">Перехід заборонено.</exception>
    public void CheckOut()
    {
        EnsureStatus(BookingStatus.CheckedIn, "Виселити можна лише заселеного гостя.");
        Status = BookingStatus.CheckedOut;
    }

    /// <summary>Скасовує бронювання: New або Confirmed → Cancelled.</summary>
    /// <exception cref="InvalidOperationException">Перехід заборонено.</exception>
    public void Cancel()
    {
        if (Status is not (BookingStatus.New or BookingStatus.Confirmed))
        {
            throw new InvalidOperationException(
                "Скасувати можна лише нове або підтверджене бронювання.");
        }

        Status = BookingStatus.Cancelled;
    }

    /// <summary>Обчислює базову вартість бронювання без знижок. Не змінює стан.</summary>
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

    private void EnsureStatus(BookingStatus expected, string message)
    {
        if (Status != expected)
        {
            throw new InvalidOperationException(message);
        }
    }
}
