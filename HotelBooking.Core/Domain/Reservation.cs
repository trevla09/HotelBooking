namespace HotelBooking.Core.Domain;

/// <summary>
/// Бронювання гостя з переліком номерів і кількістю діб.
/// </summary>
public class Reservation
{
    /// <summary>Ставка податку на додану вартість.</summary>
    private const decimal VatRate = 0.2m;

    /// <summary>Поріг суми для знижки постійному гостю, грн.</summary>
    private const decimal RegularDiscountThreshold = 1000m;

    /// <summary>Частка знижки постійному гостю.</summary>
    private const decimal RegularDiscountRate = 0.10m;

    /// <summary>Поріг суми для знижки на велике бронювання, грн.</summary>
    private const decimal LargeBookingThreshold = 5000m;

    /// <summary>Частка знижки на велике бронювання.</summary>
    private const decimal LargeBookingDiscountRate = 0.15m;

    /// <summary>Кількість рядків, з якої діє гуртова знижка.</summary>
    private const int BulkLineCount = 10;

    /// <summary>Сума гуртової знижки, грн.</summary>
    private const decimal BulkDiscountAmount = 100m;

    /// <summary>Межа кількості рядків у бронюванні (не включно).</summary>
    private const int MaxLineCount = 100;

    /// <summary>Найкоротше допустиме ім'я гостя.</summary>
    private const int MinGuestNameLength = 3;

    /// <summary>Стан: нове бронювання.</summary>
    private const int StatusNew = 0;

    /// <summary>Стан: бронювання підтверджене.</summary>
    private const int StatusConfirmed = 1;

    /// <summary>Стан: гість заселився.</summary>
    private const int StatusCheckedIn = 2;

    /// <summary>Стан: бронювання скасоване.</summary>
    private const int StatusCancelled = 3;
    private readonly List<string[]> _lines = new List<string[]>();

    /// <summary>
    /// Створює нове бронювання для гостя.
    /// </summary>
    /// <param name="id">Ідентифікатор бронювання.</param>
    /// <param name="guestName">Ім'я гостя.</param>
    public Reservation(string id, string guestName)
    {
        Id = id;
        GuestName = guestName;
        CreatedAt = DateTime.Now;
    }

    /// <summary>Ідентифікатор бронювання.</summary>
    public string Id { get; private set; }

    /// <summary>Ім'я гостя, на якого оформлено бронювання.</summary>
    public string GuestName { get; private set; }

    /// <summary>Стан бронювання.</summary>
    public int Status { get; private set; } = StatusNew;
    /// <summary>Дата й час створення бронювання.</summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Шукає бронювання за ідентифікатором.
    /// </summary>
    /// <param name="reservations">Список бронювань для пошуку.</param>
    /// <param name="reservationId">Ідентифікатор шуканого бронювання.</param>
    /// <returns>Знайдене бронювання або null, якщо його немає.</returns>
    public static Reservation FindById(List<Reservation> reservations, string reservationId)
    {
        for (int i = 0; i < reservations.Count; i++)
        {
            if (reservations[i].Id == reservationId)
            {
                return reservations[i];
            }
        }

        return null;
    }

    /// <summary>
    /// Додає до бронювання рядок: номер, кількість діб і тариф за добу.
    /// </summary>
    /// <param name="roomNumber">Номер кімнати.</param>
    /// <param name="nights">Кількість діб проживання.</param>
    /// <param name="nightlyRate">Ціна за одну добу, грн.</param>
    public void AddLine(string roomNumber, int nights, decimal nightlyRate)
    {
        string[] line = new string[3];
        line[0] = roomNumber;
        line[1] = nights.ToString();
        line[2] = nightlyRate.ToString();
        _lines.Add(line);
    }

    /// <summary>
    /// Обчислює вартість бронювання з урахуванням знижок і ПДВ.
    /// </summary>
    /// <param name="isRegularGuest">Ознака постійного гостя.</param>
    /// <returns>Сума до сплати, заокруглена до двох знаків.</returns>
    public decimal CalculateTotal(bool isRegularGuest)
    {
        decimal total = 0;
        int lineCount = 0;
        for (int i = 0; i < _lines.Count; i++)
        {
            int nights = int.Parse(_lines[i][1]);
            decimal nightlyRate = decimal.Parse(_lines[i][2]);
            total = total + (nights * nightlyRate);
            lineCount = lineCount + 1;
        }

        // знижки не сумуються: діє лише одна, пріоритет має знижка постійного гостя
        if (isRegularGuest && total > RegularDiscountThreshold)
        {
            total *= 1m - RegularDiscountRate;
        }
        else if (total > LargeBookingThreshold)
        {
            total *= 1m - LargeBookingDiscountRate;
        }

        if (lineCount > BulkLineCount)
        {
            total -= BulkDiscountAmount;
        }

        if (total < 0)
        {
            total = 0;
        }

        // ПДВ нараховується на суму вже після всіх знижок
        total += total * VatRate;

        return Math.Round(total, 2);
    }

    /// <summary>
    /// Змінює стан бронювання, якщо перехід дозволений правилами готелю.
    /// </summary>
    /// <param name="newStatus">Цільовий стан бронювання.</param>
    /// <returns>
    /// true, якщо перехід виконано; false, якщо він заборонений.
    /// </returns>
    public bool TryChangeStatus(int newStatus)
    {
        if (Status == StatusNew && newStatus == StatusConfirmed)
        {
            Status = StatusConfirmed;
            return true;
        }

        if (Status == StatusConfirmed && newStatus == StatusCheckedIn)
        {
            Status = StatusCheckedIn;
            return true;
        }

        if (Status == StatusNew && newStatus == StatusCancelled)
        {
            Status = StatusCancelled;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Перевіряє, чи коректно заповнене бронювання.
    /// </summary>
    /// <returns>true, якщо всі значення в допустимих межах; інакше false.</returns>
    public bool IsValid()
    {
        if (Id != null
            && Id != string.Empty
            && GuestName != null
            && GuestName.Length >= MinGuestNameLength
            && _lines.Count > 0
            && _lines.Count < MaxLineCount
            && Status >= StatusNew
            && Status <= StatusCancelled)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Формує текстовий звіт за рядками бронювання з підсумковою сумою.
    /// </summary>
    /// <returns>Багаторядковий текст звіту.</returns>
    public string BuildReport()
    {
        string report = string.Empty;
        for (int i = 0; i < _lines.Count; i++)
        {
            report = report
                            + "Номер: " + _lines[i][0]
                            + "; діб: " + _lines[i][1]
                            + "; ціна: " + _lines[i][2]
                            + "; сума: " + (int.Parse(_lines[i][1]) * decimal.Parse(_lines[i][2]))
                            + "\n";
        }

        report = report + "Разом: " + CalculateTotal(false) + "\n";
        return report;
    }
}
