using System.Globalization;
using HotelBooking.Core.Domain;
using HotelBooking.Core.Errors;
using HotelBooking.Core.Services;

namespace HotelBooking.App.Input;

/// <summary>
/// Барикада для меню: перетворює рядки з консолі на запити до сервісів.
/// Далі за барикаду потрапляють лише перевірені значення.
/// </summary>
internal static class ReservationInputParser
{
    /// <summary>Найбільша кількість ночей, яку приймає форма.</summary>
    private const int MaxNights = 30;

    /// <summary>Розбирає запит на бронювання.</summary>
    /// <param name="guestId">Ідентифікатор гостя.</param>
    /// <param name="roomNumber">Номер кімнати.</param>
    /// <param name="checkIn">Дата заїзду, дд.мм.рррр.</param>
    /// <param name="nights">Кількість ночей.</param>
    /// <returns>Запит або пояснення першої помилки.</returns>
    public static Result<ReserveRequest> ParseReserve(
        string? guestId,
        string? roomNumber,
        string? checkIn,
        string? nights)
    {
        string? guestError = CheckPositive(guestId, "Ідентифікатор гостя", out int guest);
        string? roomError = BookingNightParser.CheckRoom(roomNumber, out string room);
        string? dateError = BookingNightParser.CheckDate(checkIn, out DateOnly day);
        string? nightsError = CheckNights(nights, out int count);
        string? error = guestError ?? roomError ?? dateError ?? nightsError;

        return error is null
            ? Result<ReserveRequest>.Ok(
                new ReserveRequest(guest, room, StayPeriod.FromNights(day, count)))
            : Result<ReserveRequest>.Fail(error);
    }

    /// <summary>Розбирає період проживання: дата заїзду і кількість ночей.</summary>
    /// <param name="checkIn">Дата заїзду, дд.мм.рррр.</param>
    /// <param name="nights">Кількість ночей.</param>
    /// <returns>Період або пояснення першої помилки.</returns>
    public static Result<StayPeriod> ParsePeriod(string? checkIn, string? nights)
    {
        string? dateError = BookingNightParser.CheckDate(checkIn, out DateOnly day);
        string? nightsError = CheckNights(nights, out int count);
        string? error = dateError ?? nightsError;

        return error is null
            ? Result<StayPeriod>.Ok(StayPeriod.FromNights(day, count))
            : Result<StayPeriod>.Fail(error);
    }

    /// <summary>Розбирає ідентифікатор бронювання.</summary>
    /// <param name="input">Введений рядок.</param>
    /// <returns>Ідентифікатор або пояснення помилки.</returns>
    public static Result<int> ParseBookingId(string? input)
    {
        string? error = CheckPositive(input, "Номер бронювання", out int id);
        return error is null ? Result<int>.Ok(id) : Result<int>.Fail(error);
    }

    /// <summary>Розбирає дату першого дня тижня для звіту.</summary>
    /// <param name="input">Дата дд.мм.рррр.</param>
    /// <returns>Дата або пояснення помилки.</returns>
    public static Result<DateOnly> ParseDate(string? input)
    {
        string? error = BookingNightParser.CheckDate(input, out DateOnly day);
        return error is null ? Result<DateOnly>.Ok(day) : Result<DateOnly>.Fail(error);
    }

    /// <summary>Розбирає критерії пошуку. Порожнє поле означає «без умови».</summary>
    /// <param name="namePart">Частина імені гостя.</param>
    /// <param name="onDate">Дата ночі, дд.мм.рррр.</param>
    /// <param name="status">Назва стану: New, Confirmed, CheckedIn, CheckedOut, Cancelled.</param>
    /// <returns>Критерії або пояснення помилки.</returns>
    public static Result<BookingSearch> ParseSearch(
        string? namePart,
        string? onDate,
        string? status)
    {
        string? name = string.IsNullOrWhiteSpace(namePart) ? null : namePart.Trim();
        DateOnly? date = null;
        if (!string.IsNullOrWhiteSpace(onDate))
        {
            string? dateError = BookingNightParser.CheckDate(onDate, out DateOnly day);
            if (dateError is not null)
            {
                return Result<BookingSearch>.Fail(dateError);
            }

            date = day;
        }

        BookingStatus? state = null;
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse(status.Trim(), ignoreCase: true, out BookingStatus parsed)
                || !Enum.IsDefined(parsed))
            {
                return Result<BookingSearch>.Fail(
                    "Стан - одне з: New, Confirmed, CheckedIn, CheckedOut, Cancelled");
            }

            state = parsed;
        }

        BookingSearch criteria = new(name, date, state);
        return criteria.HasAnyCriterion
            ? Result<BookingSearch>.Ok(criteria)
            : Result<BookingSearch>.Fail("Задайте хоча б одну умову пошуку");
    }

    private static string? CheckPositive(string? input, string field, out int value)
    {
        bool ok = int.TryParse(
            input?.Trim(),
            NumberStyles.None,
            CultureInfo.InvariantCulture,
            out value);
        return ok && value > 0
            ? null
            : string.Format(CultureInfo.InvariantCulture, "{0} - ціле число більше 0", field);
    }

    private static string? CheckNights(string? input, out int nights)
    {
        bool ok = int.TryParse(
            input?.Trim(),
            NumberStyles.None,
            CultureInfo.InvariantCulture,
            out nights);
        if (!ok || nights < 1 || nights > MaxNights)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "Кількість ночей - від 1 до {0}",
                MaxNights);
        }

        return null;
    }
}
