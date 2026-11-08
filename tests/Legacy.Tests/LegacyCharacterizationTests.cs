using System.Globalization;
using HotelBooking.Legacy;
using Xunit;

public class LegacyCharacterizationTests
{
    public LegacyCharacterizationTests()
    {
        // формат чисел залежить від культури середовища,
        // тому фіксуємо її явно
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    }

    private static List<RoomLine> TwoLines() => new()
    {
        new RoomLine { RoomNumber = "101", Nights = 2, Rate = 150m },
        new RoomLine { RoomNumber = "204", Nights = 1, Rate = 400m }
    };

    [Theory]
    [InlineData(null, "new", "a@b.c", "ERR: null")]
    [InlineData("empty", "new", "a@b.c", "ERR: empty")]
    [InlineData("two", "draft", "a@b.c", "ERR: state")]
    [InlineData("two", "new", "no-mail", "ERR: mail")]
    public void Handle_BadInput_ReturnsErrorCode(
        string? kindOfLines, string state, string mail, string expected)
    {
        var lines = kindOfLines switch
        {
            "two" => TwoLines(),
            "empty" => new List<RoomLine>(),
            _ => null
        };
        var sut = new LegacyBookingProcessor();

        var actual = sut.Handle(1001, 7, "Іван", mail,
            "regular", 0, lines, state, "UAH",
            new DateTime(2026, 3, 1), false);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Handle_VipBooking_ReturnsExactReport()
    {
        var sut = new LegacyBookingProcessor();

        var report = sut.Handle(1001, 7, "Іван", "a@b.c",
            "vip", 3, TwoLines(), "new", "UAH",
            new DateTime(2026, 3, 1), false);

        var expected =
            "Документ #1001\n" +
            "Клієнт: Іван\n" +
            "101 x2 = 300.00 UAH\n" +
            "204 x1 = 400.00 UAH\n" +
            "Знижка: 105.00 UAH\n" +
            "Доставка: 60.00 UAH\n" +
            "Разом: 655.00 UAH\n";
        Assert.Equal(expected, report);
    }

    [Theory]
    [InlineData("regular", 0, 760)]
    [InlineData("vip", 0, 655)]
    [InlineData("staff", 0, 550)]
    [InlineData("regular", 11, 725)]
    public void Preview_ByKind_ReturnsTotal(string kind, int stays, decimal expected)
    {
        var sut = new LegacyBookingProcessor();

        var total = sut.Preview(kind, stays, TwoLines());

        Assert.Equal(expected, total);
    }

    [Fact]
    public void Preview_PayableExactlyAtThreshold_NoTransferFee()
    {
        // 4 ночі × 250 = 1000 рівно: умова «< 1000» хибна, доставки немає
        var lines = new List<RoomLine>
        {
            new RoomLine { RoomNumber = "305", Nights = 4, Rate = 250m }
        };
        var sut = new LegacyBookingProcessor();

        var total = sut.Preview("regular", 0, lines);

        Assert.Equal(1000m, total);
    }

    [Fact]
    public void DescribeGuest_Vip_BuildsCaption()
    {
        var g = new Guest
        {
            Name = " іван ",
            Email = "IVAN@MAIL.COM",
            Kind = "vip",
            StaysCount = 12,
            SinceUtc = new DateTime(2020, 1, 1)
        };
        // метод залежить від системного годинника,
        // тому очікування обчислюємо, а не вписуємо
        var years = DateTime.Now.Year - 2020;
        var expected = "ІВАН [VIP] [ЛОЯЛЬНИЙ] <ivan@mail.com> стаж " + years;

        var actual = new LegacyBookingProcessor().DescribeGuest(g);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DumpLog_AfterHandleWithMail_ListsEntries()
    {
        var sut = new LegacyBookingProcessor();
        sut.Handle(1001, 7, "Іван", "a@b.c",
            "regular", 0, TwoLines(), "paid", "UAH",
            new DateTime(2026, 3, 1), true);

        var log = sut.DumpLog();

        Assert.Equal("ok 1001\nmail -> a@b.c\n", log);
    }
}
