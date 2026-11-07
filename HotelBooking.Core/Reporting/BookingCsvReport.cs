using System.Globalization;
using System.Text;
using HotelBooking.Core.Domain;

namespace HotelBooking.Core.Reporting;

/// <summary>CSV-звіт про бронювання. Лише форматує, на диск не пише.</summary>
public static class BookingCsvReport
{
    /// <summary>Будує текст CSV. Не змінює стан.</summary>
    /// <param name="bookings">Бронювання.</param>
    /// <returns>Текст CSV з роздільником крапка з комою.</returns>
    public static string BuildCsv(IReadOnlyList<Booking> bookings)
    {
        ArgumentNullException.ThrowIfNull(bookings);
        CultureInfo culture = CultureInfo.InvariantCulture;
        StringBuilder text = new();
        text.AppendLine("id;total;status");
        foreach (Booking booking in bookings)
        {
            text.AppendLine(culture, $"{booking.Id};{booking.Total():0.00};{booking.Status}");
        }

        return text.ToString();
    }
}
