using _Tripfinity.Interfaces;
using _Tripfinity.Models.Data;
using _Tripfinity.Models.Data.Requests;
using Microsoft.AspNetCore.Mvc;

namespace _Tripfinity.Api;

[Route("api/wallet")]
[ApiController]
public class WalletApiController : ControllerBase
{
    private readonly ILogger<WalletApiController> _logger;
    private readonly IWalletService _walletService;
    private readonly IAuthService _authService;

    public WalletApiController(ILogger<WalletApiController> logger, IWalletService walletService, IAuthService authService)
    {
        _logger = logger;
        _walletService = walletService;
        _authService = authService;
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
    public async Task<IActionResult> DebitWallet([FromBody] DebitWalletRequest request)
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

    [HttpGet("my-balance")]
    public async Task<IActionResult> GetMyBalance()
    {
        var user = _authService.GetCurrentUser(HttpContext);
        if (user == null || string.IsNullOrEmpty(user.UserWalletId))
            return Unauthorized(new { message = "Not authenticated or wallet not linked" });

        var request = new GetBalanceRequest { CustomerId = user.UserWalletId };
        var response = await _walletService.GetBalanceAsync(request);

        return Ok(new
        {
            success = response?.ResponseHeader?.ResponseCode == "00",
            balance = response?.Balance ?? 0,
            currency = "NGN",
            message = response?.ResponseHeader?.ResponseMessage
        });
    }

    [HttpPost("my-fund")]
    public async Task<IActionResult> FundMyWallet([FromBody] CreditWalletRequest model)
    {
        var user = _authService.GetCurrentUser(HttpContext);
        if (user == null || string.IsNullOrEmpty(user.UserWalletId))
            return Unauthorized(new { message = "Not authenticated" });

        if (model.Amount <= 0)
            return BadRequest(new { success = false, message = "Amount must be greater than 0" });

        var request = new CreditWalletRequest
        {
            CustomerId = user.UserWalletId,
            Amount = model.Amount,
            Description = model.Description ?? "Wallet funding",
            TraceId = Guid.NewGuid().ToString("N")
        };

        var response = await _walletService.CreditWalletAsync(request);

        if (response?.ResponseHeader?.ResponseCode == "00")
        {
            var balResp = await _walletService.GetBalanceAsync(new GetBalanceRequest { CustomerId = user.UserWalletId });
            return Ok(new
            {
                success = true,
                message = "Wallet funded successfully",
                transactionId = response.TransactionId,
                balance = balResp?.Balance
            });
        }

        return BadRequest(new
        {
            success = false,
            message = response?.ResponseHeader?.ResponseMessage ?? "Funding failed"
        });
    }

    [HttpPost("create-my-wallet")]
    public async Task<IActionResult> CreateMyWallet()
    {
        var user = _authService.GetCurrentUser(HttpContext);
        if (user == null) return Unauthorized(new { message = "Not logged in" });

        if (!string.IsNullOrEmpty(user.UserWalletId))
            return Ok(new { message = "Wallet already exists", walletId = user.UserWalletId });

        var response = await _walletService.CreateWalletAsync(new CreateWalletRequest
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
        });

        if (response?.ResponseHeader?.ResponseCode == "00")
        {
            user.UserWalletId = response.AccountDetails?.CustomerId;
            var db = HttpContext.RequestServices.GetRequiredService<AppDbContext>();
            db.Users.Update(user);
            await db.SaveChangesAsync();
            _logger.LogInformation("Wallet created for user {UserId}: {WalletId}", user.Id, user.UserWalletId);
            return Ok(new { message = "Wallet created", walletId = user.UserWalletId });
        }

        _logger.LogWarning("Wallet creation failed for user {UserId}: {Code} - {Message}",
            user.Id,
            response?.ResponseHeader?.ResponseCode,
            response?.ResponseHeader?.ResponseMessage);

        return BadRequest(new
        {
            message = response?.ResponseHeader?.ResponseMessage ?? "Failed",
            code = response?.ResponseHeader?.ResponseCode
        });
    }
}