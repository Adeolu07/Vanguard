using Tripfinity.Interfaces;
using Tripfinity.Models.Data;
using Tripfinity.Models.Data.Requests;
using Tripfinity.Models.Data.Response;
using Tripfinity.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Tripfinity.Api;

// JSON surface used by the browser. It is session-scoped: callers can only ever
// reach their own wallet, and the fund-moving endpoints are admin-only.
[Route("api/wallet")]
[ApiController]
[IgnoreAntiforgeryToken]
public class WalletApiController : ControllerBase
{
    private readonly ILogger<WalletApiController> _logger;
    private readonly IWalletService _walletService;
    private readonly AppDbContext _db;

    public WalletApiController(ILogger<WalletApiController> logger, IWalletService walletService, AppDbContext db)
    {
        _logger = logger;
        _walletService = walletService;
        _db = db;
    }

    private int CurrentUserId => (int)HttpContext.Items["userId"]!;

    private Task<string?> GetOwnWalletIdAsync() =>
        _db.Users.AsNoTracking()
            .Where(u => u.Id == CurrentUserId)
            .Select(u => u.UserWalletId)
            .FirstOrDefaultAsync();

    [HttpPost("auth")]
    [ApiSessionAuth(Role = ApiRoles.Admin)]
    public async Task<IActionResult> Authenticate(AuthenticationRequest request)
    {
        try
        {
            await _walletService.EnsureAuthenticatedAsync();
            return Ok(ApiResponse<string>.Ok("Successful authentication"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Wallet Authentication failed");
            return StatusCode(502, ApiResponse<string>.Fail("Wallet Service unavailable"));
        }
    }

    [HttpPost("create")]
    [ApiSessionAuth(Role = ApiRoles.Admin)]
    public async Task<IActionResult> CreateWallet(CreateWalletRequest request)
    {
        _logger.LogInformation("Creating wallet for {FirstName} {LastName}", request.FirstName, request.LastName);
        var response = await _walletService.CreateWalletAsync(request);
        return response.ResponseHeader.ResponseCode == "00"
            ? Ok(ApiResponse<string>.Ok(response.ToString()!))
            : Ok(ApiResponse<string>.Fail(response.ResponseHeader?.ResponseMessage ?? "Wallet creation failed."));
    }

    [HttpPost("credit")]
    [ApiSessionAuth(Role = ApiRoles.Admin)]
    public async Task<IActionResult> CreditWallet(CreditWalletRequest request)
    {
        _logger.LogInformation("Credit wallet for CustomerId: {CustomerId}", request.CustomerId);
        var response = await _walletService.CreditWalletAsync(request);
        return response.ResponseHeader?.ResponseCode == "00"
            ? Ok(ApiResponse<string>.Ok(response.ToString()!))
            : Ok(ApiResponse<string>.Fail(response.ResponseHeader?.ResponseMessage ?? "Credit failed."));
    }

    [HttpPost("debit")]
    [ApiSessionAuth(Role = ApiRoles.Admin)]
    public async Task<IActionResult> DebitWallet(DebitWalletRequest request)
    {
        _logger.LogInformation("Debit wallet for CustomerId: {CustomerId}", request.CustomerId);
        var response = await _walletService.DebitWalletAsync(request);
        return response.ResponseHeader?.ResponseCode == "00"
            ? Ok(ApiResponse<string>.Ok(response.TransactionId))
            : Ok(ApiResponse<string>.Fail(response.ResponseHeader?.ResponseMessage ?? "Debit failed."));
    }

    [HttpPost("refund")]
    [ApiSessionAuth(Role = ApiRoles.Admin)]
    public async Task<IActionResult> Refund(RefundRequest request)
    {
        _logger.LogInformation("Refund for TransactionId: {TransactionId}", request.TransactionId);
        var response = await _walletService.RefundAsync(request);
        return response.ResponseHeader?.ResponseCode == "00"
            ? Ok(ApiResponse<string>.Ok(response.ResponseHeader.ResponseMessage))
            : Ok(ApiResponse<string>.Fail(response.ResponseHeader?.ResponseMessage ?? "Refund failed."));
    }

    [HttpPost("balance")]
    [ApiSessionAuth]
    public async Task<IActionResult> GetBalance()
    {
        var walletId = await GetOwnWalletIdAsync();
        if (string.IsNullOrWhiteSpace(walletId))
            return Ok(ApiResponse<WalletBalanceInfo>.Fail("No wallet is linked to this account."));

        try
        {
            var response = await _walletService.GetBalanceAsync(new GetBalanceRequest { CustomerId = walletId });

            if (response.ResponseHeader?.ResponseCode != "00")
                return Ok(ApiResponse<WalletBalanceInfo>.Fail(
                    response.ResponseHeader?.ResponseMessage ?? "Unable to fetch balance."));

            return Ok(ApiResponse<WalletBalanceInfo>.Ok(new WalletBalanceInfo(response.Balance)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get balance failed for user {UserId}", CurrentUserId);
            return StatusCode(502, ApiResponse<WalletBalanceInfo>.Fail("Wallet service unavailable."));
        }
    }

    [HttpPost("transaction")]
    [ApiSessionAuth]
    public async Task<IActionResult> GetTransaction(GetTransactionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.TransactionId))
            return BadRequest(ApiResponse<WalletTransactionInfo>.Fail("A transaction id is required."));

        // Only allow callers to read transactions that belong to their own wallet.
        var owned = await _db.Transactions.AsNoTracking()
            .AnyAsync(t => t.UserId == CurrentUserId && t.ExternalReference == request.TransactionId);

        if (!owned)
            return NotFound(ApiResponse<WalletTransactionInfo>.Fail("Transaction not found."));

        try
        {
            var response = await _walletService.GetTransactionAsync(request);

            if (response.ResponseHeader?.ResponseCode != "00")
                return Ok(ApiResponse<WalletTransactionInfo>.Fail(
                    response.ResponseHeader?.ResponseMessage ?? "Unable to fetch transaction."));

            var details = response.TransactionDetails;
            return Ok(ApiResponse<WalletTransactionInfo>.Ok(new WalletTransactionInfo(
                details?.TranType ?? "",
                details?.Amount ?? 0,
                details?.Description ?? "",
                details?.TransactionId ?? request.TransactionId,
                details?.SessionId ?? "")));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get transaction failed for transaction {TransactionId}", request.TransactionId);
            return StatusCode(502, ApiResponse<WalletTransactionInfo>.Fail("Wallet service unavailable."));
        }
    }
    
    [HttpPost("transactions")]
    [ApiSessionAuth]
    public async Task<IActionResult> GetTransactionList(GetTransactionListRequest request)
    {
        var walletId = await GetOwnWalletIdAsync();
        if (string.IsNullOrWhiteSpace(walletId))
            return Ok(ApiResponse<object>.Fail("No wallet is linked to this account."));

        request.CustomerId = walletId;
        var response = await _walletService.GetTransactionList(request);
        return Ok(response);
    }
    
    [HttpPost("nameenquiry")]
    [ApiSessionAuth]
    public async Task<IActionResult> NameEnquiry(WalletNameEnquiryRequest request)
    {
        var walletId = await GetOwnWalletIdAsync();
        if (string.IsNullOrWhiteSpace(walletId))
            return Ok(ApiResponse<WalletAccountInfo>.Fail("No wallet is linked to this account."));

        request.CustomerId = walletId;

        try
        {
            var response = await _walletService.WalletNameEnquiryAsync(request);

            if (response.ResponseHeader?.ResponseCode != "00")
                return Ok(ApiResponse<WalletAccountInfo>.Fail(
                    response.ResponseHeader?.ResponseMessage ?? "Unable to load account details."));
            
            var accountNumber = response.AccountNumber ?? "";
            var bankName = response.BankName ?? "Default Bank";
            var accountName = $"{response.FirstName} {response.LastName}".Trim();

            return Ok(ApiResponse<WalletAccountInfo>.Ok(new WalletAccountInfo(
                accountNumber, bankName, accountName)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Wallet name enquiry failed for user {UserId}", CurrentUserId);
            return StatusCode(502, ApiResponse<WalletAccountInfo>.Fail("Wallet service unavailable."));
        }
    }
}