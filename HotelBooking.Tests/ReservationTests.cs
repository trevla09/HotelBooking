using System.Globalization;
using HotelBooking.Core.Domain;
using Xunit;

namespace HotelBooking.Tests;

public class ReservationTests
{
    [Theory]
    [InlineData(false, 2196.0)]
    [InlineData(true, 1976.4)]
    public void CalculateTotal_SampleFromLab2_MatchesOutput(bool isRegular, double expected)
    {
        Reservation reservation = SampleReservation();

        decimal actual = reservation.CalculateTotal(isRegular);

        Assert.Equal((decimal)expected, actual);
    }

    [Fact]
    public void CalculateTotal_RegularGuestBelowThreshold_OnlyAddsVat()
    {
        Reservation reservation = new("B-2", "Петренко");
        reservation.AddLine("101", 2, 400m);

        decimal actual = reservation.CalculateTotal(true);

        Assert.Equal(960m, actual);
    }

    [Fact]
    public void CalculateTotal_LargeBooking_AppliesLargeDiscount()
    {
        Reservation reservation = new("B-3", "Мельник");
        reservation.AddLine("305", 10, 600m);

        decimal actual = reservation.CalculateTotal(false);

        Assert.Equal(6120m, actual);
    }

    [Theory]
    [InlineData(10, 12)]
    [InlineData(0, 0)]
    public void CalculateTotal_ElevenLines_SubtractsBulkDiscount(double rate, double expected)
    {
        Reservation reservation = new("B-4", "Коваленко");
        for (int i = 1; i <= 11; i++)
        {
            string room = i.ToString(CultureInfo.InvariantCulture);
            reservation.AddLine(room, 1, (decimal)rate);
        }

        decimal actual = reservation.CalculateTotal(false);

        Assert.Equal((decimal)expected, actual);
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(3, true)]
    [InlineData(2, false)]
    public void TryChangeStatus_FromNew_FollowsAllowedTransitions(int newStatus, bool expected)
    {
        Reservation reservation = SampleReservation();

        bool actual = reservation.TryChangeStatus(newStatus);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TryChangeStatus_ConfirmedToCheckedIn_Succeeds()
    {
        Reservation reservation = SampleReservation();
        reservation.TryChangeStatus(1);

        bool actual = reservation.TryChangeStatus(2);

        Assert.True(actual);
        Assert.Equal(2, reservation.Status);
    }

    [Theory]
    [InlineData("B-1", "Іваненко", true, true)]
    [InlineData("B-1", "Ів", true, false)]
    [InlineData("", "Іваненко", true, false)]
    [InlineData("B-1", "Іваненко", false, false)]
    public void IsValid_ByData_MatchesRules(string id, string name, bool withLine, bool expected)
    {
        Reservation reservation = new(id, name);
        if (withLine)
        {
            reservation.AddLine("101", 1, 250m);
        }

        bool actual = reservation.IsValid();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void FindById_ExistingAndMissingId_ReturnsMatchOrNull()
    {
        Reservation first = new("B-1", "Іваненко");
        Reservation second = new("B-2", "Петренко");
        List<Reservation> all = new() { first, second };

        Reservation? found = Reservation.FindById(all, "B-2");
        Reservation? missing = Reservation.FindById(all, "B-9");

        Assert.Same(second, found);
        Assert.Null(missing);
    }

    [Fact]
    public void BuildReport_TwoLines_ListsRoomsAndTotal()
    {
        Reservation reservation = SampleReservation();

        string report = reservation.BuildReport();

        Assert.Contains("Номер: 101; діб: 3", report);
        Assert.Contains("Номер: 204; діб: 12", report);
        Assert.Contains("Разом: ", report);
    }

    private static Reservation SampleReservation()
    {
        Reservation reservation = new("B-1001", "Іваненко");
        reservation.AddLine("101", 3, 250m);
        reservation.AddLine("204", 12, 90m);
        return reservation;
    }
}
