using System.Globalization;
using System.Text;
using HotelBooking.App;
using HotelBooking.App.Input;
using HotelBooking.Core.Abstractions;
using HotelBooking.Core.Bookings;
using HotelBooking.Core.Domain;
using HotelBooking.Core.Errors;
using HotelBooking.Core.Pricing;
using HotelBooking.Core.Reporting;
using HotelBooking.Core.Services;
using HotelBooking.Core.Storage;

Console.OutputEncoding = Encoding.UTF8;

var reservation = new Reservation("B-1001", "Іваненко");
reservation.AddLine("101", 3, 250m);
reservation.AddLine("204", 12, 90m);

Console.WriteLine(reservation.CalculateTotal(false));
Console.WriteLine(reservation.CalculateTotal(true));
Console.WriteLine(reservation.IsValid());
Console.WriteLine(reservation.TryChangeStatus(1));
Console.Write(reservation.BuildReport());

TotalRequest request = BuildSampleRequest();
TotalResult result = BookingTotalCalculator.Calculate(request);
string report = BookingReport.TextOf(request, result);
new BookingReportPrinter(Console.Out).Print(report);

// ЛР4. Композиційний корінь: єдине місце, де створюються конкретні реалізації.
CultureInfo ukrainian = CultureInfo.GetCultureInfo("uk-UA");
var policies = new Dictionary<string, IPricingPolicy>
{
    ["regular"] = new DiscountPricingPolicy(0.05m),
    ["corporate"] = new CorporatePricingPolicy(),
    ["staff"] = new DiscountPricingPolicy(0.30m),
};
string guestType = "regular";
IPricingPolicy pricing = policies.TryGetValue(guestType, out IPricingPolicy? found)
    ? found
    : new StandardPricingPolicy();

var repository = new InMemoryBookingRepository();
var service = new BookingService(repository, pricing, new ConsoleNotifier());

var booking = new Booking(1, 100, DateTimeOffset.Now);
booking.AddNight(new BookingNight("101", new DateOnly(2026, 11, 10), 250m));
booking.AddNight(new BookingNight("101", new DateOnly(2026, 11, 11), 250m));
service.Place(booking);

Console.WriteLine(string.Format(ukrainian, "Сума: {0:0.00}", service.TotalOf(booking)));
Console.Write(BookingCsvReport.BuildCsv(repository.GetAll()));
var archived = new ArchivedBooking(booking, new DateOnly(2026, 11, 12));
Console.WriteLine($"В архіві: бронювання № {archived.Id}, ночей {archived.Nights.Count}");

// ЛР5. Введення ночей через барикаду: помилка вводу - пояснення й повторний запит.
const int InputBookingId = 2;
service.Place(new Booking(InputBookingId, 100, DateTimeOffset.Now));
Console.WriteLine($"Ночі для бронювання № {InputBookingId} (порожній номер кімнати - завершити).");
while (true)
{
    Console.Write("Номер кімнати: ");
    string? room = Console.ReadLine();
    if (string.IsNullOrEmpty(room))
    {
        Console.WriteLine("Введення завершено.");
        break;
    }

    Console.Write("Дата (дд.мм.рррр): ");
    string? date = Console.ReadLine();
    Console.Write("Тариф, грн: ");
    string? rate = Console.ReadLine();

    Result<BookingNight> parsed = BookingNightParser.Parse(room, date, rate);
    if (!parsed.IsSuccess)
    {
        Console.WriteLine($"Помилка вводу: {parsed.Error}");
        continue;
    }

    try
    {
        service.AddNight(InputBookingId, parsed.Value!);
        Console.WriteLine("Ніч додано.");
    }
    catch (DomainRuleException ex)
    {
        Console.WriteLine($"Не додано: {ex.Message}");
    }
}

static TotalRequest BuildSampleRequest()
{
    List<BookingLine> lines = new()
    {
        new() { RoomNumber = "101", Nights = 12, NightlyRate = 100m },
        new() { RoomNumber = "204", Nights = 1, NightlyRate = 250m },
    };
    return new TotalRequest(
        BookingId: 1,
        Guest: new GuestInfo("Іваненко", "i@ex.com", true),
        Lines: lines,
        CreatedAt: new DateOnly(2026, 3, 10),
        CouponCode: null,
        Status: BookingStatus.New,
        TransferPrice: 60m);
}
