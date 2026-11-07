namespace HotelBooking.Core.Domain;

/// <summary>Номер у фонді готелю. Незмінний.</summary>
public sealed record Room
{
    /// <summary>Створює номер.</summary>
    /// <param name="number">Номер кімнати, не порожній.</param>
    /// <param name="category">Категорія номера.</param>
    /// <param name="capacity">Кількість місць, більша за нуль.</param>
    /// <param name="baseRate">Базовий тариф за ніч, грн, не від'ємний.</param>
    /// <exception cref="ArgumentException">Номер кімнати порожній.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Місткість або тариф поза межами.</exception>
    public Room(string number, string category, int capacity, decimal baseRate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(number);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
        ArgumentOutOfRangeException.ThrowIfNegative(baseRate);

        Number = number;
        Category = category ?? string.Empty;
        Capacity = capacity;
        BaseRate = baseRate;
    }

    /// <summary>Номер кімнати.</summary>
    public string Number { get; }

    /// <summary>Категорія номера.</summary>
    public string Category { get; }

    /// <summary>Кількість місць у номері.</summary>
    public int Capacity { get; }

    /// <summary>Базовий тариф за ніч, грн.</summary>
    public decimal BaseRate { get; }
}
