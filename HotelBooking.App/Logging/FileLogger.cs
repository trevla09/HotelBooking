using System.Globalization;
using Microsoft.Extensions.Logging;

namespace HotelBooking.App.Logging;

/// <summary>
/// Логер однієї категорії. Формат рядка:
/// час [РІВЕНЬ] категорія {область сценарію}: повідомлення.
/// </summary>
internal sealed class FileLogger : ILogger
{
    private readonly string _category;
    private readonly FileLoggerProvider _provider;

    /// <summary>Створює логер для категорії.</summary>
    /// <param name="category">Категорія, зазвичай повне ім'я класу.</param>
    /// <param name="provider">Постачальник, що володіє файлом.</param>
    public FileLogger(string category, FileLoggerProvider provider)
    {
        _category = category;
        _provider = provider;
    }

    /// <inheritdoc/>
    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull => _provider.PushScope(state.ToString() ?? string.Empty);

    /// <inheritdoc/>
    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    /// <inheritdoc/>
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        ArgumentNullException.ThrowIfNull(formatter);
        if (!IsEnabled(logLevel))
        {
            return;
        }

        string scope = _provider.DescribeScope();
        string line = string.Format(
            CultureInfo.InvariantCulture,
            "{0:yyyy-MM-dd HH:mm:ss} [{1}] {2}{3}: {4}",
            DateTimeOffset.Now,
            logLevel.ToString().ToUpperInvariant(),
            _category,
            scope.Length == 0 ? string.Empty : " {" + scope + "}",
            formatter(state, exception));
        if (exception is not null)
        {
            line += Environment.NewLine + exception;
        }

        _provider.Append(line);
    }
}
