using Microsoft.AspNetCore.Mvc;

namespace _Tripfinity.Interfaces;

public interface IWalletService
{
    Task<IActionResult> CreateWallet(string firstName, string lastName, string customerAlias);
    Task<IActionResult> CreditWallet(decimal amount, string customerId, string description, string traceId);
    Task<IActionResult> DebitWallet(decimal amount, string customerId, string description, string traceId);
    Task<IActionResult> GetBalance(string customerId);
}