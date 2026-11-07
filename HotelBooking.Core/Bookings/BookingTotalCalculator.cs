using HotelBooking.Core.Domain;
using HotelBooking.Core.Legacy;

namespace HotelBooking.Core.Bookings;

/// <summary>Підпрограми розрахунку вартості бронювання.</summary>
public static class BookingTotalCalculator
{
    /// <summary>Обчислює суму рядків бронювання зі знижкою за тривале проживання.</summary>
    /// <param name="lines">Рядки бронювання.</param>
    /// <returns>Сума рядків, грн.</returns>
    public static decimal SubtotalOf(IReadOnlyList<BookingLine> lines)
    {
        BookingGuards.EnsureLinesValid(lines);
        decimal subtotal = 0m;
        foreach (BookingLine line in lines)
        {
            decimal sum = line.Nights * line.NightlyRate;
            if (line.Nights >= PricingRules.LongStayNights)
            {
                sum -= sum * PricingRules.LongStayRate;
            }

            subtotal += sum;
        }

        return subtotal;
    }

    /// <summary>Обчислює знижку постійного гостя.</summary>
    /// <param name="amount">Сума до знижки, грн.</param>
    /// <param name="isRegularGuest">Чи є гість постійним.</param>
    /// <returns>Розмір знижки, грн.</returns>
    public static decimal GuestDiscountOf(decimal amount, bool isRegularGuest)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        return isRegularGuest
            ? amount * PricingRules.RegularGuestRate
            : 0m;
    }

    /// <summary>Обчислює знижку за обсягом бронювання.</summary>
    /// <param name="amount">Сума до знижки, грн.</param>
    /// <returns>Розмір знижки, грн.</returns>
    public static decimal VolumeDiscountOf(decimal amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        if (amount > PricingRules.LargeBookingFrom)
        {
            return amount * PricingRules.LargeBookingRate;
        }

        if (amount > PricingRules.MediumBookingFrom)
        {
            return amount * PricingRules.MediumBookingRate;
        }

        return 0m;
    }

    /// <summary>Обчислює знижку за кодом купона.</summary>
    /// <param name="amount">Сума до знижки, грн.</param>
    /// <param name="couponCode">Код купона або null.</param>
    /// <returns>Розмір знижки, грн.</returns>
    public static decimal CouponDiscountOf(decimal amount, string? couponCode)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        return couponCode switch
        {
            PricingRules.SaleCoupon => amount * PricingRules.CouponSaleRate,
            PricingRules.FixedCoupon when amount > PricingRules.CouponFixedFrom
                => PricingRules.CouponFixed,
            _ => 0m,
        };
    }

    /// <summary>Обчислює знижку за бронювання, створене в неділю.</summary>
    /// <param name="amount">Сума до знижки, грн.</param>
    /// <param name="createdAt">Дата створення бронювання.</param>
    /// <returns>Розмір знижки, грн.</returns>
    public static decimal WeekendDiscountOf(decimal amount, DateOnly createdAt)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        return createdAt.DayOfWeek == DayOfWeek.Sunday
            ? amount * PricingRules.WeekendRate
            : 0m;
    }

    /// <summary>Обчислює плату за трансфер.</summary>
    /// <param name="amount">Сума бронювання, грн.</param>
    /// <param name="transferPrice">Вартість трансферу, грн.</param>
    /// <returns>Плата за трансфер, грн; 0 для великих бронювань.</returns>
    public static decimal TransferFeeOf(decimal amount, decimal transferPrice)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(transferPrice);
        return amount < PricingRules.FreeTransferFrom
            ? transferPrice
            : 0m;
    }

    /// <summary>Визначає наступний стан бронювання. Не змінює стан.</summary>
    /// <param name="current">Поточний стан.</param>
    /// <param name="total">Підсумкова вартість, грн.</param>
    /// <returns>Наступний стан.</returns>
    public static BookingStatus NextStatusOf(BookingStatus current, decimal total)
    {
        if (current == BookingStatus.New && total > 0m)
        {
            return BookingStatus.Confirmed;
        }

        return current == BookingStatus.Confirmed
            ? BookingStatus.CheckedIn
            : current;
    }
}
