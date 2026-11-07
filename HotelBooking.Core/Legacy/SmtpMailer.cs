namespace HotelBooking.Core.Legacy;

/// <summary>Надсилання листів (заглушка: пише в консоль замість SMTP).</summary>
public class SmtpMailer
{
    /// <summary>Надсилає лист.</summary>
    /// <param name="to">Адреса.</param>
    /// <param name="subject">Тема.</param>
    /// <param name="body">Текст.</param>
    public void Send(string to, string subject, string body)
    {
        Console.WriteLine("Лист для " + to + ": " + subject + " " + body);
    }
}
