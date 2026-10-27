using Tripfinity.Models.Enums;
using Tripfinity.Models.Tables;
using Tripfinity.Services;

namespace Tripfinity.Models.Data.Response;

public class BookingResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    // Confirmed | InsufficientFunds | Failed
    public BookingStatus Status { get; set; }

    public Booking? Booking { get; set; }
    public Ticket? Ticket { get; set; }
}
