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

Правила описано у файлі [CODING_STANDARD.md](CODING_STANDARD.md) і закріплено
в `.editorconfig`. Перевірка: `dotnet format --verify-no-changes`

## Автор

Ізюменко Тимофій, група 471