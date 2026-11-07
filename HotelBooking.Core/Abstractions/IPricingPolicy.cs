using HotelBooking.Core.Domain;

namespace HotelBooking.Core.Abstractions;

/// <summary>Політика розрахунку ціни однієї ночі.</summary>
public interface IPricingPolicy
{
    /// <summary>Обчислює ціну ночі за цією політикою.</summary>
    /// <param name="night">Ніч проживання.</param>
    /// <returns>Ціна ночі, грн.</returns>
    decimal PriceOf(BookingNight night);
}
