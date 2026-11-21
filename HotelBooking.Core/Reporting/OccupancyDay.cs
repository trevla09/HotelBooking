namespace HotelBooking.Core.Reporting;

/// <summary>Заповнюваність готелю за один день (рядок звіту UC-06).</summary>
/// <param name="Date">Дата ночі.</param>
/// <param name="OccupiedRooms">Кількість номерів, зайнятих нескасованими бронюваннями.</param>
/// <param name="TotalRooms">Кількість номерів у фонді.</param>
public sealed record OccupancyDay(DateOnly Date, int OccupiedRooms, int TotalRooms)
{
    private const decimal PercentScale = 100m;
    private const int PercentDecimals = 1;

    /// <summary>Частка зайнятих номерів у відсотках з одним знаком після коми.</summary>
    public decimal Percent => TotalRooms == 0
        ? 0m
        : Math.Round(
            (OccupiedRooms * PercentScale) / TotalRooms,
            PercentDecimals,
            MidpointRounding.AwayFromZero);
}
