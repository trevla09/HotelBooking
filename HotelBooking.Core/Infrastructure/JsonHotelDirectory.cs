using System.Text.Json;
using HotelBooking.Core.Abstractions;
using HotelBooking.Core.Domain;

namespace HotelBooking.Core.Infrastructure;

/// <summary>Довідник номерів і гостей, прочитаний з двох JSON-файлів.</summary>
public sealed class JsonHotelDirectory : IHotelDirectory
{
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <summary>Читає номери і гостей з файлів.</summary>
    /// <param name="roomsPath">Шлях до rooms.json.</param>
    /// <param name="guestsPath">Шлях до guests.json.</param>
    /// <exception cref="ArgumentException">Шлях порожній.</exception>
    /// <exception cref="FileNotFoundException">Файлу довідника немає.</exception>
    /// <exception cref="InvalidDataException">Файл пошкоджений або дані некоректні.</exception>
    public JsonHotelDirectory(string roomsPath, string guestsPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roomsPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(guestsPath);

        Rooms = Read<Room>(roomsPath);
        Guests = Read<Guest>(guestsPath);
    }

    /// <inheritdoc/>
    public IReadOnlyList<Room> Rooms { get; }

    /// <inheritdoc/>
    public IReadOnlyList<Guest> Guests { get; }

    /// <inheritdoc/>
    public Room? FindRoom(string number) =>
        Rooms.FirstOrDefault(r => string.Equals(r.Number, number, StringComparison.Ordinal));

    /// <inheritdoc/>
    public Guest? FindGuest(int id) => Guests.FirstOrDefault(g => g.Id == id);

    private List<T> Read<T>(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Немає файлу довідника {path}.", path);
        }

        try
        {
            return JsonSerializer.Deserialize<List<T>>(File.ReadAllText(path), _options)
                ?? new List<T>();
        }
        catch (Exception ex) when (ex is JsonException or ArgumentException)
        {
            throw new InvalidDataException($"Файл {path} некоректний: {ex.Message}", ex);
        }
    }
}
