using HotelBooking.Core.Bookings;
using HotelBooking.Core.Domain;

namespace HotelBooking.Core.Legacy;

/// <summary>Успадкований розрахунок вартості бронювання (метод-монстр, ЛР3).</summary>
public static class LegacyProcessor
{
    /// <summary>Диспетчер: викликає підпрограми розрахунку в потрібному порядку.</summary>
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
    /// <returns>Вартість, грн.</returns>
    public static decimal Process(
        int bookingId, string guestName, string guestEmail,
        bool isRegular, IReadOnlyList<BookingLine> lines,
        DateOnly createdAt, string couponCode,
        BookingStatus status, bool printToConsole,
        decimal transferPrice, out BookingStatus newStatus)
    {
        BookingGuards.EnsureHeaderValid(
            bookingId, guestName, guestEmail);
        decimal total = BookingTotalCalculator.SubtotalOf(lines);
        total -= BookingTotalCalculator.GuestDiscountOf(total, isRegular);
        total -= BookingTotalCalculator.VolumeDiscountOf(total);
        total -= BookingTotalCalculator.CouponDiscountOf(total, couponCode);
        total -= BookingTotalCalculator.WeekendDiscountOf(total, createdAt);
        total += BookingTotalCalculator.TransferFeeOf(total, transferPrice);
        total = status == BookingStatus.Cancelled
            ? 0m
            : Math.Round(Math.Max(total, 0m), 2);
        newStatus = BookingTotalCalculator.NextStatusOf(status, total);
        if (printToConsole)
        {
            Console.WriteLine("Бронювання № " + bookingId);
            Console.WriteLine("Гість: " + guestName);
            foreach (BookingLine line in lines)
            {
                Console.WriteLine(line.RoomNumber + " x " +
                    line.Nights);
            }
            Console.WriteLine("Разом: " + total);
            Console.WriteLine("Стан: " + newStatus);
        }
        return total;
    }
}
