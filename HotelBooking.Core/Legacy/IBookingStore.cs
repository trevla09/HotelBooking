namespace HotelBooking.Core.Legacy;

/// <summary>Широкий інтерфейс сховища (ЛР4, завдання 5).</summary>
public interface IBookingStore
{
    /// <summary>Додає бронювання.</summary>
    /// <param name="booking">Бронювання.</param>
    void Add(LegacyBooking booking);

    /// <summary>Повертає бронювання за ідентифікатором.</summary>
    /// <param name="id">Ідентифікатор.</param>
    /// <returns>Бронювання.</returns>
    LegacyBooking GetById(int id);

    /// <summary>Зберігає бронювання у файл.</summary>
    /// <param name="booking">Бронювання.</param>
    /// <param name="path">Шлях до файлу.</param>
    void SaveToFile(LegacyBooking booking, string path);

    /// <summary>Надсилає лист.</summary>
    /// <param name="to">Адреса.</param>
    /// <param name="body">Текст листа.</param>
    void SendEmail(string to, string body);

    /// <summary>Експортує бронювання в CSV.</summary>
    /// <returns>Текст CSV.</returns>
    string ExportCsv();

    /// <summary>Робить резервну копію.</summary>
    /// <param name="folder">Тека для копії.</param>
    void Backup(string folder);

    /// <summary>Друкує рахунок.</summary>
    /// <param name="booking">Бронювання.</param>
    void PrintInvoice(LegacyBooking booking);
}
