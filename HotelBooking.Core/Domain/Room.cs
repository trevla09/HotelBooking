namespace HotelBooking.Core.Domain;

/// <summary>Номер у фонді готелю.</summary>
public sealed class Room
{
    /// <summary>Номер кімнати.</summary>
    public string Number { get; init; } = string.Empty;

    /// <summary>Категорія номера.</summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>Кількість місць у номері.</summary>
    public int Capacity { get; init; }

    /// <summary>Базовий тариф за добу, грн.</summary>
    public decimal BaseRate { get; init; }
}
