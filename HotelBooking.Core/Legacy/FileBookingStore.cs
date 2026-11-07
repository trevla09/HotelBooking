using System.Text.Json;

namespace HotelBooking.Core.Legacy;

/// <summary>Збереження бронювань у JSON-файл (заглушка до класу-Бога).</summary>
public class FileBookingStore
{
    /// <summary>Записує бронювання у файл.</summary>
    /// <param name="booking">Бронювання.</param>
    /// <param name="path">Шлях до файлу.</param>
    public void SaveToFile(LegacyBooking booking, string path)
    {
        File.WriteAllText(path, JsonSerializer.Serialize(booking));
    }
}
