# HotelBooking

![CI](https://github.com/trevla09/HotelBooking/actions/workflows/ci.yml/badge.svg)

Навчальний проєкт з дисципліни «Конструювання програмного забезпечення».
Варіант № 8: бронювання номерів у готелі.

## Склад рішення

- HotelBooking.Core — логіка предметної області (бібліотека класів)
- HotelBooking.App — консольний застосунок (введення, виведення)
- HotelBooking.Tests — модульні тести (xUnit)

## Вимоги

.NET SDK 8.0 (версію закріплено в `global.json`), Git.

## Складання та запуск

```
dotnet build
dotnet run --project HotelBooking.App
```

## Стандарт кодування

Правила (версія 2.0.0) описано у файлі [CODING_STANDARD.md](CODING_STANDARD.md) і закріплено
в `.editorconfig` та `Directory.Build.props`. Чекліст огляду і журнал рішень - у `docs/standard/`.

Перевірка перед комітом:

```
dotnet format --verify-no-changes
dotnet build -c Release
dotnet test
```

## Автор

Ізюменко Тимофій, група 471

## Обробка помилок (ЛР5)

Класифікація помилок - у `docs/errors.md`. Барикада проходить між консоллю
і ядром: рядки розбирає `HotelBooking.App/Input/BookingNightParser` і повертає
`Result<BookingNight>`, а ядро приймає лише типізовані значення. Порушення
правил предметної області - `DomainRuleException` з ідентифікатором правила.
Журнал пише `ILogger` (Microsoft.Extensions.Logging), аудит операцій -
файл `audit.log` (у Git не потрапляє).

## Безперервна інтеграція (ЛР8)

Конвеєр `.github/workflows/ci.yml` запускається на кожен push у `main` і `feature/**`
та на кожен pull request у `main`. Зміна потрапляє в `main` лише тоді, коли пройдено
всі критерії якості:

| Крок | Команда | Що ламає збірку |
|---|---|---|
| Формат | `dotnet format --verify-no-changes` | зайвий пробіл, відступ, кодування файла |
| Складання | `dotnet build -c Release` | будь-яке попередження CS, CA, IDE, SA |
| Тести | `dotnet test -c Release` | хоча б один червоний тест |
| Покриття | `/p:Threshold=80` | покриття рядків нижче за 80 % |

Версія збірки задається у `Directory.Build.props` (`VersionPrefix`) і виводиться першим
рядком застосунку. Під час restore працює NuGetAudit: вразливий пакет ламає складання.
