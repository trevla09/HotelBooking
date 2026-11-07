namespace HotelBooking.Core.Bookings;

/// <summary>Перевірки передумов для розрахунку вартості бронювання.</summary>
internal static class BookingGuards
{
    /// <summary>Перевіряє весь запит на розрахунок.</summary>
    /// <param name="request">Запит на розрахунок.</param>
    /// <exception cref="ArgumentNullException">Запит або гостя не задано.</exception>
    /// <exception cref="ArgumentException">Дані запиту некоректні.</exception>
    public static void EnsureRequestValid(TotalRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Guest);
        EnsureHeaderValid(
            request.BookingId,
            request.Guest.Name,
            request.Guest.Email);
        EnsureLinesValid(request.Lines);
        ArgumentOutOfRangeException.ThrowIfNegative(request.TransferPrice);
    }

    /// <summary>Перевіряє заголовок бронювання: номер і контакти гостя.</summary>
    /// <param name="bookingId">Номер бронювання, більший за нуль.</param>
    /// <param name="guestName">Ім'я гостя.</param>
    /// <param name="guestEmail">Пошта гостя, має містити символ @.</param>
    /// <exception cref="ArgumentOutOfRangeException">Номер не більший за нуль.</exception>
    /// <exception cref="ArgumentException">
    /// Ім'я або пошта порожні чи некоректні.</exception>
    public static void EnsureHeaderValid(
        int bookingId,
        string guestName,
        string guestEmail)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bookingId);
        ArgumentException.ThrowIfNullOrWhiteSpace(guestName);
        ArgumentException.ThrowIfNullOrWhiteSpace(guestEmail);
        if (!guestEmail.Contains('@', StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "Пошта має містити символ @.",
                nameof(guestEmail));
        }
    }

    /// <summary>Перевіряє один рядок бронювання.</summary>
    /// <param name="line">Рядок бронювання.</param>
    /// <exception cref="ArgumentNullException">Рядок не задано.</exception>
    /// <exception cref="ArgumentException">Номер кімнати порожній.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Ночей не більше нуля або тариф від'ємний.</exception>
    public static void EnsureLineValid(BookingLine line)
    {
        ArgumentNullException.ThrowIfNull(line);
        ArgumentException.ThrowIfNullOrWhiteSpace(line.RoomNumber);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(
            line.Nights,
            nameof(line.Nights));
        ArgumentOutOfRangeException.ThrowIfNegative(
            line.NightlyRate,
            nameof(line.NightlyRate));
    }

    /// <summary>Перевіряє колекцію рядків бронювання.</summary>
    /// <param name="lines">Рядки бронювання, хоча б один.</param>
    /// <exception cref="ArgumentNullException">Колекцію не задано.</exception>
    /// <exception cref="ArgumentException">У бронюванні немає жодного рядка.</exception>
    public static void EnsureLinesValid(IReadOnlyList<BookingLine> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);
        if (lines.Count == 0)
        {
            throw new ArgumentException(
                "Бронювання не містить жодного номера.",
                nameof(lines));
        }

        foreach (BookingLine line in lines)
        {
            EnsureLineValid(line);
        }
    }
}
