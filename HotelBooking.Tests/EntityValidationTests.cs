using HotelBooking.Core.Domain;
using HotelBooking.Core.Errors;
using Xunit;

namespace HotelBooking.Tests;

public class EntityValidationTests
{
    [Fact]
    public void Guest_EmailWithoutAt_ThrowsWithParamName()
    {
        string email = "ivan.example.com";

        ArgumentException ex = Assert.Throws<ArgumentException>(
            () => _ = new Guest(1, "Іваненко", email));

        Assert.Equal("email", ex.ParamName);
    }

    [Fact]
    public void Guest_ValidData_KeepsValues()
    {
        Guest guest = new(1, "Іваненко", "i@ex.com") { IsRegular = true };

        Assert.Equal("i@ex.com", guest.Email);
        Assert.True(guest.IsRegular);
    }

    [Fact]
    public void Room_ZeroCapacity_ThrowsOutOfRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => _ = new Room("101", "standard", 0, 250m));
    }

    [Fact]
    public void Room_NullCategory_BecomesEmpty()
    {
        Room room = new("101", null!, 2, 250m);

        Assert.Equal(string.Empty, room.Category);
        Assert.Equal(2, room.Capacity);
    }

    [Fact]
    public void Result_Ok_IsSuccessWithValue()
    {
        Result<int> result = Result<int>.Ok(5);

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value);
    }

    [Fact]
    public void Result_Fail_CarriesError()
    {
        Result<int> result = Result<int>.Fail("Тариф - число");

        Assert.False(result.IsSuccess);
        Assert.Equal("Тариф - число", result.Error);
    }
}
