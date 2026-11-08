using HotelBooking.App.Input;
using HotelBooking.Core.Domain;
using HotelBooking.Core.Errors;
using Xunit;

namespace HotelBooking.Tests;

public class BookingNightParserTests
{
    [Theory]
    [InlineData("", "10.11.2026", "250,50")]
    [InlineData("   ", "10.11.2026", "250,50")]
    [InlineData("12345678901", "10.11.2026", "250,50")]
    [InlineData("101", "31.02.2026", "250,50")]
    [InlineData("101", "2026-11-10", "250,50")]
    [InlineData("101", "10.11.2026", "двісті")]
    [InlineData("101", "10.11.2026", "-1")]
    [InlineData("101", "10.11.2026", "100000,01")]
    public void Parse_InvalidInput_ReturnsFail(string room, string date, string rate)
    {
        Result<BookingNight> result = BookingNightParser.Parse(room, date, rate);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
    }

    [Fact]
    public void Parse_ValidInput_ReturnsNight()
    {
        Result<BookingNight> result = BookingNightParser.Parse(" 101 ", "10.11.2026", "250,50");

        Assert.True(result.IsSuccess);
        Assert.Equal("101", result.Value!.RoomNumber);
        Assert.Equal(250.50m, result.Value.Rate);
    }

    [Fact]
    public void CountValidNights_MixedRows_CountsPositiveIntegers()
    {
        string?[] rows = ["3", "два", "0", "2", null, "-1"];

        int actual = BookingNightParser.CountValidNights(rows);

        Assert.Equal(2, actual);
    }

    [Fact]
    public void CountValidNights_NullRows_ThrowsArgumentNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => _ = BookingNightParser.CountValidNights(null!));
    }
}
