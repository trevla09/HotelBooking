using System.Globalization;

namespace HotelBooking.App;

/// <summary>Журнал аудиту у файлі: операція, бронювання, час. Володіє потоком.</summary>
internal sealed class FileAuditLog : IDisposable
{
    private readonly StreamWriter _writer;
    private bool _disposed;

    /// <summary>Відкриває файл аудиту для дописування.</summary>
    /// <param name="path">Шлях до файлу.</param>
    /// <exception cref="IOException">Файл заблоковано або недоступний.</exception>
    public FileAuditLog(string path)
    {
        _writer = new StreamWriter(path, append: true);
    }

    /// <summary>Дописує рядок аудиту.</summary>
    /// <param name="operation">Назва операції.</param>
    /// <param name="bookingId">Номер бронювання.</param>
    /// <exception cref="ObjectDisposedException">Журнал уже закрито.</exception>
    public void Write(string operation, int bookingId)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _writer.WriteLine(string.Format(
            CultureInfo.InvariantCulture,
            "{0:O};{1};{2}",
            DateTimeOffset.Now,
            operation,
            bookingId));
    }

    /// <summary>Скидає буфер і закриває файл. Повторний виклик нічого не робить.</summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _writer.Flush();
        _writer.Dispose();
        _disposed = true;
    }
}
