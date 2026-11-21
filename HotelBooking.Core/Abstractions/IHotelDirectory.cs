using HotelBooking.Core.Domain;

namespace HotelBooking.Core.Abstractions;

/// <summary>Довідники готелю: фонд номерів і картотека гостей. Лише читання.</summary>
public interface IHotelDirectory
{
    /// <summary>Усі номери фонду, упорядковані за номером кімнати.</summary>
    IReadOnlyList<Room> Rooms { get; }

    /// <summary>Усі гості картотеки, упорядковані за ідентифікатором.</summary>
    IReadOnlyList<Guest> Guests { get; }

    /// <summary>Шукає номер за номером кімнати.</summary>
    /// <param name="number">Номер кімнати.</param>
    /// <returns>Номер або null, якщо такого немає у фонді.</returns>
    Room? FindRoom(string number);

    /// <summary>Шукає гостя за ідентифікатором.</summary>
    /// <param name="id">Ідентифікатор гостя.</param>
    /// <returns>Гість або null, якщо його немає в картотеці.</returns>
    Guest? FindGuest(int id);
}
