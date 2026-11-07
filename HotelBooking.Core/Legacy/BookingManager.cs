namespace HotelBooking.Core.Legacy;

/// <summary>Клас-Бог для аудиту SOLID (ЛР4, завдання 5).</summary>
public class BookingManager
{
    private readonly FileBookingStore _store = new FileBookingStore();
    private readonly SmtpMailer _mailer = new SmtpMailer();

    /// <summary>Рахує суму, зберігає бронювання у файл і надсилає лист.</summary>
    /// <param name="booking">Бронювання.</param>
    /// <param name="guestType">Тип гостя: regular, corporate або staff.</param>
    /// <returns>Сума бронювання, грн.</returns>
    public decimal Place(LegacyBooking booking, string guestType)
    {
        decimal total = 0m;
        foreach (var line in booking.Lines)
            total += line.NightlyRate * line.Nights;

        if (guestType == "regular")
            total = total * 0.95m;
        else if (guestType == "corporate")
            total = total * 0.90m;
        else if (guestType == "staff")
            total = total * 0.70m;

        if (total > 1000m)
            total = total - 50m;

        booking.Total = total;
        booking.Status = 1;
        _store.SaveToFile(booking, "bookings.json");
        _mailer.Send(booking.Email, "Booking " + booking.Id, "" + total);
        return total;
    }

    /// <summary>Будує CSV-звіт, пише його у файл і в консоль.</summary>
    /// <param name="bookings">Бронювання.</param>
    /// <returns>Текст звіту.</returns>
    public string BuildCsvReport(List<LegacyBooking> bookings)
    {
        var text = "id;total;status" + Environment.NewLine;
        foreach (var b in bookings)
            text += b.Id + ";" + b.Total + ";" + b.Status;

        File.WriteAllText("report.csv", text);
        Console.WriteLine("Звіт збережено");
        return text;
    }

    /// <summary>Переносить бронювання в архів.</summary>
    /// <param name="booking">Бронювання.</param>
    public void Archive(LegacyBooking booking)
    {
        LegacyBooking archived = new ArchivedBooking(booking.Id);
        foreach (var line in booking.Lines)
            archived.AddLine(line);

        _store.SaveToFile(archived, "archive.json");
    }
}
