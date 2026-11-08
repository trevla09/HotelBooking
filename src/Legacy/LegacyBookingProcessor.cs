using System;
using System.Collections.Generic;

namespace HotelBooking.Legacy;

public class Guest
{
    public int Id;
    public string Name = "";
    public string Email = "";
    public GuestKind Kind = GuestKind.Regular;
    public int StaysCount;
    public DateTime SinceUtc;
}

public class RoomLine
{
    public string RoomNumber = "";
    public int Nights;
    public decimal Rate;
    public string Category = "";
}

public class LegacyBookingProcessor
{
    private decimal _tmpSum;
    private decimal _tmpDiscount;
    private readonly List<string> _log = new();

    public string Handle(int bookingId, int guestId,
        string guestName, string? guestMail,
        GuestKind guestKind, int guestStays,
        List<RoomLine>? lines, BookingState state,
        string currency, DateTime createdAt, bool sendMail)
    {
        if (lines is null) return "ERR: null";
        if (lines.Count == 0) return "ERR: empty";
        if (state is not (BookingState.New or BookingState.Paid)) return "ERR: state";
        if (guestMail is null || !guestMail.Contains("@")) return "ERR: mail";
        _log.Add("ok " + bookingId);

        _tmpSum = Subtotal(lines);
        _tmpDiscount = DiscountOf(_tmpSum, guestKind, guestStays);
        decimal ship = ShippingOf(_tmpSum - _tmpDiscount);
        decimal total = _tmpSum - _tmpDiscount + ship;

        string txt = "Документ #" + bookingId + "\n";
        txt += "Клієнт: " + guestName + "\n";
        for (int i = 0; i < lines.Count; i++)
        {
            txt += lines[i].RoomNumber + " x" + lines[i].Nights + " = " +
                (lines[i].Nights * lines[i].Rate)
                    .ToString("0.00") + " " + currency + "\n";
        }
        txt += "Знижка: " + _tmpDiscount.ToString("0.00") +
            " " + currency + "\n";
        txt += "Доставка: " + ship.ToString("0.00") +
            " " + currency + "\n";
        txt += "Разом: " + total.ToString("0.00") +
            " " + currency + "\n";

        if (sendMail)
        {
            _log.Add("mail -> " + guestMail);
        }
        return txt;
    }

    private static decimal Subtotal(List<RoomLine> lines)
    {
        decimal sum = 0m;
        for (int i = 0; i < lines.Count; i++)
        {
            sum += lines[i].Nights * lines[i].Rate;
        }
        return sum;
    }

    private static decimal DiscountOf(decimal subtotal, GuestKind guestKind, int guestStays)
    {
        decimal discount = 0m;
        if (guestKind == GuestKind.Vip)
        {
            discount = subtotal * PricingRules.VipRate;
            if (discount > PricingRules.MaxDiscount) discount = PricingRules.MaxDiscount;
        }
        else if (guestKind == GuestKind.Staff)
        {
            discount = subtotal * PricingRules.StaffRate;
            if (discount > PricingRules.MaxDiscount) discount = PricingRules.MaxDiscount;
        }
        else if (guestStays > PricingRules.LoyalStays)
        {
            discount = subtotal * PricingRules.LoyalRate;
            if (discount > PricingRules.MaxDiscount) discount = PricingRules.MaxDiscount;
        }
        return discount;
    }

    private static decimal ShippingOf(decimal payable) =>
        payable < PricingRules.FreeShipFrom ? PricingRules.ShipCost : 0m;

    public decimal Preview(GuestKind guestKind, int guestStays,
        List<RoomLine> lines)
    {
        decimal subtotal = Subtotal(lines);
        decimal payable = subtotal - DiscountOf(subtotal, guestKind, guestStays);
        return payable + ShippingOf(payable);
    }

    public string DescribeGuest(Guest g)
    {
        string s = g.Name.Trim().ToUpper();
        if (g.Kind == GuestKind.Vip) s += " [VIP]";
        if (g.StaysCount > PricingRules.LoyalStays) s += " [ЛОЯЛЬНИЙ]";
        s += " <" + g.Email.ToLower() + ">";
        int years = DateTime.Now.Year - g.SinceUtc.Year;
        s += " стаж " + years;
        return s;
    }

    public string DumpLog()
    {
        string r = "";
        foreach (var l in _log) r += l + "\n";
        return r;
    }
}
