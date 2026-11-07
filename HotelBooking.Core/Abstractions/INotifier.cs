namespace HotelBooking.Core.Abstractions;

/// <summary>Сповіщувач про події бронювання.</summary>
public interface INotifier
{
    /// <summary>Надсилає сповіщення.</summary>
    /// <param name="message">Текст сповіщення.</param>
    void Notify(string message);
}
