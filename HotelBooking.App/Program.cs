using System.Globalization;
using System.Text;
using HotelBooking.Core.Domain;

Console.OutputEncoding = Encoding.UTF8;

Booking booking = new()
{
    Id = 1,
    GuestId = 10,
    CreatedAt = DateTimeOffset.Now,
};

booking.AddNight(new BookingNight
{
    RoomNumber = "204",
    Date = new DateOnly(2026, 11, 13),
    Rate = 1450.00m,
});
booking.AddNight(new BookingNight
{
    RoomNumber = "204",
    Date = new DateOnly(2026, 11, 14),
    Rate = 1650.00m,
});

string total = booking.Total()
    .ToString("F2", CultureInfo.InvariantCulture);

Console.WriteLine($"Бронювання #{booking.Id}");
Console.WriteLine($"Стан: {booking.Status}");
Console.WriteLine($"Ночей: {booking.Nights.Count}");
Console.WriteLine($"Сума: {total}");
