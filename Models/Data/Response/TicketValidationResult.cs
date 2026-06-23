using _Tripfinity.Models.Tables;

namespace _Tripfinity.Models.Data.Response;

public class TicketValidationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Ticket? Ticket { get; set; }
}
