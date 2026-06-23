using _Tripfinity.Models.Data.Response;
using _Tripfinity.Models.Tables;

namespace _Tripfinity.Models.ViewModels;

public class ConfirmationViewModel
{
    public required Booking Booking { get; set; }
    public Ticket? Ticket { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public TransactionDetails? TransactionDetails { get; set; }
    
}