using _Tripfinity.Interfaces;
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
        // var traceId = Guid.NewGuid().ToString("N");
        
        throw new NotImplementedException();
        // var debit -
    }

    public async Task<PaymentResult> ProcessCancellationAsync(Booking booking, User user, bool isMarshal, DateTime tripTime)
    {
        throw new NotImplementedException();
    }
}