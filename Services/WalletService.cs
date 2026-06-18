using _Tripfinity.Interfaces;
using _Tripfinity.Models.Data;
using Microsoft.AspNetCore.Mvc;

namespace _Tripfinity.Services;

public class WalletService : IWalletService
{
    private readonly AppDbContext _context;

    public async Task<IActionResult> CreateWallet(string firstName, string lastName, string customerAlias)
    {
        throw new NotImplementedException();
        
        // creates and stores the customer ID in the DB for transactions with that user
    }

    public async Task<IActionResult> CreditWallet(decimal amount, string customerId, string description, string traceId)
    {
        throw new NotImplementedException();
        // use customer wallet ID in the DB to credit user wallet
    }

    public async Task<IActionResult> DebitWallet(decimal amount, string customerId, string description, string traceId)
    {
        throw new NotImplementedException();
        
    }

    public async Task<IActionResult> GetBalance(string customerId)
    {
        throw new NotImplementedException();
    }
}