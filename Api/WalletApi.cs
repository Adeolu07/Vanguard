using _Tripfinity.Interfaces;
using _Tripfinity.Models.Data.Requests;
using _Tripfinity.Models.Data.Response;
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
    public async Task<IActionResult> CreateWallet(CreateWalletRequest request)
    {
        _logger.LogInformation("Creating wallet for {FirstName} {LastName}", request.FirstName, request.LastName);
        var response = await _walletService.CreateWalletAsync(request);
        return response.ResponseHeader.ResponseCode == "00"
            ? Ok(ApiResponse<string>.Ok(response.AccountDetails.CustomerId))
            : Ok(ApiResponse<string>.Fail(response.ResponseHeader?.ResponseMessage ?? "Wallet creation failed."));
    }

    [HttpPost("credit")]
    public async Task<IActionResult> CreditWallet(CreditWalletRequest request)
    {
        _logger.LogInformation("Credit wallet for CustomerId: {CustomerId}", request.CustomerId);
        var response = await _walletService.CreditWalletAsync(request);
        return response.ResponseHeader?.ResponseCode == "00"
            ? Ok(ApiResponse<string>.Ok(response.TransactionId))
            : Ok(ApiResponse<string>.Fail(response.ResponseHeader?.ResponseMessage ?? "Credit failed."));
    }

    [HttpPost("debit")]
    public async Task<IActionResult> DebitWallet(DebitWalletRequest request)
    {
        _logger.LogInformation("Debit wallet for CustomerId: {CustomerId}", request.CustomerId);
        var response = await _walletService.DebitWalletAsync(request);
        return response.ResponseHeader?.ResponseCode == "00"
            ? Ok(ApiResponse<string>.Ok(response.TransactionId))
            : Ok(ApiResponse<string>.Fail(response.ResponseHeader?.ResponseMessage ?? "Debit failed."));
    }

    [HttpPost("refund")]
    public async Task<IActionResult> Refund(RefundRequest request)
    {
        _logger.LogInformation("Refund for TransactionId: {TransactionId}", request.TransactionId);
        var response = await _walletService.RefundAsync(request);
        return response.ResponseHeader?.ResponseCode == "00"
            ? Ok(ApiResponse<string>.Ok(response.ResponseHeader.ResponseMessage))
            : Ok(ApiResponse<string>.Fail(response.ResponseHeader?.ResponseMessage ?? "Refund failed."));
    }

    [HttpPost("balance")]
    public async Task<IActionResult> GetBalance(GetBalanceRequest request)
    {
        _logger.LogInformation("Get balance for CustomerId: {CustomerId}", request.CustomerId);
        try
        {
            var response = await _walletService.GetBalanceAsync(request);

            if (response.ResponseHeader?.ResponseCode != "00")
                return Ok(ApiResponse<WalletBalanceInfo>.Fail(
                    response.ResponseHeader?.ResponseMessage ?? "Unable to fetch balance."));

            return Ok(ApiResponse<WalletBalanceInfo>.Ok(new WalletBalanceInfo(response.Balance)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get balance failed for CustomerId: {CustomerId}", request.CustomerId);
            return StatusCode(502, ApiResponse<WalletBalanceInfo>.Fail("Wallet service unavailable."));
        }
    }

    [HttpPost("transaction")]
    public async Task<IActionResult> GetTransaction(GetTransactionRequest request)
    {
        _logger.LogInformation("Get transaction for TransactionId: {TransactionId}", request.TransactionId);

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
            _logger.LogError(ex, "Get transaction failed for TransactionId: {TransactionId}", request.TransactionId);
            return StatusCode(502, ApiResponse<WalletTransactionInfo>.Fail("Wallet service unavailable."));
        }
    }
    
    [HttpPost("transactions")]
    public async Task<IActionResult> GetTransactionList(GetTransactionListRequest request)
    {
        _logger.LogInformation("Get transaction list for : {CustomerId}", request.CustomerId);
        var response = await _walletService.GetTransactionList(request);
        return Ok(response);
    }
    
    [HttpPost("nameenquiry")]
    public async Task<IActionResult> NameEnquiry(WalletNameEnquiryRequest request)
    {
        _logger.LogInformation("Wallet name enquiry for CustomerId: {CustomerId}", request.CustomerId);

        try
        {
            var response = await _walletService.WalletNameEnquiryAsync(request);

            if (response.ResponseHeader?.ResponseCode != "00")
                return Ok(ApiResponse<WalletAccountInfo>.Fail(
                    response.ResponseHeader?.ResponseMessage ?? "Unable to load account details."));

            var accountName = $"{response.FirstName} {response.LastName}".Trim();
            return Ok(ApiResponse<WalletAccountInfo>.Ok(new WalletAccountInfo(
                response.AccountNumber ?? "",
                response.BankName ?? "",
                accountName)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Wallet name enquiry failed for CustomerId: {CustomerId}", request.CustomerId);
            return StatusCode(502, ApiResponse<WalletAccountInfo>.Fail("Wallet service unavailable."));
        }
        
    }
}