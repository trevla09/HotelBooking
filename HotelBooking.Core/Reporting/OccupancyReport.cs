namespace HotelBooking.Core.Reporting;

/// <summary>Звіт «Заповнюваність за тиждень» (UC-06).</summary>
/// <param name="FirstDay">Перша дата тижня.</param>
/// <param name="Days">Сім рядків за днями від першої дати.</param>
public sealed record OccupancyReport(DateOnly FirstDay, IReadOnlyList<OccupancyDay> Days)
{
    private const int PercentDecimals = 1;

    /// <summary>Середня заповнюваність за тиждень, відсотки з одним знаком після коми.</summary>
    public decimal AveragePercent => Days.Count == 0
        ? 0m
        : Math.Round(Days.Average(d => d.Percent), PercentDecimals, MidpointRounding.AwayFromZero);
}
