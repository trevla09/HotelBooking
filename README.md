# HotelBooking

![CI](https://github.com/trevla09/HotelBooking/actions/workflows/ci.yml/badge.svg)

Консольний застосунок адміністратора готелю: бронює номери на період, рахує ціну
ночі за сезоном, веде бронювання від створення до виселення і показує
заповнюваність за тиждень.

## Автор і варіант

Ізюменко Тимофій Романович, група 471, варіант № 8 (HotelBooking).
Дисципліна «Конструювання програмного забезпечення», самостійна робота № 3.

## Швидкий старт

Потрібен .NET SDK 8.0 (версію закріплено в `global.json`) і Git.

```
git clone https://github.com/trevla09/HotelBooking.git
dotnet build HotelBooking
dotnet run --project HotelBooking/HotelBooking.App
```

Дані (40 номерів, 80 умовних гостей, 60 бронювань) лежать у теці `data` і
копіюються поруч із виконуваним файлом при складанні.

## Структура рішення

- `HotelBooking.Core` - бібліотека логіки: домен (`Domain`), сервіси сценаріїв
  (`Services`), політики ціни (`Pricing`), звіт (`Reporting`), інтерфейси
  (`Abstractions`) і JSON-сховища (`Infrastructure`).
- `HotelBooking.App` - консольне меню, барикада вводу (`Input`), журнал у файл
  (`Logging`) і композиційний корінь `Program.cs`.
- `HotelBooking.Tests` - модульні тести xUnit з ручними дублерами: фейкове сховище,
  фейковий довідник, фіксований годинник.

Шари і правило залежностей описано в [docs/architecture.md](docs/architecture.md).

## Можливості

Сценарії детально описано в [docs/spec.md](docs/spec.md), прохідні приклади - у
[docs/user-guide.md](docs/user-guide.md).

- UC-01 Забронювати номер: перевірка вільності за напіввідкритим інтервалом `[заїзд; виїзд)`.
- UC-02 Заселити або скасувати бронювання.
- UC-03 Оплатити бронювання (сума за сезонними коефіцієнтами 1,00 / 1,20 / 1,50 / 1,80).
- UC-04 Виселити гостя з підсумком.
- UC-05 Знайти бронювання за частиною імені, датою ночі і станом.
- UC-06 Звіт «Заповнюваність за тиждень».

## Тести і покриття

```
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
reportgenerator -reports:HotelBooking.Tests/coverage.cobertura.xml -targetdir:coverage-report
```

Покриття рахується за `HotelBooking.Core` без теки `Infrastructure` і за
`HotelBooking.App.Input` (межу задано в `HotelBooking.Tests.csproj`, властивості
`Include` і `Exclude`). Поточне значення: рядкове XX,X %, за гілками XX,X %.
Тестових випадків - 178.

Те саме, що перевіряє конвеєр, локально однією командою:

```
dotnet format --verify-no-changes && dotnet build -c Release -warnaserror && dotnet test -c Release /p:CollectCoverage=true /p:Threshold=70 /p:ThresholdType=line
```

Конвеєр `.github/workflows/ci.yml` запускається на push у будь-яку гілку і на pull
request у `main`: формат, складання з `-warnaserror`, тести, поріг 70 %,
публікація `coverage.cobertura.xml` як артефакту.

## Технології

.NET 8, C# 12, xUnit 2.6, coverlet, аналізатори Roslyn (CA, IDE) і StyleCop.Analyzers,
System.Text.Json, Microsoft.Extensions.Logging, GitHub Actions.

## Обмеження і відомий борг

Програма розрахована на одного адміністратора: якщо запустити два екземпляри з
однією текою `data`, вони перезапишуть зміни один одного (TD-04). Бронювання займає
один номер; кілька номерів в одному бронюванні модель дозволяє, але меню такого
не вміє. Ціна рахується щоразу за поточною таблицею сезонів, тож зміна коефіцієнта
змінить суму вже створених неоплачених бронювань. У `Core` лишився код
лабораторних ЛР1-ЛР7, який меню не використовує (TD-01). Повний реєстр боргу - у
[docs/self-check.md](docs/self-check.md), рішення - у [docs/decisions.md](docs/decisions.md).
