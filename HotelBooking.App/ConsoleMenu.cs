using System.Globalization;
using HotelBooking.App.Input;
using HotelBooking.Core.Domain;
using HotelBooking.Core.Errors;
using HotelBooking.Core.Reporting;
using HotelBooking.Core.Services;

namespace HotelBooking.App;

/// <summary>
/// Текстове меню адміністратора. Читає з <see cref="TextReader"/> і пише в
/// <see cref="TextWriter"/>, тому в тестах замість консолі підставляють рядки.
/// </summary>
internal sealed class ConsoleMenu
{
    private const int MaxAttempts = 3;

    private readonly MenuServices _services;
    private readonly TextReader _input;
    private readonly TextWriter _output;
    private readonly CultureInfo _culture = CultureInfo.GetCultureInfo("uk-UA");

    /// <summary>Створює меню.</summary>
    /// <param name="services">Сервіси застосунку.</param>
    /// <param name="input">Звідки читати відповіді.</param>
    /// <param name="output">Куди писати.</param>
    public ConsoleMenu(MenuServices services, TextReader input, TextWriter output)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(output);
        _services = services;
        _input = input;
        _output = output;
    }

    /// <summary>Показує меню, поки користувач не вибере 0 або не закінчиться ввід.</summary>
    public void Run()
    {
        while (true)
        {
            PrintMenu();
            string? choice = Ask("Ваш вибір");
            if (choice is null || choice == "0")
            {
                _output.WriteLine("До побачення.");
                return;
            }

            Execute(choice);
        }
    }

    private void PrintMenu()
    {
        _output.WriteLine();
        _output.WriteLine("=== HotelBooking ===");
        _output.WriteLine("1. Вільні номери на період");
        _output.WriteLine("2. Забронювати номер (UC-01)");
        _output.WriteLine("3. Оплатити бронювання (UC-03)");
        _output.WriteLine("4. Заселити гостя (UC-02)");
        _output.WriteLine("5. Скасувати бронювання (UC-02)");
        _output.WriteLine("6. Виселити гостя (UC-04)");
        _output.WriteLine("7. Знайти бронювання (UC-05)");
        _output.WriteLine("8. Заповнюваність за тиждень (UC-06)");
        _output.WriteLine("0. Вихід");
    }

    private void Execute(string choice)
    {
        Action? action = choice switch
        {
            "1" => ShowFreeRooms,
            "2" => Reserve,
            "3" => Pay,
            "4" => CheckIn,
            "5" => Cancel,
            "6" => CheckOut,
            "7" => Search,
            "8" => WeeklyReport,
            _ => null,
        };
        if (action is null)
        {
            _output.WriteLine("Немає такого пункту, введіть число від 0 до 8.");
            return;
        }

        try
        {
            action();
        }
        catch (DomainRuleException ex)
        {
            _output.WriteLine($"Відмовлено ({ex.Rule}): {ex.Message}");
        }
    }

    private void ShowFreeRooms()
    {
        Result<StayPeriod> parsed = Retry(() => ReservationInputParser.ParsePeriod(
            Ask("Дата заїзду (дд.мм.рррр)"),
            Ask("Кількість ночей")));
        if (!parsed.IsSuccess)
        {
            return;
        }

        StayPeriod period = parsed.Value!;
        IReadOnlyList<Room> free = _services.Reservations.FreeRooms(period);
        _output.WriteLine(
            $"Вільно {free.Count} номерів з {Format(period.CheckIn)} " +
            $"по {Format(period.CheckOut)}:");
        foreach (Room room in free)
        {
            _output.WriteLine(string.Format(
                _culture,
                "  {0,-5} {1,-9} місць {2}  {3:N2} грн/ніч",
                room.Number,
                room.Category,
                room.Capacity,
                room.BaseRate));
        }
    }

    private void Reserve()
    {
        Result<ReserveRequest> parsed = Retry(() => ReservationInputParser.ParseReserve(
            Ask("Ідентифікатор гостя"),
            Ask("Номер кімнати"),
            Ask("Дата заїзду (дд.мм.рррр)"),
            Ask("Кількість ночей")));
        if (!parsed.IsSuccess)
        {
            return;
        }

        ReserveRequest form = parsed.Value!;
        Booking booking = _services.Reservations.Reserve(form);
        _output.WriteLine(string.Format(
            _culture,
            "Створено бронювання № {0}: номер {1}, ночей {2}, до сплати {3:N2} грн.",
            booking.Id,
            form.RoomNumber,
            form.Period.Nights,
            _services.Bookings.TotalOf(booking)));
    }

    private void Pay()
    {
        int? id = AskBookingId();
        if (id is null)
        {
            return;
        }

        decimal total = _services.Bookings.TotalOf(_services.Bookings.Load(id.Value));
        string? answer = Ask(string.Format(_culture, "До сплати {0:N2} грн. Сума", total));
        if (!decimal.TryParse(answer, NumberStyles.Number, _culture, out decimal amount))
        {
            _output.WriteLine("Сума - число, дробова частина через кому.");
            return;
        }

        _services.Bookings.Pay(id.Value, amount);
        _output.WriteLine($"Бронювання № {id} оплачено і підтверджено.");
    }

    private void CheckIn()
    {
        int? id = AskBookingId();
        if (id is not null)
        {
            _services.Bookings.CheckIn(id.Value);
            _output.WriteLine($"Гостя за бронюванням № {id} заселено.");
        }
    }

    private void Cancel()
    {
        int? id = AskBookingId();
        if (id is null)
        {
            return;
        }

        string? reason = Ask("Причина");
        _services.Bookings.Cancel(
            id.Value,
            string.IsNullOrWhiteSpace(reason) ? "без пояснення" : reason.Trim());
        _output.WriteLine($"Бронювання № {id} скасовано.");
    }

    private void CheckOut()
    {
        int? id = AskBookingId();
        if (id is not null)
        {
            decimal total = _services.Bookings.CheckOut(id.Value);
            _output.WriteLine(string.Format(
                _culture,
                "Гостя виселено. Підсумок за проживання {0:N2} грн.",
                total));
        }
    }

    private void Search()
    {
        _output.WriteLine("Порожнє поле - без цієї умови.");
        Result<BookingSearch> parsed = Retry(() => ReservationInputParser.ParseSearch(
            Ask("Частина імені гостя"),
            Ask("Дата ночі (дд.мм.рррр)"),
            Ask("Стан (New/Confirmed/CheckedIn/CheckedOut/Cancelled)")));
        if (!parsed.IsSuccess)
        {
            return;
        }

        IReadOnlyList<BookingView> found = _services.Queries.Search(parsed.Value!);
        _output.WriteLine($"Знайдено: {found.Count}");
        foreach (BookingView view in found)
        {
            _output.WriteLine(
                $"  № {view.Id,-4} {view.GuestName,-20} {view.Rooms,-8} " +
                $"{Format(view.CheckIn)} - {Format(view.CheckOut)}  {view.Status}");
        }
    }

    private void WeeklyReport()
    {
        Result<DateOnly> parsed = Retry(
            () => ReservationInputParser.ParseDate(Ask("Перший день тижня (дд.мм.рррр)")));
        if (!parsed.IsSuccess)
        {
            return;
        }

        OccupancyReport report = _services.Queries.WeeklyOccupancy(parsed.Value);
        _output.WriteLine($"Заповнюваність з {Format(report.FirstDay)}:");
        foreach (OccupancyDay day in report.Days)
        {
            _output.WriteLine(string.Format(
                _culture,
                "  {0:dd.MM ddd}  {1,3} з {2}  {3,5:0.0} %",
                day.Date,
                day.OccupiedRooms,
                day.TotalRooms,
                day.Percent));
        }

        _output.WriteLine(string.Format(_culture, "Середня: {0:0.0} %", report.AveragePercent));
    }

    private int? AskBookingId()
    {
        Result<int> parsed = Retry(
            () => ReservationInputParser.ParseBookingId(Ask("Номер бронювання")));
        return parsed.IsSuccess ? parsed.Value : null;
    }

    private Result<T> Retry<T>(Func<Result<T>> read)
    {
        Result<T> result = read();
        for (int attempt = 1; !result.IsSuccess && attempt < MaxAttempts; attempt++)
        {
            _output.WriteLine($"Помилка вводу: {result.Error}. Спробуйте ще раз.");
            result = read();
        }

        if (!result.IsSuccess)
        {
            _output.WriteLine($"Помилка вводу: {result.Error}. Повертаємось до меню.");
        }

        return result;
    }

    private string? Ask(string prompt)
    {
        _output.Write($"{prompt}: ");
        return _input.ReadLine()?.Trim();
    }

    private string Format(DateOnly date) => date.ToString("dd.MM.yyyy", _culture);
}
