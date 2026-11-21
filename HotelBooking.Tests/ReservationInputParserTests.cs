using HotelBooking.App.Input;
using HotelBooking.Core.Domain;
using HotelBooking.Core.Errors;
using HotelBooking.Core.Services;
using Xunit;

namespace HotelBooking.Tests;

public class ReservationInputParserTests
{
    [Theory]
    [InlineData(null, "101", "01.12.2026", "2", "Ідентифікатор гостя")]
    [InlineData("", "101", "01.12.2026", "2", "Ідентифікатор гостя")]
    [InlineData("-3", "101", "01.12.2026", "2", "Ідентифікатор гостя")]
    [InlineData("0", "101", "01.12.2026", "2", "Ідентифікатор гостя")]
    [InlineData("1", "", "01.12.2026", "2", "Номер кімнати")]
    [InlineData("1", "101", "31.11.2026", "2", "Дата")]
    [InlineData("1", "101", "01.12.2026", "0", "ночей")]
    [InlineData("1", "101", "01.12.2026", "31", "ночей")]
    [InlineData("1", "101", "01.12.2026", "два", "ночей")]
    public void ParseReserve_InvalidField_ExplainsFirstError(
        string? guest,
        string? room,
        string? date,
        string? nights,
        string expectedPart)
    {
        Result<ReserveRequest> result =
            ReservationInputParser.ParseReserve(guest, room, date, nights);

        Assert.False(result.IsSuccess);
        Assert.Contains(expectedPart, result.Error);
    }

    [Fact]
    public void ParseReserve_ValidInput_BuildsHalfOpenPeriod()
    {
        Result<ReserveRequest> result =
            ReservationInputParser.ParseReserve(" 7 ", "201", "30.12.2026", "3");

        Assert.True(result.IsSuccess);
        Assert.Equal(7, result.Value!.GuestId);
        Assert.Equal("201", result.Value.RoomNumber);
        Assert.Equal(new DateOnly(2027, 1, 2), result.Value.Period.CheckOut);
    }

    [Fact]
    public void ParsePeriod_ValidInput_ReturnsNights()
    {
        Result<StayPeriod> result = ReservationInputParser.ParsePeriod("01.12.2026", "4");

        Assert.True(result.IsSuccess);
        Assert.Equal(4, result.Value!.Nights);
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("1.5")]
    public void ParseBookingId_NotPositiveInteger_Fails(string? input)
    {
        Assert.False(ReservationInputParser.ParseBookingId(input).IsSuccess);
    }

    [Fact]
    public void ParseDate_WrongFormat_Fails()
    {
        Assert.False(ReservationInputParser.ParseDate("2026-11-23").IsSuccess);
        Assert.True(ReservationInputParser.ParseDate("23.11.2026").IsSuccess);
    }

    [Fact]
    public void ParseSearch_AllEmpty_AsksForCriterion()
    {
        Result<BookingSearch> result = ReservationInputParser.ParseSearch(" ", null, "");

        Assert.False(result.IsSuccess);
        Assert.Contains("хоча б одну", result.Error);
    }

    [Theory]
    [InlineData("checkedin", BookingStatus.CheckedIn)]
    [InlineData("Cancelled", BookingStatus.Cancelled)]
    public void ParseSearch_StatusIgnoresCase(string input, BookingStatus expected)
    {
        Result<BookingSearch> result = ReservationInputParser.ParseSearch(null, null, input);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value!.Status);
    }

    [Theory]
    [InlineData("Paid")]
    [InlineData("9")]
    public void ParseSearch_UnknownStatus_Fails(string input)
    {
        Assert.False(ReservationInputParser.ParseSearch(null, null, input).IsSuccess);
    }

    [Fact]
    public void ParseSearch_BadDate_Fails()
    {
        Assert.False(ReservationInputParser.ParseSearch("Ко", "1.1.26", null).IsSuccess);
    }

    [Fact]
    public void ParseSearch_NameAndDate_TrimsName()
    {
        Result<BookingSearch> result =
            ReservationInputParser.ParseSearch(" Коваль ", "10.11.2026", null);

        Assert.True(result.IsSuccess);
        Assert.Equal("Коваль", result.Value!.GuestNamePart);
        Assert.Equal(new DateOnly(2026, 11, 10), result.Value.OnDate);
    }
}
