using System.Text;
using HotelBooking.App;
using HotelBooking.App.Logging;
using HotelBooking.Core;
using HotelBooking.Core.Infrastructure;
using HotelBooking.Core.Pricing;
using HotelBooking.Core.Services;
using Microsoft.Extensions.Logging;

// Композиційний корінь: єдине місце, де створюються конкретні реалізації
// і де вирішується, звідки брати дані. Решта коду знає лише інтерфейси.
Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;
Console.WriteLine($"HotelBooking v{VersionInfo.Current()}");

string dataFolder = args.Length > 0
    ? args[0]
    : Path.Combine(AppContext.BaseDirectory, "data");
string logPath = Path.Combine(AppContext.BaseDirectory, "logs", "app.log");

using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddProvider(new FileLoggerProvider(logPath));
    builder.SetMinimumLevel(LogLevel.Debug);
});
ILogger log = loggerFactory.CreateLogger("HotelBooking.App");

// Верхній обробник: єдине місце, де дозволено catch (Exception).
try
{
    var directory = new JsonHotelDirectory(
        Path.Combine(dataFolder, "rooms.json"),
        Path.Combine(dataFolder, "guests.json"));
    var repository = new JsonBookingRepository(Path.Combine(dataFolder, "bookings.json"));
    var clock = new SystemClock();

    var services = new MenuServices(
        new ReservationService(
            repository,
            directory,
            clock,
            loggerFactory.CreateLogger<ReservationService>()),
        new BookingService(
            repository,
            new SeasonalPricingPolicy(),
            new ConsoleNotifier(),
            loggerFactory.CreateLogger<BookingService>()),
        new BookingQueryService(repository, directory));

    log.LogInformation(
        "Старт: {Rooms} номерів, {Guests} гостей, {Bookings} бронювань з {Folder}",
        directory.Rooms.Count,
        directory.Guests.Count,
        repository.GetAll().Count,
        dataFolder);
    Console.WriteLine(
        $"Завантажено: {directory.Rooms.Count} номерів, {directory.Guests.Count} гостей, " +
        $"{repository.GetAll().Count} бронювань. Журнал: {logPath}");

    new ConsoleMenu(services, Console.In, Console.Out).Run();
    return 0;
}
catch (Exception ex) when (ex is IOException or InvalidDataException)
{
    log.LogError(ex, "Дані недоступні");
    Console.WriteLine($"Не вдалося прочитати дані: {ex.Message}");
    return 1;
}
catch (Exception ex)
{
    log.LogCritical(ex, "Непередбачений збій");
    Console.WriteLine("Непередбачена помилка, подробиці в журналі.");
    return 2;
}
