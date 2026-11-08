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

    public decimal Amount => Nights * Rate;
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
        _tmpDiscount = DiscountPolicy.For(_tmpSum, guestKind, guestStays);
        decimal ship = ShippingPolicy.For(_tmpSum - _tmpDiscount);
        decimal total = _tmpSum - _tmpDiscount + ship;

        string txt = new ReportBuilder(currency)
            .Header(bookingId, guestName)
            .Lines(lines)
            .Row("Знижка", _tmpDiscount)
            .Row("Доставка", ship)
            .Row("Разом", total)
            .Build();

        if (sendMail)
        {
            _log.Add("mail -> " + guestMail);
        }
        return txt;
    }

    private static decimal Subtotal(List<RoomLine> lines)
    {
        decimal sum = 0m;
        foreach (var line in lines)
        {
            sum += line.Amount;
        }
        return sum;
    }

    public decimal Preview(GuestKind guestKind, int guestStays,
        List<RoomLine> lines)
    {
        decimal subtotal = Subtotal(lines);
        decimal payable = subtotal - DiscountPolicy.For(subtotal, guestKind, guestStays);
        return payable + ShippingPolicy.For(payable);
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
