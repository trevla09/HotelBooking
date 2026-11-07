using System.Globalization;
using System.Text;
using HotelBooking.Core.Bookings;

namespace HotelBooking.Core.Reporting;

/// <summary>Формування текстового звіту про бронювання.</summary>
public static class BookingReport
{
    /// <summary>Формує текст звіту. Не змінює стан і нічого не друкує.</summary>
    /// <param name="request">Запит на розрахунок.</param>
    /// <param name="result">Результат розрахунку.</param>
    /// <returns>Текст звіту з інваріантною культурою форматування.</returns>
    public static string TextOf(TotalRequest request, TotalResult result)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(result);
        CultureInfo culture = CultureInfo.InvariantCulture;
        StringBuilder text = new();
        text.AppendLine(culture, $"Бронювання № {request.BookingId}");
        text.AppendLine(culture, $"Гість: {request.Guest.Name}");
        foreach (BookingLine line in request.Lines)
        {
            text.AppendLine(culture, $"{line.RoomNumber} x {line.Nights}");
        }

        text.AppendLine(culture, $"Разом: {result.Total:F2}");
        text.AppendLine(culture, $"Стан: {result.Status}");
        return text.ToString();
    }
}
