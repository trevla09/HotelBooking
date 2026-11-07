#nullable enable

using HotelBooking.Core.Domain;

namespace HotelBooking.Core.Services;

/// <summary>Погані фрагменти з методички ЛР5 (стан «до» виправлення).</summary>
public sealed partial class BookingService
{
    /// <summary>АНТИПАТЕРН 1: порожній catch, дефект зникає.</summary>
    /// <param name="bookingId">Номер бронювання.</param>
    /// <returns>Бронювання або null.</returns>
    public Booking? Load(int bookingId)
    {
        try
        {
            return _repository.GetById(bookingId);
        }
        catch (Exception)
        {
        }
        return null;
    }

    /// <summary>АНТИПАТЕРНИ 2 і 3: широке перехоплення, throw ex.</summary>
    /// <param name="bookingId">Номер бронювання.</param>
    /// <param name="reason">Причина скасування.</param>
    public void Cancel(int bookingId, string reason)
    {
        try
        {
            var booking = _repository.GetById(bookingId);
            booking!.Cancel();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Щось пішло не так");
            throw ex;
        }
    }

    /// <summary>АНТИПАТЕРН 4: try/catch замість звичайної умови.</summary>
    /// <param name="rows">Рядки з кількістю ночей.</param>
    /// <returns>Кількість коректних значень.</returns>
    public int CountValidNights(string[] rows)
    {
        var valid = 0;
        foreach (var row in rows)
        {
            try
            {
                var n = int.Parse(row);
                if (n > 0) valid++;
            }
            catch (FormatException)
            {
                // не число - пропускаємо
            }
        }
        return valid;
    }
}
