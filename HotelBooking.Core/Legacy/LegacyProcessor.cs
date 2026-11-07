namespace HotelBooking.Core.Legacy;

/// <summary>Успадкований розрахунок вартості бронювання (метод-монстр, ЛР3).</summary>
public static class LegacyProcessor
{
    /// <summary>Перевіряє дані, рахує вартість, змінює стан і друкує звіт.</summary>
    /// <param name="bookingId">Номер бронювання.</param>
    /// <param name="guestName">Ім'я гостя.</param>
    /// <param name="guestEmail">Пошта гостя.</param>
    /// <param name="isRegular">Чи є гість постійним.</param>
    /// <param name="lines">Рядки бронювання.</param>
    /// <param name="createdAt">Дата створення бронювання.</param>
    /// <param name="couponCode">Код купона.</param>
    /// <param name="status">Поточний стан.</param>
    /// <param name="printToConsole">Чи друкувати звіт у консоль.</param>
    /// <param name="transferPrice">Вартість трансферу, грн.</param>
    /// <param name="newStatus">Новий стан.</param>
    /// <returns>Вартість, грн, або -1 у разі помилки.</returns>
    public static decimal Process(
        int bookingId, string guestName, string guestEmail,
        bool isRegular, List<BookingLine> lines,
        DateOnly createdAt, string couponCode, string status,
        bool printToConsole, decimal transferPrice,
        out string newStatus)
    {
        newStatus = status;
        decimal total = 0;
        if (bookingId <= 0)
        {
            Console.WriteLine("Помилка: номер бронювання");
            return -1;
        }
        if (guestName == null || guestName == "")
        {
            Console.WriteLine("Помилка: немає гостя");
            return -1;
        }
        if (guestEmail == null || !guestEmail.Contains("@"))
        {
            Console.WriteLine("Помилка: пошта гостя");
            return -1;
        }
        if (lines == null || lines.Count == 0)
        {
            Console.WriteLine("Помилка: немає номерів");
            return -1;
        }
        if (status != "New" && status != "Confirmed" &&
            status != "CheckedIn" && status != "Cancelled")
        {
            Console.WriteLine("Помилка: невідомий стан");
            return -1;
        }
        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i].Nights <= 0)
            {
                Console.WriteLine("Помилка: кількість ночей");
                return -1;
            }
            if (lines[i].NightlyRate < 0)
            {
                Console.WriteLine("Помилка: тариф");
                return -1;
            }
            decimal sum = lines[i].Nights * lines[i].NightlyRate;
            if (lines[i].Nights >= 10)
                sum = sum * 0.95m;
            total = total + sum;
        }
        if (isRegular)
            total = total * 0.93m;
        if (total > 5000)
            total = total * 0.9m;
        else if (total > 2000)
            total = total * 0.95m;
        if (couponCode == "SALE10")
            total = total - total * 0.1m;
        else if (couponCode == "MINUS200" && total > 1000)
            total = total - 200;
        if (createdAt.DayOfWeek == DayOfWeek.Sunday)
            total = total * 0.98m;
        if (total < 1000)
            total = total + transferPrice;
        if (total < 0)
            total = 0;
        total = Math.Round(total, 2);
        if (status == "New" && total > 0)
            newStatus = "Confirmed";
        else if (status == "Confirmed")
            newStatus = "CheckedIn";
        else if (status == "Cancelled")
        {
            newStatus = "Cancelled";
            total = 0;
        }
        if (printToConsole)
        {
            Console.WriteLine("Бронювання № " + bookingId);
            Console.WriteLine("Гість: " + guestName);
            Console.WriteLine("Пошта: " + guestEmail);
            for (int i = 0; i < lines.Count; i++)
            {
                Console.WriteLine(lines[i].RoomNumber + " x " +
                    lines[i].Nights);
            }
            Console.WriteLine("Разом: " + total);
            Console.WriteLine("Стан: " + newStatus);
        }
        return total;
    }
}
