using System.Globalization;
using HotelBooking.Core.Domain;
using HotelBooking.Core.Errors;

namespace HotelBooking.App.Input;

/// <summary>Барикада: перетворює рядки з консолі на перевірену ніч проживання.</summary>
internal static class BookingNightParser
{
    /// <summary>Найдовший допустимий номер кімнати, символів.</summary>
    private const int MaxRoomNumberLength = 10;

    /// <summary>Найбільший допустимий тариф за ніч, грн.</summary>
    private const decimal MaxRate = 100_000m;

    /// <summary>Розбирає введені рядки.</summary>
    /// <param name="roomNumber">Номер кімнати.</param>
    /// <param name="date">Дата у форматі дд.мм.рррр.</param>
    /// <param name="rate">Тариф, дробова частина через кому.</param>
    /// <returns>Ніч проживання або пояснення помилки.</returns>
    public static Result<BookingNight> Parse(string? roomNumber, string? date, string? rate)
    {
        string? roomError = CheckRoom(roomNumber, out string room);
        string? dateError = CheckDate(date, out DateOnly day);
        string? rateError = CheckRate(rate, out decimal price);
        string? error = roomError ?? dateError ?? rateError;

        return error is null
            ? Result<BookingNight>.Ok(new BookingNight(room, day, price))
            : Result<BookingNight>.Fail(error);
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

    /// <summary>Перевіряє номер кімнати.</summary>
    /// <param name="input">Введений рядок.</param>
    /// <param name="room">Номер без пробілів по краях.</param>
    /// <returns>Пояснення помилки або null.</returns>
    internal static string? CheckRoom(string? input, out string room)
    {
        room = input?.Trim() ?? string.Empty;
        if (room.Length == 0 || room.Length > MaxRoomNumberLength)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "Номер кімнати - від 1 до {0} символів",
                MaxRoomNumberLength);
        }

        return null;
    }

    /// <summary>Перевіряє дату у форматі дд.мм.рррр.</summary>
    /// <param name="input">Введений рядок.</param>
    /// <param name="day">Розібрана дата.</param>
    /// <returns>Пояснення помилки або null.</returns>
    internal static string? CheckDate(string? input, out DateOnly day)
    {
        bool ok = DateOnly.TryParseExact(
            input?.Trim(),
            "dd.MM.yyyy",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out day);
        return ok ? null : "Дата - у форматі дд.мм.рррр, наприклад 10.11.2026";
    }

    private static string? CheckRate(string? input, out decimal price)
    {
        CultureInfo ukrainian = CultureInfo.GetCultureInfo("uk-UA");
        bool ok = decimal.TryParse(input, NumberStyles.Number, ukrainian, out price);
        if (!ok || price < 0m || price > MaxRate)
        {
            return string.Format(
                ukrainian,
                "Тариф - число від 0 до {0:N0}, наприклад 250,50",
                MaxRate);
        }

        return null;
    }
}
