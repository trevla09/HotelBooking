using System;

namespace HotelBooking.Legacy;

public static class DiscountPolicy
{
    public static decimal For(decimal subtotal, GuestKind guestKind, int guestStays)
    {
        decimal discount = subtotal * RateFor(guestKind, guestStays);
        return Math.Min(discount, PricingRules.MaxDiscount);
    }

    private static decimal RateFor(GuestKind guestKind, int guestStays) => guestKind switch
    {
        GuestKind.Vip => PricingRules.VipRate,
        GuestKind.Staff => PricingRules.StaffRate,
        _ => guestStays > PricingRules.LoyalStays ? PricingRules.LoyalRate : 0m,
    };
}
