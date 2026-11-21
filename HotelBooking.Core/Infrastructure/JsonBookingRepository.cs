using System.Text.Json;
using System.Text.Json.Serialization;
using HotelBooking.Core.Abstractions;
using HotelBooking.Core.Domain;

namespace HotelBooking.Core.Infrastructure;

/// <summary>
/// Сховище бронювань у JSON-файлі. Читає файл один раз при створенні,
/// далі працює зі сховищем у пам'яті і переписує файл цілком у <see cref="Save"/>.
/// </summary>
public sealed class JsonBookingRepository : IBookingRepository
{
    private readonly string _path;
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly InMemoryBookingRepository _memory = new();

    /// <summary>Створює сховище і завантажує бронювання з файлу, якщо він є.</summary>
    /// <param name="path">Шлях до JSON-файлу.</param>
    /// <exception cref="ArgumentException">Шлях порожній.</exception>
    /// <exception cref="InvalidDataException">Файл пошкоджений.</exception>
    public JsonBookingRepository(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _path = path;
        Load();
    }

    /// <inheritdoc/>
    public void Add(Booking booking) => _memory.Add(booking);

    /// <inheritdoc/>
    public Booking? GetById(int id) => _memory.GetById(id);

    /// <inheritdoc/>
    public IReadOnlyList<Booking> GetAll() => _memory.GetAll().OrderBy(b => b.Id).ToList();

    /// <inheritdoc/>
    public int NextId() => _memory.NextId();

    /// <inheritdoc/>
    public void Save()
    {
        // TODO TD-04: файл переписується цілком; два процеси одночасно зіпсують дані.
        List<BookingSnapshot> snapshots = GetAll().Select(b => b.ToSnapshot()).ToList();

        string? folder = Path.GetDirectoryName(Path.GetFullPath(_path));
        if (!string.IsNullOrEmpty(folder))
        {
            Directory.CreateDirectory(folder);
        }

        // Спершу пишемо у тимчасовий файл, щоб збій посеред запису не зіпсував дані.
        string temp = _path + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(snapshots, _options));
        File.Move(temp, _path, overwrite: true);
    }

    private void Load()
    {
        if (!File.Exists(_path))
        {
            return;
        }

        try
        {
            List<BookingSnapshot> snapshots = JsonSerializer.Deserialize<List<BookingSnapshot>>(
                File.ReadAllText(_path),
                _options) ?? new List<BookingSnapshot>();
            foreach (BookingSnapshot snapshot in snapshots)
            {
                _memory.Add(Booking.Restore(snapshot));
            }
        }
        catch (Exception ex)
            when (ex is JsonException or ArgumentException or InvalidOperationException)
        {
            throw new InvalidDataException($"Файл {_path} пошкоджений: {ex.Message}", ex);
        }
    }
}
