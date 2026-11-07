using System.Text;
using HotelBooking.Core.Bookings;
using HotelBooking.Core.Domain;
using HotelBooking.Core.Reporting;

Console.OutputEncoding = Encoding.UTF8;

var reservation = new Reservation("B-1001", "Іваненко");
reservation.AddLine("101", 3, 250m);
reservation.AddLine("204", 12, 90m);

Console.WriteLine(reservation.CalculateTotal(false));
Console.WriteLine(reservation.CalculateTotal(true));
Console.WriteLine(reservation.IsValid());
Console.WriteLine(reservation.TryChangeStatus(1));
Console.Write(reservation.BuildReport());

TotalRequest request = BuildSampleRequest();
TotalResult result = BookingTotalCalculator.Calculate(request);
string report = BookingReport.TextOf(request, result);
new BookingReportPrinter(Console.Out).Print(report);

static TotalRequest BuildSampleRequest()
{
    List<BookingLine> lines = new()
    {
        new() { RoomNumber = "101", Nights = 12, NightlyRate = 100m },
        new() { RoomNumber = "204", Nights = 1, NightlyRate = 250m },
    };
    return new TotalRequest(
        BookingId: 1,
        Guest: new GuestInfo("Іваненко", "i@ex.com", true),
        Lines: lines,
        CreatedAt: new DateOnly(2026, 3, 10),
        CouponCode: null,
        Status: BookingStatus.New,
        TransferPrice: 60m);
}
