using HotelBooking.Core.Abstractions;
using HotelBooking.Core.Domain;

namespace HotelBooking.Core.Pricing;

/// <summary>Звичайна ціна: тариф ночі без знижки.</summary>
public sealed class StandardPricingPolicy : IPricingPolicy
{
    /// <inheritdoc/>
    public decimal PriceOf(BookingNight night)
    {
        ArgumentNullException.ThrowIfNull(night);
        return night.Rate;
    }
}
