using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using HotelBooking.Bench;
using HotelBooking.Core.Domain;
using HotelBooking.Core.Errors;
using HotelBooking.Core.Pricing;
using HotelBooking.Core.Services;
using HotelBooking.Core.Storage;
using Microsoft.Extensions.Logging.Abstractions;

// Семінар 2, тема 4. 100 000 невдалих оплат (правило booking.amount):
// Pay кидає DomainRuleException, PayAsResult повертає Result.Fail.
const int Operations = 100_000;
const int Rounds = 7;
const decimal TooLittle = 100m;

#if DEBUG
Console.WriteLine("УВАГА: збірка Debug. Запускай з -c Release.");
#endif

var repository = new InMemoryBookingRepository();
var service = new BookingService(
    repository,
    new StandardPricingPolicy(),
    new SilentNotifier(),
    NullLogger<BookingService>.Instance);

// Бронювання на 500 грн: дві ночі по 250. Сплачуємо 100 - завжди мало.
var booking = new Booking(1, 100, DateTimeOffset.Now);
booking.AddNight(new BookingNight("101", new DateOnly(2026, 11, 10), 250m));
booking.AddNight(new BookingNight("101", new DateOnly(2026, 11, 11), 250m));
repository.Add(booking);

// Прогрів: JIT компілює обидва шляхи ще до вимірювання.
_ = ViaException(service, 1_000);
_ = ViaResult(service, 1_000);

var exceptionMs = new List<double>();
var resultMs = new List<double>();
for (int round = 0; round < Rounds; round++)
{
    exceptionMs.Add(Measure(() => ViaException(service, Operations)));
    resultMs.Add(Measure(() => ViaResult(service, Operations)));
}

CultureInfo inv = CultureInfo.InvariantCulture;
Console.WriteLine($".NET: {RuntimeInformation.FrameworkDescription}");
Console.WriteLine($"ОС: {RuntimeInformation.OSDescription}");
Console.WriteLine($"Логічних ядер: {Environment.ProcessorCount}");
Console.WriteLine(string.Format(
    inv, "Операцій: {0}, раундів: {1}", Operations, Rounds));
Print("Виняток (Pay)", exceptionMs);
Print("Result (PayAsResult)", resultMs);
Console.WriteLine(string.Format(
    inv,
    "Виняток повільніший у {0:0.0} раза (за медіаною)",
    Median(exceptionMs) / Median(resultMs)));
Console.WriteLine(string.Format(
    inv,
    "Одна невдала операція: виняток {0:0.00} мкс, Result {1:0.000} мкс",
    Median(exceptionMs) * 1000 / Operations,
    Median(resultMs) * 1000 / Operations));

static int ViaException(BookingService service, int count)
{
    int failures = 0;
    for (int i = 0; i < count; i++)
    {
        try
        {
            service.Pay(1, TooLittle);
        }
        catch (DomainRuleException)
        {
            failures++;
        }
    }

    return failures;
}

static int ViaResult(BookingService service, int count)
{
    int failures = 0;
    for (int i = 0; i < count; i++)
    {
        Result<decimal> paid = service.PayAsResult(1, TooLittle);
        if (!paid.IsSuccess)
        {
            failures++;
        }
    }

    return failures;
}

static double Measure(Func<int> run)
{
    GC.Collect();
    GC.WaitForPendingFinalizers();
    GC.Collect();
    var watch = Stopwatch.StartNew();
    int failures = run();
    watch.Stop();

    // Захист від хибного вимірювання: усі операції мають бути невдалими.
    if (failures != Operations)
    {
        throw new InvalidOperationException(
            $"Очікувалось {Operations} невдач, отримано {failures}");
    }

    return watch.Elapsed.TotalMilliseconds;
}

static double Median(List<double> values)
{
    List<double> sorted = values.Order().ToList();
    return sorted[sorted.Count / 2];
}

static void Print(string name, List<double> values)
{
    Console.WriteLine(string.Format(
        CultureInfo.InvariantCulture,
        "{0}: медіана {1:0.0} мс, мін {2:0.0} мс, макс {3:0.0} мс",
        name,
        Median(values),
        values.Min(),
        values.Max()));
}
