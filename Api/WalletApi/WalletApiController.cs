using _Tripfinity.Interfaces;
using _Tripfinity.Models.Data.Requests;
using Microsoft.AspNetCore.Mvc;

namespace _Tripfinity.Api;

[Route("api/wallet")]
[ApiController]
public class WalletApiController : ControllerBase
{
    private readonly IWalletService _walletService;

    public WalletApiController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    [HttpPost("createWallet")]
    public async Task<IActionResult> CreateWallet([FromBody] CreateWalletRequest request)
    {
        var response = await _walletService.CreateWalletAsync(request);
        return Ok(response);
    }

    [HttpPost("credit")]
    public async Task<IActionResult> CreditWallet([FromBody] CreditWalletRequest request)
    {
        var response = await _walletService.CreditWalletAsync(request);
        return Ok(response);
    }

    [HttpPost("debit")]
    public async Task<IActionResult> DebitWallet([FromBody] DebitWalletRequest request)
    {
        var response = await _walletService.DebitWalletAsync(request);
        return Ok(response);
    }

    [HttpPost("balance")]
    public async Task<IActionResult> GetBalance([FromBody] GetBalanceRequest request)
    {
        var response = await _walletService.GetBalanceAsync(request);
        return Ok(response);
    }

    [HttpPost("refund")]
    public async Task<IActionResult> Refund([FromBody] RefundRequest request)
    {
        var response = await _walletService.RefundAsync(request);
        return Ok(response);
    }

    [HttpGet("transaction")]
    public async Task<IActionResult> GetTransaction([FromQuery] GetTransactionRequest request)
    {
        var response = await _walletService.GetTransactionAsync(request);
        return Ok(response);
    }
}