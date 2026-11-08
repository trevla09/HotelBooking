namespace HotelBooking.Core.Bookings;

/// <summary>Іменовані константи правил ціноутворення бронювання.</summary>
public static class PricingRules
{
    /// <summary>Кількість ночей, з якої діє знижка за тривале проживання.</summary>
    public const int LongStayNights = 10;

    /// <summary>Частка знижки за тривале проживання.</summary>
    public const decimal LongStayRate = 0.05m;

    /// <summary>Частка знижки постійного гостя.</summary>
    public const decimal RegularGuestRate = 0.07m;

    /// <summary>Сума, понад яку бронювання вважається великим, грн.</summary>
    public const decimal LargeBookingFrom = 5000m;

    /// <summary>Частка знижки для великого бронювання.</summary>
    public const decimal LargeBookingRate = 0.10m;

    /// <summary>Найбільша знижка за обсягом бронювання, грн.</summary>
    public const decimal MaxVolumeDiscount = 1000m;

    /// <summary>Сума, понад яку бронювання вважається середнім, грн.</summary>
    public const decimal MediumBookingFrom = 2000m;

    /// <summary>Частка знижки для середнього бронювання.</summary>
    public const decimal MediumBookingRate = 0.05m;

    /// <summary>Частка знижки за бронювання, створене в неділю.</summary>
    public const decimal WeekendRate = 0.02m;

    /// <summary>Частка знижки за купоном SALE10.</summary>
    public const decimal CouponSaleRate = 0.10m;

    /// <summary>Фіксована знижка за купоном MINUS200, грн.</summary>
    public const decimal CouponFixed = 200m;

    /// <summary>Сума, понад яку діє купон MINUS200, грн.</summary>
    public const decimal CouponFixedFrom = 1000m;

    /// <summary>Сума, від якої трансфер безкоштовний, грн.</summary>
    public const decimal FreeTransferFrom = 1000m;

    /// <summary>Код купона з відсотковою знижкою.</summary>
    public const string SaleCoupon = "SALE10";

    /// <summary>Код купона з фіксованою знижкою.</summary>
    public const string FixedCoupon = "MINUS200";
}
