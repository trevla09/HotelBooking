using System.Diagnostics.CodeAnalysis;

namespace HotelBooking.Core.Errors;

/// <summary>Результат операції: або значення, або код і пояснення помилки.</summary>
/// <typeparam name="T">Тип значення.</typeparam>
[SuppressMessage(
    "Design",
    "CA1000:Do not declare static members on generic types",
    Justification = "Фабрики Ok і Fail - API типу-результату з методички ЛР5.")]
public readonly struct Result<T>
{
    private Result(bool ok, T? value, string code, string error)
    {
        IsSuccess = ok;
        Value = value;
        Code = code;
        Error = error;
    }

    /// <summary>Чи успішна операція.</summary>
    public bool IsSuccess { get; }

    /// <summary>Значення; задане лише в разі успіху.</summary>
    public T? Value { get; }

    /// <summary>Код помилки, наприклад booking.amount; порожній у разі успіху.</summary>
    public string Code { get; }

    /// <summary>Пояснення помилки; порожнє в разі успіху.</summary>
    public string Error { get; }

    /// <summary>Створює успішний результат.</summary>
    /// <param name="value">Значення.</param>
    /// <returns>Успішний результат.</returns>
    public static Result<T> Ok(T value) =>
        new(true, value, string.Empty, string.Empty);

    /// <summary>Створює результат з помилкою вводу (код input).</summary>
    /// <param name="error">Пояснення для користувача.</param>
    /// <returns>Неуспішний результат.</returns>
    public static Result<T> Fail(string error) => Fail("input", error);

    /// <summary>Створює результат з кодом порушеного правила.</summary>
    /// <param name="code">Код правила, наприклад booking.amount.</param>
    /// <param name="error">Пояснення для користувача.</param>
    /// <returns>Неуспішний результат.</returns>
    public static Result<T> Fail(string code, string error) =>
        new(false, default, code, error);
}
