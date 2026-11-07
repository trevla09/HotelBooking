namespace HotelBooking.Core.Reporting;

/// <summary>Виведення готового звіту в потік. Не знає про консоль.</summary>
public sealed class BookingReportPrinter
{
    private readonly TextWriter _output;

    /// <summary>Створює принтер, який пише в заданий потік.</summary>
    /// <param name="output">Потік, у який пишеться звіт.</param>
    public BookingReportPrinter(TextWriter output)
    {
        ArgumentNullException.ThrowIfNull(output);
        _output = output;
    }

    /// <summary>Записує звіт у потік. Нічого не повертає.</summary>
    /// <param name="report">Текст звіту.</param>
    public void Print(string report)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(report);
        _output.Write(report);
    }
}
