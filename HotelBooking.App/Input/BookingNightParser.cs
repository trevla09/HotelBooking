using System.Globalization;
using HotelBooking.Core.Domain;
using HotelBooking.Core.Errors;

namespace HotelBooking.App.Input;

/// <summary>Барикада: перетворює рядки з консолі на перевірену ніч проживання.</summary>
internal static class BookingNightParser
{
    /// <summary>Розбирає введені рядки.</summary>
    /// <param name="roomNumber">Номер кімнати.</param>
    /// <param name="date">Дата у форматі дд.мм.рррр.</param>
    /// <param name="rate">Тариф, дробова частина через кому.</param>
    /// <returns>Ніч проживання або пояснення помилки.</returns>
    public static Result<BookingNight> Parse(string? roomNumber, string? date, string? rate)
    {
        CultureInfo ukrainian = CultureInfo.GetCultureInfo("uk-UA");
        if (string.IsNullOrWhiteSpace(roomNumber))
        {
            return Result<BookingNight>.Fail("Номер кімнати не може бути порожнім");
        }

        if (!DateOnly.TryParseExact(
            date,
            "dd.MM.yyyy",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out DateOnly day))
        {
            return Result<BookingNight>.Fail("Дата - у форматі дд.мм.рррр, наприклад 10.11.2026");
        }

        if (!decimal.TryParse(rate, NumberStyles.Number, ukrainian, out decimal price)
            || price < 0m)
        {
            return Result<BookingNight>.Fail("Тариф - число не менше 0, наприклад 250,50");
        }

        return Result<BookingNight>.Ok(new BookingNight(roomNumber.Trim(), day, price));
    }

    /// <summary>Рахує рядки з коректною кількістю ночей (ціле число більше 0).</summary>
    /// <param name="rows">Рядки з кількістю ночей.</param>
    /// <returns>Кількість коректних значень.</returns>
    public static int CountValidNights(IEnumerable<string?> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);
        int valid = 0;
        foreach (string? row in rows)
        {
            bool isNumber = int.TryParse(
                row, NumberStyles.Integer, CultureInfo.InvariantCulture, out int nights);
            if (isNumber && nights > 0)
            {
                valid++;
            }
        }

        return valid;
    }
}
