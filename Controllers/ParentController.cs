using _Tripfinity.Interfaces;
using _Tripfinity.Models;
using _Tripfinity.Models.Data.Requests;
using _Tripfinity.Models.Data.Response;
using _Tripfinity.Models.Tables;
using Microsoft.AspNetCore.Mvc;

namespace _Tripfinity.Controllers;

public class ParentController : Controller
{
    protected int? UserId => HttpContext.Session.GetInt32("userId");
    protected bool IsAuthenticated => HttpContext.Session.GetInt32("userId") != null;
    
    protected IActionResult RedirectToLogin() => RedirectToAction("SignIn", "Auth");

    protected async Task<TransactionDetails> FetchTransaction(IWalletService walletService, Booking booking)
    {
        if (string.IsNullOrEmpty(booking.PaymentTransactionId))
            return null;
        var transactionRequest = await walletService.GetTransactionAsync(new GetTransactionRequest
        {
            TransactionId = booking.PaymentTransactionId
        });
        return transactionRequest?.TransactionDetails!;
    }

}