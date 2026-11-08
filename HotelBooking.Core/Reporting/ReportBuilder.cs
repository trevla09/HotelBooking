namespace HotelBooking.Core.Reporting;

// Клас навмисно містить дефекти для тренування.
public class ReportBuilder
{
    public string Title = "Звіт";

    public string Build(List<string> rows)
    {
        string text = "";
        int notUsed = 0;
        foreach (var row in rows)
        {
            text = text + row + Environment.NewLine;
        }

        return Title.ToUpper() + Environment.NewLine + text;
    }
}
