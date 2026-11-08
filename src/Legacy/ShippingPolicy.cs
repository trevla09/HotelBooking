namespace HotelBooking.Legacy;

public static class ShippingPolicy
{
    public static decimal For(decimal payable) =>
        payable < PricingRules.FreeShipFrom ? PricingRules.ShipCost : 0m;
}
