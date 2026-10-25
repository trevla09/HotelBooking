namespace HotelBooking.Core.Domain;

/// <summary>Стан бронювання в його життєвому циклі.</summary>
public enum BookingStatus
{
    /// <summary>Нове, ще не підтверджене.</summary>
    New = 0,

    /// <summary>Підтверджене готелем.</summary>
    Confirmed = 1,

    /// <summary>Гість заселився.</summary>
    CheckedIn = 2,

    /// <summary>Гість виїхав.</summary>
    CheckedOut = 3,

    /// <summary>Скасоване.</summary>
    Cancelled = 4,
}
