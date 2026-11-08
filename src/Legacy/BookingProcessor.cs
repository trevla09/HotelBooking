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

    public bool HasValidEmail => !string.IsNullOrEmpty(Email) && Email.Contains('@');

    public bool IsLoyal => StaysCount > PricingRules.LoyalStays;

    public string Describe()
    {
        string s = Name.Trim().ToUpper();
        if (Kind == GuestKind.Vip) s += " [VIP]";
        if (IsLoyal) s += " [ЛОЯЛЬНИЙ]";
        s += " <" + Email.ToLower() + ">";
        int years = DateTime.Now.Year - SinceUtc.Year;
        s += " стаж " + years;
        return s;
    }
}

public class RoomLine
{
    public string RoomNumber = "";
    public int Nights;
    public decimal Rate;
    public string Category = "";

    public decimal Amount => Nights * Rate;
}

public class BookingProcessor
{
    private readonly List<string> _log = new();

    public string Handle(BookingRequest request)
    {
        string? error = Validate(request);
        if (error is not null) return error;
        _log.Add("ok " + request.BookingId);

        var lines = request.Lines!;
        var client = request.Client;
        decimal subtotal = Subtotal(lines);
        decimal discount = DiscountPolicy.For(subtotal, client.Kind, client.StaysCount);
        decimal ship = ShippingPolicy.For(subtotal - discount);
        decimal total = subtotal - discount + ship;

        string txt = new ReportBuilder(request.Currency)
            .Header(request.BookingId, client.Name)
            .Lines(lines)
            .Row("Знижка", discount)
            .Row("Доставка", ship)
            .Row("Разом", total)
            .Build();

        if (request.SendMail)
        {
            _log.Add("mail -> " + client.Email);
        }
        return txt;
    }

    private static string? Validate(BookingRequest request)
    {
        if (request.Lines is null) return "ERR: null";
        if (request.Lines.Count == 0) return "ERR: empty";
        if (request.State is not (BookingState.New or BookingState.Paid)) return "ERR: state";
        if (!request.Client.HasValidEmail) return "ERR: mail";
        return null;
    }

    private static decimal Subtotal(IReadOnlyList<RoomLine> lines)
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

    public string DumpLog()
    {
        string r = "";
        foreach (var l in _log) r += l + "\n";
        return r;
    }
}
