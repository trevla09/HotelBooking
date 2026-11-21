using Microsoft.Extensions.Logging;

namespace HotelBooking.App.Logging;

/// <summary>
/// Постачальник журналу у файл. Один потік на весь застосунок,
/// усі логери пишуть через нього під замком.
/// </summary>
internal sealed class FileLoggerProvider : ILoggerProvider
{
    private readonly StreamWriter _writer;
    private readonly object _gate = new();
    private readonly AsyncLocal<LogScope?> _scope = new();
    private bool _disposed;

    /// <summary>Відкриває файл журналу для дописування, створює теку за потреби.</summary>
    /// <param name="path">Шлях до файлу журналу.</param>
    /// <exception cref="IOException">Файл недоступний.</exception>
    public FileLoggerProvider(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        string? folder = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(folder))
        {
            Directory.CreateDirectory(folder);
        }

        _writer = new StreamWriter(path, append: true) { AutoFlush = true };
    }

    /// <inheritdoc/>
    public ILogger CreateLogger(string categoryName) => new FileLogger(categoryName, this);

    /// <summary>Закриває файл журналу. Повторний виклик нічого не робить.</summary>
    public void Dispose()
    {
        lock (_gate)
        {
            if (_disposed)
            {
                return;
            }

            _writer.Dispose();
            _disposed = true;
        }
    }

    /// <summary>Відкриває область журналу для всіх логерів цього постачальника.</summary>
    /// <param name="text">Текст області.</param>
    /// <returns>Область; Dispose її закриває.</returns>
    internal LogScope PushScope(string text) => new(_scope, text);

    /// <summary>Описує відкриті області.</summary>
    /// <returns>Ланцюжок областей або порожній рядок.</returns>
    internal string DescribeScope() => _scope.Value?.Chain() ?? string.Empty;

    /// <summary>Дописує готовий рядок у файл.</summary>
    /// <param name="line">Рядок журналу.</param>
    internal void Append(string line)
    {
        lock (_gate)
        {
            if (!_disposed)
            {
                _writer.WriteLine(line);
            }
        }
    }
}
