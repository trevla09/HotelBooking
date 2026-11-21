namespace HotelBooking.Core.Domain;

/// <summary>
/// Період проживання - напіввідкритий інтервал дат [заїзд; виїзд).
/// День виїзду до періоду не входить, тому номер вільний для наступного гостя того ж дня.
/// </summary>
public sealed record StayPeriod
{
    /// <summary>Створює період проживання.</summary>
    /// <param name="checkIn">Дата заїзду, перша ніч періоду.</param>
    /// <param name="checkOut">Дата виїзду, пізніша за дату заїзду.</param>
    /// <exception cref="ArgumentException">Дата виїзду не пізніша за дату заїзду.</exception>
    public StayPeriod(DateOnly checkIn, DateOnly checkOut)
    {
        if (checkOut <= checkIn)
        {
            throw new ArgumentException(
                "Дата виїзду має бути пізнішою за дату заїзду.", nameof(checkOut));
        }

        CheckIn = checkIn;
        CheckOut = checkOut;
    }

    /// <summary>Дата заїзду; входить до періоду.</summary>
    public DateOnly CheckIn { get; }

    /// <summary>Дата виїзду; до періоду не входить.</summary>
    public DateOnly CheckOut { get; }

    /// <summary>Кількість ночей у періоді, завжди більша за нуль.</summary>
    public int Nights => CheckOut.DayNumber - CheckIn.DayNumber;

    /// <summary>Створює період із дати заїзду і кількості ночей.</summary>
    /// <param name="checkIn">Дата заїзду.</param>
    /// <param name="nights">Кількість ночей, більша за нуль.</param>
    /// <returns>Період [checkIn; checkIn + nights).</returns>
    /// <exception cref="ArgumentOutOfRangeException">Кількість ночей не більша за нуль.</exception>
    public static StayPeriod FromNights(DateOnly checkIn, int nights)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(nights);
        return new StayPeriod(checkIn, checkIn.AddDays(nights));
    }

    /// <summary>Перевіряє, чи ніч з цією датою входить до періоду.</summary>
    /// <param name="date">Дата ночі.</param>
    /// <returns>true, якщо заїзд ≤ дата &lt; виїзд.</returns>
    public bool Contains(DateOnly date) => date >= CheckIn && date < CheckOut;

    /// <summary>Перевіряє, чи має період спільні ночі з іншим періодом.</summary>
    /// <param name="other">Інший період.</param>
    /// <returns>true, якщо є хоча б одна спільна ніч.</returns>
    /// <exception cref="ArgumentNullException">Інший період не задано.</exception>
    public bool Overlaps(StayPeriod other)
    {
        ArgumentNullException.ThrowIfNull(other);
          return CheckIn < other.CheckOut   && other.CheckIn < CheckOut;
    }

    /// <summary>Перелічує дати всіх ночей періоду за зростанням.</summary>
    /// <returns>Дати від заїзду до дня перед виїздом.</returns>
    public IEnumerable<DateOnly> EachNight()
    {
        for (DateOnly date = CheckIn; date < CheckOut; date = date.AddDays(1))
        {
            yield return date;
        }
    }
}
