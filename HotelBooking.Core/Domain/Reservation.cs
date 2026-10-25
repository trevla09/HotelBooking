using System;
using System.Collections.Generic;
using System.Linq;

namespace HotelBooking.Core.Domain;

// клас
public class Reservation
{
    private readonly List<string[]> _lines = new List<string[]>();

    // конструктор
    public Reservation(string id, string guestName)
    {
        Id = id;              // ставимо id
        GuestName = guestName;         // ставимо cl
        CreatedAt = DateTime.Now;   // ставимо дату
    }

    public string Id { get; private set; }

    public string GuestName { get; private set; }

    public int Status { get; private set; } = 0;  // 0-нова,1-підтв,2-засел,3-скасов

    public DateTime CreatedAt { get; private set; }

    // public string prim;   // примітка, поки не треба

    // пошук
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

    // метод додавання
    public void AddLine(string roomNumber, int nights, decimal nightlyRate)
    {
        string[] line = new string[3];
        line[0] = roomNumber;
        line[1] = nights.ToString();
        line[2] = nightlyRate.ToString();
        _lines.Add(line);   // додаємо t у ln
    }

    // ProcessData
    public decimal CalculateTotal(bool isRegularGuest)
    {
        decimal total = 0;
        int lineCount = 0;
        for (int i = 0; i < _lines.Count; i++)
        {
            int nights = int.Parse(_lines[i][1]);
            decimal nightlyRate = decimal.Parse(_lines[i][2]);
            total = total + (nights * nightlyRate);   // додаємо до суми
            lineCount = lineCount + 1;     // збільшуємо kolvo на одиницю
        }

        // if (sum1 > 500) { sum1 = sum1 - 50; } // стара знижка
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

        total = total + (total * 0.2m);
        return Math.Round(total, 2);   // повертаємо sum1
    }

    // міняємо статус
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

        return false;   // не можна
    }

    // перевірка
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

    // звіт
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
        return report;   // повертаємо s
    }
}
