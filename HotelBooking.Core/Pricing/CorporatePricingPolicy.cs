using HotelBooking.Core.Abstractions;
using HotelBooking.Core.Domain;

namespace HotelBooking.Core.Pricing;

/// <summary>Корпоративна ціна: знижка 10 % на кожну ніч.</summary>
public sealed class CorporatePricingPolicy : IPricingPolicy
{
    private const decimal CorporateFactor = 0.90m;

    /// <inheritdoc/>
    public decimal PriceOf(BookingNight night)
    {
        ArgumentNullException.ThrowIfNull(night);
        return Math.Round(night.Rate * CorporateFactor, 2);
    }
}
