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

    /// <summary>Чи займає бронювання номери: будь-який стан, крім скасованого.</summary>
    public bool IsActive => Status != BookingStatus.Cancelled;

    /// <summary>Відновлює бронювання зі знімка стану. Використовується сховищем.</summary>
    /// <param name="snapshot">Збережений знімок.</param>
    /// <returns>Бронювання з тими самими ночами і станом.</returns>
    /// <exception cref="ArgumentNullException">Знімок не задано.</exception>
    public static Booking Restore(BookingSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        Booking booking = new(snapshot.Id, snapshot.GuestId, snapshot.CreatedAt);
        foreach (BookingNight night in snapshot.Nights)
        {
            booking.AddNight(night);
        }

        booking.Status = snapshot.Status;
        return booking;
    }

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

    /// <summary>Перевіряє, чи займає бронювання номер хоча б на одну ніч періоду.</summary>
    /// <param name="roomNumber">Номер кімнати.</param>
    /// <param name="period">Період [заїзд; виїзд).</param>
    /// <returns>true, якщо бронювання не скасоване і має ніч у цьому номері в межах періоду.</returns>
    /// <exception cref="ArgumentNullException">Період не задано.</exception>
    public bool Occupies(string roomNumber, StayPeriod period)
    {
        ArgumentNullException.ThrowIfNull(period);
        return IsActive
            && _nights.Exists(n => n.RoomNumber == roomNumber && period.Contains(n.Date));
    }

    /// <summary>Створює знімок стану для збереження. Не змінює стан.</summary>
    /// <returns>Знімок з копією списку ночей.</returns>
    public BookingSnapshot ToSnapshot() =>
        new(Id, GuestId, CreatedAt, Status, _nights.ToList());

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
