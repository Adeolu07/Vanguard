using Tripfinity.Interfaces;
using Tripfinity.Models.Data.Requests;
using Tripfinity.Models.Data.Response;
using Tripfinity.Models.Tables;
using Microsoft.AspNetCore.Mvc;

namespace Tripfinity.Controllers;

public class ParentController : Controller
{ 
    protected int? UserId => HttpContext.Session.GetInt32("userId");
    protected bool IsAuthenticated => HttpContext.Session.GetInt32("userId") != null;
    
    protected IActionResult RedirectToLogin() => RedirectToAction("SignIn", "Auth");

    protected static async Task<WalletTransactionDetails?> FetchTransaction(IWalletService walletService, Booking booking)
    {
        if (string.IsNullOrEmpty(booking.PaymentTransactionId))
            return null;
        var transactionRequest = await walletService.GetTransactionAsync(new GetTransactionRequest
        {
            TransactionId = booking.PaymentTransactionId
        });
        return transactionRequest.TransactionDetails!;
    }

}