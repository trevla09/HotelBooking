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
        if (lines != null)
        {
            if (lines.Count > 0)
            {
                if (state == BookingState.New || state == BookingState.Paid)
                {
                    if (guestMail != null &&
                        guestMail.Contains("@"))
                    {
                        _log.Add("ok " + bookingId);
                    }
                    else
                    {
                        return "ERR: mail";
                    }
                }
                else
                {
                    return "ERR: state";
                }
            }
            else
            {
                return "ERR: empty";
            }
        }
        else
        {
            return "ERR: null";
        }

        _tmpSum = 0m;
        for (int i = 0; i < lines.Count; i++)
        {
            _tmpSum += lines[i].Nights * lines[i].Rate;
        }

        _tmpDiscount = 0m;
        if (guestKind == GuestKind.Vip)
        {
            _tmpDiscount = _tmpSum * PricingRules.VipRate;
            if (_tmpDiscount > PricingRules.MaxDiscount) _tmpDiscount = PricingRules.MaxDiscount;
        }
        else if (guestKind == GuestKind.Staff)
        {
            _tmpDiscount = _tmpSum * PricingRules.StaffRate;
            if (_tmpDiscount > PricingRules.MaxDiscount) _tmpDiscount = PricingRules.MaxDiscount;
        }
        else if (guestStays > PricingRules.LoyalStays)
        {
            _tmpDiscount = _tmpSum * PricingRules.LoyalRate;
            if (_tmpDiscount > PricingRules.MaxDiscount) _tmpDiscount = PricingRules.MaxDiscount;
        }

        decimal ship = 0m;
        if (_tmpSum - _tmpDiscount < PricingRules.FreeShipFrom) ship = PricingRules.ShipCost;
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

    public decimal Preview(GuestKind guestKind, int guestStays,
        List<RoomLine> lines)
    {
        decimal s = 0m;
        foreach (var it in lines)
        {
            s += it.Nights * it.Rate;
        }
        decimal d = 0m;
        if (guestKind == GuestKind.Vip)
        {
            d = s * PricingRules.VipRate;
            if (d > PricingRules.MaxDiscount) d = PricingRules.MaxDiscount;
        }
        else if (guestKind == GuestKind.Staff)
        {
            d = s * PricingRules.StaffRate;
            if (d > PricingRules.MaxDiscount) d = PricingRules.MaxDiscount;
        }
        else if (guestStays > PricingRules.LoyalStays)
        {
            d = s * PricingRules.LoyalRate;
            if (d > PricingRules.MaxDiscount) d = PricingRules.MaxDiscount;
        }
        decimal sh = 0m;
        if (s - d < PricingRules.FreeShipFrom) sh = PricingRules.ShipCost;
        return s - d + sh;
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
