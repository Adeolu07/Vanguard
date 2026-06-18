using _Tripfinity.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace _Tripfinity.Controllers;

public class WalletApi : ControllerBase
{
    private readonly IWalletService _walletService;
    [HttpPost]
    public async Task<IActionResult> CreditWallet(decimal amount, string customerId, string description, string traceId)
    {
        var credit = _walletService.CreditWallet(amount, customerId, description, traceId);
        return Ok(credit);
    }

    [HttpPost]
    public async Task<IActionResult> DebitWallet(decimal amount, string customerId, string description, string traceId)
    {
        var debit  = await _walletService.DebitWallet(amount, customerId, description, traceId);
        return Ok(debit);
    }

    [HttpPost]
    public async Task<IActionResult> GetBalance(string customerId)
    {
        var balance = await _walletService.GetBalance(customerId);
        return Ok(balance);
    }

}