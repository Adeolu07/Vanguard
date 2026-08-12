using _Tripfinity.Models.Tables;

namespace _Tripfinity.Interfaces;

public interface IPaymentService
{
    Task<PaymentResult> ProcessPaymentAsync(User user, Booking booking);
    Task<PaymentResult> ProcessCancellationAsync(Booking booking, User user, bool isMarshal, DateTime tripTime);
}

public class PaymentResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}