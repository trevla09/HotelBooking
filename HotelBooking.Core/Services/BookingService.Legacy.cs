using HotelBooking.Core.Domain;

namespace HotelBooking.Core.Services;

/// <summary>Погані фрагменти з методички ЛР5 (стан «до» виправлення).</summary>
public sealed partial class BookingService
{
    /// <summary>БУЛО: барикади немає, ядро розбирає рядки саме.</summary>
    /// <param name="bookingId">Номер бронювання.</param>
    /// <param name="roomNumber">Номер кімнати.</param>
    /// <param name="date">Дата рядком.</param>
    /// <param name="rate">Тариф рядком.</param>
    public void AddNight(int bookingId, string roomNumber, string date, string rate)
    {
        var booking = _repository.GetById(bookingId);
        var d = DateOnly.Parse(date);
        var r = decimal.Parse(rate);
        booking!.AddNight(new BookingNight(roomNumber, d, r));
    }
}
