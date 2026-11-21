using HotelBooking.App;
using HotelBooking.Core.Domain;
using HotelBooking.Core.Pricing;
using HotelBooking.Core.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HotelBooking.Tests;

public class ConsoleMenuTests
{
    private readonly FakeBookingRepository _repository = new();

    [Fact]
    public void Run_ReservePayCheckIn_GoesThroughLifecycle()
    {
        string output = Run("2", "1", "101", "10.06.2026", "2", "3", "1", "3000", "4", "1", "0");

        Assert.Contains("Створено бронювання № 1", output);
        Assert.Matches(@"до сплати 3\s000,00 грн", output);
        Assert.Contains("заселено", output);
        Assert.Equal(BookingStatus.CheckedIn, _repository.GetById(1)!.Status);
    }

    [Fact]
    public void Run_DomainRuleBroken_ShowsRuleAndKeepsMenu()
    {
        string output = Run("4", "99", "0");

        Assert.Contains("Відмовлено (booking.exists)", output);
        Assert.Contains("До побачення.", output);
    }

    [Fact]
    public void Run_BadInputThreeTimes_ReturnsToMenu()
    {
        string output = Run("8", "x", "y", "z", "0");

        Assert.Contains("Повертаємось до меню", output);
    }

    [Fact]
    public void Run_UnknownItem_ExplainsChoices()
    {
        string output = Run("42", "0");

        Assert.Contains("Немає такого пункту", output);
    }

    private string Run(params string[] lines)
    {
        FakeHotelDirectory directory = FakeHotelDirectory.WithThreeRooms();
        MenuServices services = new(
            new ReservationService(
                _repository,
                directory,
                new FixedClock(new DateOnly(2026, 3, 1)),
                NullLogger<ReservationService>.Instance),
            new BookingService(
                _repository,
                new SeasonalPricingPolicy(),
                new SpyNotifier(),
                NullLogger<BookingService>.Instance),
            new BookingQueryService(_repository, directory));
        using StringReader input = new(string.Join(Environment.NewLine, lines));
        using StringWriter output = new();

        new ConsoleMenu(services, input, output).Run();

        return output.ToString();
    }
}
