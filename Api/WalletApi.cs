using _Tripfinity.Interfaces;
using _Tripfinity.Models.Data.Requests;
using Microsoft.AspNetCore.Mvc;

namespace _Tripfinity.Api;

[Route("api/wallet")]
[ApiController]
public class WalletApiController : ControllerBase
{
    private readonly ILogger<WalletApiController> _logger;
    private readonly IWalletService _walletService;

    public WalletApiController(ILogger<WalletApiController> logger, IWalletService walletService)
    {
        _logger = logger;
        _walletService = walletService;
    }

    [HttpPost("auth")]
    public async Task<IActionResult> Authenticate([FromBody] AuthenticationRequest request)
    {
        await _walletService.EnsureAuthenticatedAsync();
        return Ok("Successful authentication");
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateWallet([FromBody] CreateWalletRequest request)
    {
        _logger.LogInformation("Creating wallet for {FirstName} {LastName}", request.FirstName, request.LastName);
        var response = await _walletService.CreateWalletAsync(request);
        return Ok(response);
    }

    [HttpPost("credit")]
    public async Task<IActionResult> CreditWallet([FromBody] CreditWalletRequest request)
    {
        _logger.LogInformation("Credit wallet for CustomerId: {CustomerId}", request.CustomerId);
        var response = await _walletService.CreditWalletAsync(request);
        return Ok(response);
    }

    [HttpPost("debit")]
    public async Task<IActionResult> DebitWallet([FromBody] DebitWalletRequest request)
    {
        _logger.LogInformation("Debit wallet for CustomerId: {CustomerId}", request.CustomerId);
        var response = await _walletService.DebitWalletAsync(request);
        return Ok(response);
    }

    [HttpPost("refund")]
    public async Task<IActionResult> Refund([FromBody] RefundRequest request)
    {
        _logger.LogInformation("Refund for TransactionId: {TransactionId}", request.TransactionId);
        var response = await _walletService.RefundAsync(request);
        return Ok(response);
    }

    [HttpPost("balance")]
    public async Task<IActionResult> GetBalance([FromBody] GetBalanceRequest request)
    {
        _logger.LogInformation("Get balance for CustomerId: {CustomerId}", request.CustomerId);
        var response = await _walletService.GetBalanceAsync(request);
        return Ok(response);
    }

    [HttpPost("transaction")]
    public async Task<IActionResult> GetTransaction([FromBody] GetTransactionRequest request)
    {
        _logger.LogInformation("Get transaction for TransactionId: {TransactionId}", request.TransactionId);
        var response = await _walletService.GetTransactionAsync(request);
        return Ok(response);
    }
    
    [HttpPost("transactions")]
    public async Task<IActionResult> GetTransactionList([FromBody] GetTransactionListRequest request)
    {
        _logger.LogInformation("Get transaction list for : {CustomerId}", request.CustomerId);
        var response = await _walletService.GetTransactionList(request);
        return Ok(response);
    }
    
    [HttpPost("nameenquiry")]
    public async Task<IActionResult> NameEnquiry( WalletNameEnquiryRequest request)
    {
        _logger.LogInformation("Get transaction list for : {CustomerId}", request.CustomerId);
        var response = await _walletService.WalletNameEnquiryAsync(request);
        return Ok(response);
    }
}