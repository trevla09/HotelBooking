using System.Globalization;
using HotelBooking.Core.Domain;
using HotelBooking.Core.Infrastructure;
using Xunit;

namespace HotelBooking.Tests;

public class JsonStorageTests
{
    [Fact]
    public void Save_ThenLoad_RestoresStatusAndNights()
    {
        string path = TempFile();
        try
        {
            JsonBookingRepository first = new(path);
            Booking booking = new(first.NextId(), 2, DateTimeOffset.UnixEpoch);
            booking.AddNight(new BookingNight("101", Day("2026-06-10"), 1000m));
            booking.AddNight(new BookingNight("101", Day("2026-06-11"), 1000m));
            booking.Confirm();
            first.Add(booking);
            first.Save();

            JsonBookingRepository second = new(path);
            Booking? loaded = second.GetById(1);

            Assert.NotNull(loaded);
            Assert.Equal(BookingStatus.Confirmed, loaded!.Status);
            Assert.Equal(2, loaded.Nights.Count);
            Assert.Equal(2000m, loaded.Total());
            Assert.Equal(2, second.NextId());
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Constructor_MissingFile_StartsEmpty()
    {
        string path = TempFile();

        JsonBookingRepository repository = new(path);

        Assert.Empty(repository.GetAll());
        Assert.Equal(1, repository.NextId());
    }

    [Fact]
    public void Constructor_BrokenJson_ThrowsInvalidData()
    {
        string path = TempFile();
        File.WriteAllText(path, "{ це не json");
        try
        {
            Assert.Throws<InvalidDataException>(() => new JsonBookingRepository(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Constructor_DuplicateNightInFile_ThrowsInvalidData()
    {
        string path = TempFile();
        const string Night = "{\"roomNumber\":\"101\",\"date\":\"2026-06-10\",\"rate\":1000}";
        File.WriteAllText(
            path,
            "[{\"id\":1,\"guestId\":1,\"createdAt\":\"2026-06-01T10:00:00+03:00\","
                + "\"status\":\"New\",\"nights\":[" + Night + "," + Night + "]}]");
        try
        {
            Assert.Throws<InvalidDataException>(() => new JsonBookingRepository(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Directory_ReadsRoomsAndGuests()
    {
        string rooms = TempFile();
        string guests = TempFile();
        File.WriteAllText(
            rooms,
            "[{\"number\":\"101\",\"category\":\"Standard\",\"capacity\":2,\"baseRate\":1000}]");
        File.WriteAllText(
            guests,
            "[{\"id\":1,\"fullName\":\"Олена Коваль\",\"email\":\"olena@example.com\"}]");
        try
        {
            JsonHotelDirectory directory = new(rooms, guests);

            Assert.Equal(1000m, directory.FindRoom("101")!.BaseRate);
            Assert.Equal("Олена Коваль", directory.FindGuest(1)!.FullName);
            Assert.Null(directory.FindRoom("999"));
        }
        finally
        {
            File.Delete(rooms);
            File.Delete(guests);
        }
    }

    [Fact]
    public void Directory_InvalidGuestEmail_ThrowsInvalidData()
    {
        string rooms = TempFile();
        string guests = TempFile();
        File.WriteAllText(rooms, "[]");
        File.WriteAllText(guests, "[{\"id\":1,\"fullName\":\"Без пошти\",\"email\":\"нема\"}]");
        try
        {
            Assert.Throws<InvalidDataException>(() => new JsonHotelDirectory(rooms, guests));
        }
        finally
        {
            File.Delete(rooms);
            File.Delete(guests);
        }
    }

    [Fact]
    public void Directory_MissingFile_ThrowsFileNotFound()
    {
        Assert.Throws<FileNotFoundException>(
            () => new JsonHotelDirectory(TempFile(), TempFile()));
    }

    private static string TempFile() =>
        Path.Combine(Path.GetTempPath(), $"hb-{Guid.NewGuid():N}.json");

    private static DateOnly Day(string text) => DateOnly.Parse(text, CultureInfo.InvariantCulture);
}
