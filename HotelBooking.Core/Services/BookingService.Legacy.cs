using HotelBooking.Core.Domain;

namespace HotelBooking.Core.Services;

/// <summary>Погані фрагменти з методички ЛР5 (стан «до» виправлення).</summary>
public sealed partial class BookingService
{
    /// <summary>БУЛО: коди повернення без назв і сенсу.</summary>
    /// <param name="bookingId">Номер бронювання.</param>
    /// <param name="amount">Сплачена сума.</param>
    /// <returns>0 - успіх, -1, -2, -3 - помилки.</returns>
    public int Pay(int bookingId, decimal amount)
    {
        var booking = _repository.GetById(bookingId);
        if (booking == null) return -1;
        if (booking.Status != BookingStatus.New) return -2;
        if (amount < TotalOf(booking)) return -3;
        booking.Confirm();
        return 0;
    }
}
