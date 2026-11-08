using System.Collections.Generic;
using System.Text;

namespace HotelBooking.Legacy;

public sealed class ReportBuilder
{
    private readonly StringBuilder _text = new();
    private readonly string _currency;

    public ReportBuilder(string currency)
    {
        _currency = currency;
    }

    public ReportBuilder Header(int bookingId, string guestName)
    {
        _text.Append("Документ #").Append(bookingId).Append('\n');
        _text.Append("Клієнт: ").Append(guestName).Append('\n');
        return this;
    }

    public ReportBuilder Lines(IEnumerable<RoomLine> lines)
    {
        foreach (var line in lines)
        {
            _text.Append(line.RoomNumber).Append(" x").Append(line.Nights)
                .Append(" = ").Append(Money(line.Amount)).Append('\n');
        }
        return this;
    }

    public ReportBuilder Row(string label, decimal amount)
    {
        _text.Append(label).Append(": ").Append(Money(amount)).Append('\n');
        return this;
    }

    public string Build() => _text.ToString();

    private string Money(decimal amount) => amount.ToString("0.00") + " " + _currency;
}
