using HotelBooking.Core.Abstractions;
using HotelBooking.Core.Domain;

namespace HotelBooking.Core.Pricing;

/// <summary>
/// Профільне правило варіанта: ціна ночі = базовий тариф × сезонний коефіцієнт,
/// округлена до копійок від нуля (spec.md, таблиця 4.1).
/// </summary>
public sealed class SeasonalPricingPolicy : IPricingPolicy
{
    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">Ніч не задано.</exception>
    public decimal PriceOf(BookingNight night)
    {
        ArgumentNullException.ThrowIfNull(night);
        decimal factor = SeasonCalendar.FactorOf(SeasonCalendar.SeasonOf(night.Date));
        return Math.Round(night.Rate * factor, Money.Decimals, MidpointRounding.AwayFromZero);
    }
}
