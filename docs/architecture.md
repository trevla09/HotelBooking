# Архітектура HotelBooking

Рішення складається з трьох проєктів. Шари всередині `HotelBooking.Core` розкладено
по теках (варіант 1 із завдання). Чому не чотири проєкти - див. запис D-05 у
`docs/decisions.md`.

## Таблиця шарів

| Шар | Де лежить | Що містить | Що йому заборонено |
|---|---|---|---|
| Домен | `Core/Domain`, `Core/Errors` | `Booking`, `BookingNight`, `StayPeriod`, `Room`, `Guest`, `BookingStatus`, `SeasonCalendar`, `DomainRuleException`, `Result<T>` | знати про файли, консоль, JSON, журнал |
| Прикладний | `Core/Abstractions`, `Core/Services`, `Core/Pricing`, `Core/Reporting` | `ReservationService`, `BookingService`, `BookingQueryService`, інтерфейси `IBookingRepository`, `IHotelDirectory`, `IClock`, `IPricingPolicy`, `INotifier`, `SeasonalPricingPolicy`, рядки звіту | знати формат зберігання і консоль |
| Інфраструктура | `Core/Infrastructure` | `JsonBookingRepository`, `JsonHotelDirectory`, `InMemoryBookingRepository`, `SystemClock` | містити правила домену |
| Застосунок | `HotelBooking.App` | `Program.cs` (композиційний корінь), `ConsoleMenu`, барикада `Input/*Parser`, `Logging/FileLogger*`, `ConsoleNotifier` | рахувати гроші й перевіряти правила |

Логування (`ILogger`) прикладний шар отримує через конструктор. Конкретний
постачальник журналу (`FileLoggerProvider`) живе в застосунку.

## Рисунок 1 - Схема залежностей

```
 HotelBooking.App ──────► Services / Pricing / Reporting ──────► Domain
   (меню, корінь)          (сценарії, інтерфейси)                (сутності, правила)
        │                          ▲
        │                          │ реалізує IBookingRepository, IHotelDirectory, IClock
        └──────────────► Infrastructure (JSON, системний годинник)

 HotelBooking.Tests ───► усе вище (фейки замість Infrastructure)
```

- App → прикладний шар: меню викликає лише методи сервісів і нічого не рахує саме.
- Прикладний → Domain: сервіси створюють `Booking` і змінюють його стан тільки методами-переходами.
- Infrastructure → прикладний: JSON-сховище реалізує інтерфейс, оголошений в `Abstractions`, а не навпаки.
- App → Infrastructure: тільки в `Program.cs`, де створюються конкретні класи.
- Tests → усе: тести підставляють `FakeBookingRepository`, `FakeHotelDirectory`, `FixedClock`.

Оскільки домен і сервіси лежать в одному проєкті з інфраструктурою, компілятор
правило не стереже. Його стереже тест `ArchitectureTests.Layer_DoesNotDependOn`: він
рефлексією обходить поля, властивості й сигнатури методів типів кожного шару і падає,
якщо, наприклад, у `Domain` з'являється тип із `System.IO`. Перевіряв навмисно:
додав у `Booking` метод з параметром `FileInfo` - тест почервонів з повідомленням
`Booking -> System.IO.FileInfo`.

## Композиційний корінь

`HotelBooking.App/Program.cs`, верхні оператори:

```csharp
var directory = new JsonHotelDirectory(
    Path.Combine(dataFolder, "rooms.json"),
    Path.Combine(dataFolder, "guests.json"));
var repository = new JsonBookingRepository(Path.Combine(dataFolder, "bookings.json"));
var clock = new SystemClock();

var services = new MenuServices(
    new ReservationService(repository, directory, clock,
        loggerFactory.CreateLogger<ReservationService>()),
    new BookingService(repository, new SeasonalPricingPolicy(), new ConsoleNotifier(),
        loggerFactory.CreateLogger<BookingService>()),
    new BookingQueryService(repository, directory));

new ConsoleMenu(services, Console.In, Console.Out).Run();
```

Усі `new` для конкретних реалізацій зібрано тут. Через це сервіси можна тестувати
з фейками, а заміна JSON на щось інше зачіпає один файл. Одне й те саме сховище
передається трьом сервісам, тож вони бачать ті самі бронювання.

## Точки розширення

| Зміна | Куди вносити | Механізм |
|---|---|---|
| Нова схема ціни (знижка постійним, ціна вихідного дня) | новий клас `IPricingPolicy`, підключити в `Program.cs` | стратегія; `BookingService` не змінюється |
| Новий сезон або інші межі сезонів | рядок у `SeasonCalendar.SeasonOf` і значення в `Season` / `FactorOf` | таблиця рішень 4.1 зі специфікації; тести-таблиця `SeasonalPricingPolicyTests` |
| Інше сховище (CSV, SQLite) | новий клас `IBookingRepository` / `IHotelDirectory` у `Infrastructure` | інтерфейс сховища; сервіси й меню не змінюються |
| Підмінний час (наприклад, «завтра» для демонстрації) | нова реалізація `IClock` | інтерфейс; уже використано в тестах через `FixedClock` |
