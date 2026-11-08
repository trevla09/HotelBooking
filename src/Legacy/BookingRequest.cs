using System.Collections.Generic;

namespace HotelBooking.Legacy;

public sealed record BookingRequest(
    int BookingId,
    Guest Client,
    IReadOnlyList<RoomLine>? Lines,
    BookingState State,
    string Currency,
    bool SendMail);
