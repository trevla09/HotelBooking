namespace HotelBooking.Core.Legacy;

/// <summary>Рядок бронювання зі старого коду.</summary>
public class LegacyLine
{
    /// <summary>Номер кімнати.</summary>
    public string RoomNumber { get; set; } = "";

    /// <summary>Кількість ночей.</summary>
    public int Nights { get; set; }

    /// <summary>Тариф за ніч, грн.</summary>
    public decimal NightlyRate { get; set; }
}

/// <summary>Бронювання зі старого коду: усі поля відкриті.</summary>
public class LegacyBooking
{
    /// <summary>Ідентифікатор.</summary>
    public int Id { get; set; }

    /// <summary>Стан числом.</summary>
    public int Status { get; set; }

    /// <summary>Сума.</summary>
    public decimal Total { get; set; }

    /// <summary>Пошта гостя.</summary>
    public string Email { get; set; } = "";

    /// <summary>Рядки бронювання.</summary>
    public List<LegacyLine> Lines { get; set; } = new();

    /// <summary>Додає рядок.</summary>
    /// <param name="line">Рядок.</param>
    public virtual void AddLine(LegacyLine line) => Lines.Add(line);
}

/// <summary>Архівне бронювання, успадковане від звичайного.</summary>
public class ArchivedBooking : LegacyBooking
{
    /// <summary>Створює архівне бронювання.</summary>
    /// <param name="id">Ідентифікатор.</param>
    public ArchivedBooking(int id) => Id = id;

    /// <summary>Архів змінювати не можна.</summary>
    /// <param name="line">Рядок.</param>
    public override void AddLine(LegacyLine line)
    {
        throw new NotSupportedException("Архів змінювати не можна");
    }
}
