namespace HotelBooking.Core.Domain;

/// <summary>Одна ніч проживання: номер, дата і тариф. Незмінна.</summary>
public sealed record BookingNight
{
    /// <summary>Створює ніч проживання.</summary>
    /// <param name="roomNumber">Номер кімнати, не порожній.</param>
    /// <param name="date">Дата ночі (вечір заселення).</param>
    /// <param name="rate">Тариф за ніч, грн, не від'ємний.</param>
    /// <exception cref="ArgumentException">Номер кімнати порожній.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Тариф від'ємний.</exception>
    public BookingNight(string roomNumber, DateOnly date, decimal rate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roomNumber);
        ArgumentOutOfRangeException.ThrowIfNegative(rate);

        RoomNumber = roomNumber;
        Date = date;
        Rate = rate;
    }

    /// <summary>Номер кімнати на цю ніч.</summary>
    public string RoomNumber { get; }

    /// <summary>Дата ночі проживання.</summary>
    public DateOnly Date { get; }

    /// <summary>Тариф за цю ніч, грн.</summary>
    public decimal Rate { get; }

    /// <summary>Дата виїзду після цієї ночі (наступний день).</summary>
    public DateOnly CheckOutDate => Date.AddDays(1);
}
