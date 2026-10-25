namespace HotelBooking.Core.Domain;


public sealed class Booking
{
    private readonly List<BookingNight> _nights = new();

    public int Id { get; init; }

    public int GuestId { get; init; }

    public BookingStatus Status { get; set; } = BookingStatus.New;

    public DateTimeOffset CreatedAt { get; init; }

    public IReadOnlyList<BookingNight> Nights => _nights;

    
    public void AddNight(BookingNight night) => _nights.Add(night);

    
    public decimal Total()
    {
        decimal sum = 0m;
        foreach (BookingNight night in _nights)
        {
            sum += night.Rate;
        }

        return sum;
    }
}