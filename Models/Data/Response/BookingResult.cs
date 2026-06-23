using _Tripfinity.Models.Tables;

namespace _Tripfinity.Models.Data.Response;

public class BookingResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    // Confirmed | InsufficientFunds | Failed
    public string Status { get; set; } = string.Empty;

    public Models.Booking? Booking { get; set; }
    public Ticket? Ticket { get; set; }
}
