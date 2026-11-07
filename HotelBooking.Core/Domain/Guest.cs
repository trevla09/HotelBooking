namespace HotelBooking.Core.Domain;

/// <summary>Гість готелю. Незмінний.</summary>
public sealed record Guest
{
    /// <summary>Створює гостя.</summary>
    /// <param name="id">Ідентифікатор, більший за нуль.</param>
    /// <param name="fullName">Повне ім'я, не порожнє.</param>
    /// <param name="email">Пошта, має містити символ @.</param>
    /// <exception cref="ArgumentOutOfRangeException">Ідентифікатор не більший за нуль.</exception>
    /// <exception cref="ArgumentException">Ім'я порожнє або пошта некоректна.</exception>
    public Guest(int id, string fullName, string email)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        if (!email.Contains('@', StringComparison.Ordinal))
        {
            throw new ArgumentException("Пошта має містити символ @.", nameof(email));
        }

        Id = id;
        FullName = fullName;
        Email = email;
    }

    /// <summary>Ідентифікатор гостя.</summary>
    public int Id { get; }

    /// <summary>Повне ім'я гостя.</summary>
    public string FullName { get; }

    /// <summary>Пошта гостя.</summary>
    public string Email { get; }

    /// <summary>Ознака постійного гостя.</summary>
    public bool IsRegular { get; init; }
}
