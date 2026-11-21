namespace HotelBooking.App.Logging;

/// <summary>
/// Область журналу (наприклад, «UC-02 Booking:61»). Області вкладаються
/// стеком у межах потоку виконання; Dispose знімає верхню.
/// </summary>
internal sealed class LogScope : IDisposable
{
    private readonly AsyncLocal<LogScope?> _current;
    private readonly LogScope? _parent;
    private readonly string _text;

    /// <summary>Відкриває нову область поверх поточної.</summary>
    /// <param name="current">Сховище поточної області, спільне для постачальника.</param>
    /// <param name="text">Текст області.</param>
    public LogScope(AsyncLocal<LogScope?> current, string text)
    {
        _current = current;
        _parent = current.Value;
        _text = text;
        current.Value = this;
    }

    /// <summary>Закриває область і повертає попередню.</summary>
    public void Dispose() => _current.Value = _parent;

    /// <summary>Текст цієї області разом з усіма зовнішніми.</summary>
    /// <returns>Області через « > ».</returns>
    public string Chain() => _parent is null ? _text : $"{_parent.Chain()} > {_text}";
}
