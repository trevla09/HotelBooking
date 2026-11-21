namespace HotelBooking.Core.Abstractions;

/// <summary>Джерело поточного часу. У тестах підмінюється фіксованим годинником.</summary>
public interface IClock
{
    /// <summary>Поточний момент із зсувом часового поясу.</summary>
    DateTimeOffset Now { get; }

    /// <summary>Поточна дата за місцевим часом готелю.</summary>
    DateOnly Today { get; }
}
