namespace HotelBooking.Core.Errors;

/// <summary>Порушено правило предметної області.</summary>
public class DomainRuleException : Exception
{
    /// <summary>Створює виняток з ідентифікатором правила.</summary>
    /// <param name="rule">Ідентифікатор правила, наприклад booking.editable.</param>
    /// <param name="message">Повідомлення українською з конкретними значеннями.</param>
    public DomainRuleException(string rule, string message)
        : base(message)
    {
        Rule = rule;
    }

    /// <summary>Створює виняток з ідентифікатором правила і внутрішнім винятком.</summary>
    /// <param name="rule">Ідентифікатор правила.</param>
    /// <param name="message">Повідомлення українською з конкретними значеннями.</param>
    /// <param name="innerException">Виняток, що спричинив порушення.</param>
    public DomainRuleException(string rule, string message, Exception innerException)
        : base(message, innerException)
    {
        Rule = rule;
    }

    /// <summary>Ідентифікатор порушеного правила.</summary>
    public string Rule { get; }
}
