using System.Text.Json;
using System.Text.Json.Serialization;
using HotelBooking.Core.Abstractions;
using HotelBooking.Core.Domain;

namespace HotelBooking.Core.Infrastructure;

/// <summary>
/// Сховище бронювань у JSON-файлі. Читає файл один раз при створенні,
/// далі працює з пам'яттю і переписує файл цілком у <see cref="Save"/>.
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

    private readonly Dictionary<int, Booking> _bookings = new();

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
    /// <exception cref="ArgumentNullException">Бронювання не задано.</exception>
    /// <exception cref="InvalidOperationException">Бронювання з таким Id уже є.</exception>
    public void Add(Booking booking)
    {
        ArgumentNullException.ThrowIfNull(booking);
        if (!_bookings.TryAdd(booking.Id, booking))
        {
            throw new InvalidOperationException(
                $"Бронювання № {booking.Id} уже є у сховищі.");
        }
    }

    /// <inheritdoc/>
    public Booking? GetById(int id) =>
        _bookings.TryGetValue(id, out Booking? booking) ? booking : null;

    /// <inheritdoc/>
    public IReadOnlyList<Booking> GetAll() => _bookings.Values.OrderBy(b => b.Id).ToList();

    /// <inheritdoc/>
    public int NextId() => _bookings.Count == 0 ? 1 : _bookings.Keys.Max() + 1;

    /// <inheritdoc/>
    public void Save()
    {
        List<BookingSnapshot> snapshots = _bookings.Values
            .OrderBy(b => b.Id)
            .Select(b => b.ToSnapshot())
            .ToList();

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

        List<BookingSnapshot>? snapshots;
        try
        {
            snapshots = JsonSerializer.Deserialize<List<BookingSnapshot>>(
                File.ReadAllText(_path),
                _options);
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException($"Файл {_path} пошкоджений: {ex.Message}", ex);
        }

        foreach (BookingSnapshot snapshot in snapshots ?? new List<BookingSnapshot>())
        {
            Add(Booking.Restore(snapshot));
        }
    }
}
