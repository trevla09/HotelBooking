using HotelBooking.Core.Abstractions;
using HotelBooking.Core.Domain;

namespace HotelBooking.Tests;

internal sealed class FakeHotelDirectory : IHotelDirectory
{
    private readonly List<Room> _rooms = new();
    private readonly List<Guest> _guests = new();

    public IReadOnlyList<Room> Rooms => _rooms;

    public IReadOnlyList<Guest> Guests => _guests;

    public static FakeHotelDirectory WithThreeRooms()
    {
        FakeHotelDirectory directory = new();
        directory.AddRoom(new Room("101", "Standard", 2, 1000m));
        directory.AddRoom(new Room("102", "Standard", 2, 1000m));
        directory.AddRoom(new Room("201", "Lux", 3, 2500m));
        directory.AddGuest(new Guest(1, "Олена Коваль", "olena@example.com"));
        directory.AddGuest(new Guest(2, "Андрій Шевчук", "andrii@example.com"));
        directory.AddGuest(new Guest(3, "Олег Ковальчук", "oleh@example.com"));
        return directory;
    }

    public void AddRoom(Room room) => _rooms.Add(room);

    public void AddGuest(Guest guest) => _guests.Add(guest);

    public Room? FindRoom(string number) => _rooms.Find(r => r.Number == number);

    public Guest? FindGuest(int id) => _guests.Find(g => g.Id == id);
}
