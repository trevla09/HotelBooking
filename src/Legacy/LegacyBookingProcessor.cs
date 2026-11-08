using System;
using System.Collections.Generic;

namespace HotelBooking.Legacy;

public class Guest
{
    public int Id;
    public string Name = "";
    public string Email = "";
    public string Kind = "regular";
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
        string guestKind, int guestStays,
        List<RoomLine>? lines, string state,
        string currency, DateTime createdAt, bool sendMail)
    {
        if (lines != null)
        {
            if (lines.Count > 0)
            {
                if (state == "new" || state == "paid")
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
        if (guestKind == "vip")
        {
            _tmpDiscount = _tmpSum * 0.15m;
            if (_tmpDiscount > 500m) _tmpDiscount = 500m;
        }
        else if (guestKind == "staff")
        {
            _tmpDiscount = _tmpSum * 0.30m;
            if (_tmpDiscount > 500m) _tmpDiscount = 500m;
        }
        else if (guestStays > 10)
        {
            _tmpDiscount = _tmpSum * 0.05m;
            if (_tmpDiscount > 500m) _tmpDiscount = 500m;
        }

        decimal ship = 0m;
        if (_tmpSum - _tmpDiscount < 1000m) ship = 60m;
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

    public decimal Preview(string guestKind, int guestStays,
        List<RoomLine> lines)
    {
        decimal s = 0m;
        foreach (var it in lines)
        {
            s += it.Nights * it.Rate;
        }
        decimal d = 0m;
        if (guestKind == "vip")
        {
            d = s * 0.15m;
            if (d > 500m) d = 500m;
        }
        else if (guestKind == "staff")
        {
            d = s * 0.30m;
            if (d > 500m) d = 500m;
        }
        else if (guestStays > 10)
        {
            d = s * 0.05m;
            if (d > 500m) d = 500m;
        }
        decimal sh = 0m;
        if (s - d < 1000m) sh = 60m;
        return s - d + sh;
    }

    public string DescribeGuest(Guest g)
    {
        string s = g.Name.Trim().ToUpper();
        if (g.Kind == "vip") s += " [VIP]";
        if (g.StaysCount > 10) s += " [ЛОЯЛЬНИЙ]";
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
