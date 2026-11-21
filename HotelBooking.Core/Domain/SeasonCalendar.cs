namespace HotelBooking.Core.Domain;

/// <summary>Календар сезонів готелю: дата ночі - сезон - коефіцієнт (spec.md, таблиця 4.1).</summary>
public static class SeasonCalendar
{
    /// <summary>Коефіцієнт низького сезону.</summary>
    public const decimal LowFactor = 1.00m;

    /// <summary>Коефіцієнт середнього сезону.</summary>
    public const decimal MiddleFactor = 1.20m;

    /// <summary>Коефіцієнт високого сезону.</summary>
    public const decimal HighFactor = 1.50m;

    /// <summary>Коефіцієнт святкового періоду.</summary>
    public const decimal HolidayFactor = 1.80m;

    private const int January = 1;
    private const int April = 4;
    private const int May = 5;
    private const int June = 6;
    private const int July = 7;
    private const int August = 8;
    private const int September = 9;
    private const int October = 10;
    private const int December = 12;
    private const int HolidayFirstDayInDecember = 24;
    private const int HolidayLastDayInJanuary = 7;

    /// <summary>Визначає сезон для ночі з цією датою.</summary>
    /// <param name="date">Дата ночі проживання.</param>
    /// <returns>Сезон; святковий період має пріоритет над місячним правилом.</returns>
    public static Season SeasonOf(DateOnly date)
    {
        if (IsHoliday(date))
        {
            return Season.Holiday;
        }

        return date.Month switch
        {
            June or July or August => Season.High,
            April or May or September or October => Season.Middle,
            _ => Season.Low,
        };
    }

    /// <summary>Повертає коефіцієнт до базового тарифу для сезону.</summary>
    /// <param name="season">Сезон.</param>
    /// <returns>Коефіцієнт, не менший за 1.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Невідоме значення сезону.</exception>
    public static decimal FactorOf(Season season) => season switch
    {
        Season.Low => LowFactor,
        Season.Middle => MiddleFactor,
        Season.High => HighFactor,
        Season.Holiday => HolidayFactor,
        _ => throw new ArgumentOutOfRangeException(nameof(season), season, "Невідомий сезон."),
    };

    private static bool IsHoliday(DateOnly date) =>
        (date.Month == December && date.Day >= HolidayFirstDayInDecember)
        || (date.Month == January && date.Day <= HolidayLastDayInJanuary);
}
