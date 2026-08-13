using _Tripfinity.Interfaces;
using _Tripfinity.Models.Data.Requests;
using _Tripfinity.Models.Tables;

namespace _Tripfinity.Services;

public class PaymentService : IPaymentService
{
    private readonly IWalletService _walletService;
    private readonly IMarshalService _marshalService;
    private readonly IAdminService _adminService;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(IWalletService walletService, IMarshalService marshalService, IAdminService adminService,
        ILogger<PaymentService> logger)
    {
        _walletService = walletService;
        _marshalService = marshalService;
        _adminService = adminService;
        _logger = logger;
    }


    public async Task<PaymentResult> ProcessPaymentAsync(User user, Booking booking)
    {
        var traceId = Guid.NewGuid().ToString("N");
        var debit = await _walletService.DebitWalletAsync(new DebitWalletRequest
        {
            Amount = booking.TotalAmount,
            CustomerId = user.UserWalletId!,
            Description = $"Tripfinity {booking.TransportType} booking #{booking.Id}",
            TraceId = traceId
        });
        

        if (debit.ResponseHeader.ResponseCode != "00")
            return new PaymentResult
            {
                Success = false,
                ErrorMessage = debit.ResponseHeader.ResponseMessage
            };
        
        var marshalWalletId = await _marshalService.GetMarshalWalletIdAsync(booking);
        var adminWalletId = await _adminService.GetAdminWalletIdAsync();

        if (string.IsNullOrEmpty(marshalWalletId) || string.IsNullOrEmpty(adminWalletId))
        {
            // Refund user if marshal/admin wallet is not found
            await _walletService.RefundAsync(new RefundRequest
            {
                CustomerId = user.UserWalletId!,
                TransactionId = debit.TransactionId,
                Description = "Missing marshal/admin wallet – refunding"
            });
            return new PaymentResult { Success = false, ErrorMessage = "Unable to process payment split." };
        }

        // wallet is found
        var marshalCredit = await _walletService.CreditWalletAsync(new CreditWalletRequest
        {
            Amount = booking.TotalAmount * 0.8m,
            CustomerId = marshalWalletId,
            Description = $"Earnings from booking #{booking.Id}",
            TraceId = Guid.NewGuid().ToString("N")
        });

        var adminCredit = await _walletService.CreditWalletAsync(new CreditWalletRequest
        {
            Amount = booking.TotalAmount * 0.2m,
            CustomerId = adminWalletId,
            Description = $"Commission from booking #{booking.Id}",
            TraceId = Guid.NewGuid().ToString("N")
        });

        if (marshalCredit.ResponseHeader.ResponseCode != "00" ||
            adminCredit.ResponseHeader.ResponseCode != "00")
        {
            // Roll back: refund user fully if any of the credits fail
            await _walletService.RefundAsync(new RefundRequest
            {
                CustomerId = user.UserWalletId,
                TransactionId = debit.TransactionId,
                Description = "Split failed – refunding"
            });

            if (marshalCredit.ResponseHeader.ResponseCode == "00")
                await _walletService.RefundAsync(new RefundRequest
                {
                    CustomerId = marshalWalletId,
                    TransactionId = marshalCredit.TransactionId,
                    Description = "Reversal due to split failure"
                });

            if (adminCredit.ResponseHeader.ResponseCode == "00")
                await _walletService.RefundAsync(new RefundRequest
                {
                    CustomerId = adminWalletId,
                    TransactionId = adminCredit.TransactionId,
                    Description = "Reversal due to split failure"
                });

            booking.PaymentTransactionId = debit.TransactionId;
            booking.PaymentTraceId = debit.TraceId;
            return new PaymentResult { Success = false, ErrorMessage = "Payment split failed – wallet refunded." };
        }

        return new PaymentResult { Success = true };
        
    }

