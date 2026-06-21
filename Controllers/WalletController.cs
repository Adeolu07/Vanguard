using _Tripfinity.Interfaces;
using _Tripfinity.Models.Data.Requests;
using Microsoft.AspNetCore.Mvc;

namespace _Tripfinity.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WalletController : ControllerBase
{
    private readonly ILogger<WalletController> _logger;
    private readonly IWalletService _walletService;

    public WalletController(ILogger<WalletController> logger, IWalletService walletService)
    {
        _walletService = walletService;
        _logger = logger;
    }

    [HttpPost("authenticate")]
    public async Task<IActionResult> Authentication([FromBody] AuthenticationRequest request)
    {
        _logger.LogInformation("Authenticating user: {Username}", request.Username);
        var response = await _walletService.AuthenticationAsync(request);
        _logger.LogInformation("raw token:  {RawResponse}", response.Token);
        return Ok(response);
    }
    
    [HttpPost("createWallet")]
    public async Task<IActionResult> CreateWallet([FromBody] CreateWalletRequest request)
    {
        _logger.LogInformation("Creating wallet for {FirstName} {LastName}", request.FirstName, request.LastName);
        var response = await _walletService.CreateWalletAsync(request);
        return Ok(response);
    }
    
    [HttpPost("credit")]
    public async Task<IActionResult> CreditWallet([FromBody] CreditWalletRequest request)
    {
        _logger.LogInformation("Credit wallet for customer with ID: {CustomerId}", request.CustomerId);
        var response = await _walletService.CreditWalletAsync(request);
        return Ok(response);
    }
    
    [HttpPost("debit")]
    public async Task<IActionResult> DebitWallet([FromBody]DebitWalletRequest request)
    {
        _logger.LogInformation("Debit wallet for customer with ID: {CustomerId}", request.CustomerId);
        var response = await _walletService.DebitWalletAsync(request);
        return Ok(response);
    }
    
    [HttpPost("refund")]
    public async Task<IActionResult> Refund([FromBody] RefundRequest request)
    {
        _logger.LogInformation("Refund transaction for transaction: {TransactionId}", request.TransactionId);
        var response = await _walletService.RefundAsync(request);
        return Ok(response);
    }

    [HttpPost("balance")]
    public async Task<IActionResult> GetBalance([FromBody] GetBalanceRequest request)
    {
        _logger.LogInformation("Get balance for customer with ID: {CustomerId}", request.CustomerId);
        var response = await _walletService.GetBalanceAsync(request);
        return Ok(response);
    }
    
    [HttpPost("getTransaction")]
    public async Task<IActionResult> GetTransaction([FromBody] GetTransactionRequest request)
    {
        _logger.LogInformation("Get transaction for transaction: {TransactionId}", request.TransactionId);
        var response = await _walletService.GetTransactionAsync(request);
        return Ok(response);
    }
}