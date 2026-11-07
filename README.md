# HotelBooking

Навчальний проєкт з дисципліни «Конструювання програмного забезпечення».
Варіант № 8: бронювання номерів у готелі.

## Склад рішення

- HotelBooking.Core — логіка предметної області (бібліотека класів)
- HotelBooking.App — консольний застосунок (введення, виведення)

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
dotnet build -warnaserror
```

## Автор

Ізюменко Тимофій, група 471