    public async Task<PaymentResult> ProcessCancellationAsync(Booking booking, User user, bool isMarshalCancelling, DateTime tripTime)
    {
         var now = DateTime.Now;
        var marshalWalletId = await _marshalService.GetMarshalWalletIdAsync(booking);
        var adminWalletId = await _adminService.GetAdminWalletIdAsync();

        if (string.IsNullOrEmpty(marshalWalletId) || string.IsNullOrEmpty(adminWalletId))
            return new PaymentResult { Success = false, ErrorMessage = "Marshal or admin wallet missing." };

        var total = booking.TotalAmount;

        try
        {
            if (isMarshalCancelling)
            {
                // Full refund to user
                await _walletService.CreditWalletAsync(new CreditWalletRequest
                {
                    Amount = total,
                    CustomerId = user.UserWalletId!,
                    Description = $"Full refund – Marshal cancelled booking #{booking.Id}",
                    TraceId = Guid.NewGuid().ToString("N")
                });

                // Reverse marshal's earnings (80%) and penalty (5%)
                await _walletService.DebitWalletAsync(new DebitWalletRequest
                {
                    Amount = total * 0.85m,  // 80% + 5% penalty
                    CustomerId = marshalWalletId,
                    Description = $"Reversal + penalty – marshal cancelled booking #{booking.Id}",
                    TraceId = Guid.NewGuid().ToString("N")
                });

                // Reverse admin's commission (20%) but add penalty (5%) -> net debit 15%
                await _walletService.DebitWalletAsync(new DebitWalletRequest
                {
                    Amount = total * 0.15m,
                    CustomerId = adminWalletId,
                    Description = $"Commission reversal – marshal cancelled +#{booking.Id}",
                    TraceId = Guid.NewGuid().ToString("N")
                });

                // Penalty credit to admin (5%) – already included in the 15% debit above?
                // Original code: debit admin 20%, credit admin 5% => net debit 15%. So the -15% is correct.
            }
            else // User cancellation
            {
                if (tripTime > now.AddHours(2))
                {
                    // More than 2h before: user gets 80%
                    await _walletService.CreditWalletAsync(new CreditWalletRequest
                    {
                        Amount = total * 0.8m,
                        CustomerId = user.UserWalletId!,
                        Description = $"80% refund – user cancelled booking #{booking.Id}",
                        TraceId = Guid.NewGuid().ToString("N")
                    });
                    
                    // marshal is debited 80% of what user paid
                    await _walletService.DebitWalletAsync(new DebitWalletRequest
                    {
                        Amount = total * 0.8m,
                        CustomerId = marshalWalletId,
                        Description = $"Reversal – user cancelled booking #{booking.Id}",
                        TraceId = Guid.NewGuid().ToString("N")
                    });
                    // admin keeps 20% – no change
                }
                else if (now < tripTime)
                {
                    // Less than 2h before departure: user gets 60%
                    await _walletService.CreditWalletAsync(new CreditWalletRequest
                    {
                        Amount = total * 0.6m,
                        CustomerId = user.UserWalletId!,
                        Description = $"60% refund – late cancellation #{booking.Id}",
                        TraceId = Guid.NewGuid().ToString("N")
                    });
                    // marshal returns 55% (keeps 25%)
                    await _walletService.DebitWalletAsync(new DebitWalletRequest
                    {
                        Amount = total * 0.55m,
                        CustomerId = marshalWalletId,
                        Description = $"Reversal – late cancellation #{booking.Id}",
                        TraceId = Guid.NewGuid().ToString("N")
                    });
                    // admin returns 5% (keeps 15%)
                    await _walletService.DebitWalletAsync(new DebitWalletRequest
                    {
                        Amount = total * 0.05m,
                        CustomerId = adminWalletId,
                        Description = $"Partial commission reversal – late cancellation #{booking.Id}",
                        TraceId = Guid.NewGuid().ToString("N")
                    });
                }
                else
                {
                    // After departure: no refund
                    return new PaymentResult { Success = true , Refunded = false}; // success but no refund
                }
            }
            return new PaymentResult { Success = true, Refunded = true};
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cancellation payment failed for booking {BookingId}", booking.Id);
            return new PaymentResult { Success = false, ErrorMessage = "Payment processing error during cancellation." };
        }
    }
}