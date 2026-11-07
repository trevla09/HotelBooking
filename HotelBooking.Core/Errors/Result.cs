using System.Diagnostics.CodeAnalysis;

namespace HotelBooking.Core.Errors;

/// <summary>Результат операції: або значення, або пояснення помилки.</summary>
/// <typeparam name="T">Тип значення.</typeparam>
[SuppressMessage(
    "Design",
    "CA1000:Do not declare static members on generic types",
    Justification = "Фабрики Ok і Fail - API типу-результату з методички ЛР5.")]
public readonly struct Result<T>
{
    private Result(bool ok, T? value, string error)
    {
        IsSuccess = ok;
        Value = value;
        Error = error;
    }

    /// <summary>Чи успішна операція.</summary>
    public bool IsSuccess { get; }

    /// <summary>Значення; задане лише в разі успіху.</summary>
    public T? Value { get; }

    /// <summary>Пояснення помилки; порожнє в разі успіху.</summary>
    public string Error { get; }

    /// <summary>Створює успішний результат.</summary>
    /// <param name="value">Значення.</param>
    /// <returns>Успішний результат.</returns>
    public static Result<T> Ok(T value) => new(true, value, string.Empty);

    /// <summary>Створює результат з помилкою.</summary>
    /// <param name="error">Пояснення для користувача.</param>
    /// <returns>Неуспішний результат.</returns>
    public static Result<T> Fail(string error) => new(false, default, error);
}
