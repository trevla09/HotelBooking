# Класова модель HotelBooking (ЛР4)

## Сутності

| Сутність | Роль | Незмінний | Інваріант |
|---|---|---|---|
| `BookingNight` | позиція документа: одна ніч у номері | так, `sealed record` | `RoomNumber` не порожній; `Rate >= 0` |
| `Booking` | документ зі станом (агрегат) | ні, агрегат | `Id > 0`; `GuestId > 0`; ночі додаються лише у стані `New`; немає двох ночей з однаковими датою й номером |
| `Guest` | довідник учасників | так, `sealed record` | `Id > 0`; `FullName` не порожнє; `Email` містить `@` |
| `Room` | довідник об'єктів обліку | так, `sealed record` | `Number` не порожній; `Capacity > 0`; `BaseRate >= 0` |

## Стани бронювання

| Зі стану | До стану | Метод | Умова |
|---|---|---|---|
| `New` | `Confirmed` | `Confirm` | є хоча б одна ніч |
| `New` | `Cancelled` | `Cancel` | завжди |
| `Confirmed` | `CheckedIn` | `CheckIn` | завжди |
| `Confirmed` | `Cancelled` | `Cancel` | завжди |
| `CheckedIn` | `CheckedOut` | `CheckOut` | завжди |

Будь-який інший перехід кидає `InvalidOperationException`.
