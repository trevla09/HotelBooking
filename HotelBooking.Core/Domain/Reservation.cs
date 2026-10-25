using System;
using System.Collections.Generic;
using System.Linq;

namespace HotelBooking.Core.Domain;

/// <summary>
/// Бронювання гостя з переліком номерів і кількістю діб.
/// </summary>
public class Reservation
{
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
    public int Status { get; private set; } = 0;  // 0-нова,1-підтв,2-засел,3-скасов

    /// <summary>Дата й час створення бронювання.</summary>
    public DateTime CreatedAt { get; private set; }

    // public string prim;   

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

        // if (sum1 > 500) { sum1 = sum1 - 50; } // стара знижка
        // знижки не сумуються: діє лише одна, пріоритет має знижка постійного гостя
        if (isRegularGuest == true && total > 1000)
        {
            total = total * 0.9m;
        }
        else if (total > 5000)
        {
            total = total * 0.85m;
        }
        else
        {
            total = total;
        }

        if (lineCount > 10)
        {
            total = total - 100;
        }

        if (total < 0)
        {
            total = 0;
        }
        // ПДВ нараховується на суму вже після всіх знижок
        total = total + (total * 0.2m);
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
        if (Status == 0 && newStatus == 1)
        {
            Status = 1;
            return true;
        }

        if (Status == 1 && newStatus == 2)
        {
            Status = 2;
            return true;
        }

        if (Status == 0 && newStatus == 3)
        {
            Status = 3;
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
                   && GuestName.Length > 2
                   && _lines.Count > 0
                   && _lines.Count < 100
                   && Status >= 0
                   && Status <= 3)
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
