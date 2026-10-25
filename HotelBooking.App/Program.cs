using System.Text;
using HotelBooking.Core.Domain;

Console.OutputEncoding = Encoding.UTF8;

var reservation = new Reservation("B-1001", "Іваненко");
reservation.AddLine("101", 3, 250m);
reservation.AddLine("204", 12, 90m);

Console.WriteLine(reservation.CalculateTotal(false));
Console.WriteLine(reservation.CalculateTotal(true));
Console.WriteLine(reservation.IsValid());
Console.WriteLine(reservation.TryChangeStatus(1));
Console.Write(reservation.BuildReport());
