namespace HotelBooking.Core.Domain;

/// <summary>Сезон, що визначає коефіцієнт до базового тарифу ночі.</summary>
public enum Season
{
    /// <summary>Низький сезон: листопад - березень, крім свят.</summary>
    Low = 0,

    /// <summary>Середній сезон: квітень, травень, вересень, жовтень.</summary>
    Middle = 1,

    /// <summary>Високий сезон: червень - серпень.</summary>
    High = 2,

    /// <summary>Святковий період: 24 грудня - 7 січня включно.</summary>
    Holiday = 3,
}
