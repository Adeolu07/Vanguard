using System.Diagnostics;
using Tripfinity.Interfaces;
using Tripfinity.Models.Data;
using Tripfinity.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Tripfinity.Controllers;

public class HomeController : ParentController
{
    private readonly IPassengerService _passenger;
    private readonly ILogger<HomeController> _logger;
    private readonly IMarshalService _marshalService;

    public HomeController(IPassengerService passenger, ILogger<HomeController> logger , IMarshalService marshalService)
    {
        _passenger = passenger;
        _marshalService = marshalService;
        _logger = logger;
    }

    public IActionResult Index()
    {
        if (IsAuthenticated)
            return RedirectToAction("Dashboard");
        return View();
    }
    public IActionResult Privacy() => View();
    public IActionResult Terms() => View();
    
    
    public async Task<IActionResult> Wallet(int page = 1)
    {
        _logger.LogInformation("GET /Wallet");
        if (!IsAuthenticated)
            return RedirectToAction("Index", "Home");
        
        var user = await _passenger.GetPassengerAsync(UserId!.Value);
        if (user is null) 
            return RedirectToLogin();
        
        ViewBag.WalletId = user.UserWalletId;
        var transactions = await _passenger.GetWalletTransactions(UserId!.Value, page);
        return View(transactions);
    }
    
    public async Task<IActionResult> Dashboard()
    {
        if (!IsAuthenticated) 
            return RedirectToLogin();

        var genericUser = await _passenger.GetUserByIdAsync(UserId!.Value);
        if (genericUser is null) 
            return RedirectToLogin();

        if (genericUser.Role == "Admin")
            return RedirectToAction("Index","Admin");

        var passenger = await _passenger.GetPassengerAsync(UserId.Value);
        if (passenger is null)
            return RedirectToLogin();

        ViewBag.FirstName = genericUser.FirstName;
        ViewBag.WalletBalance = await _passenger.GetWalletBalanceAsync(genericUser.UserWalletId);
        
        
        var bookings = await _passenger.GetUpcomingBookingsAsync(genericUser.Id);
        return View(bookings);
    }
    
    public async Task<IActionResult> Profile()
    {
        if (!IsAuthenticated) return RedirectToLogin();

        var user = await _passenger.GetUserByIdAsync(UserId!.Value);
        if (user is null) return RedirectToLogin();

        if (user.Role == "Marshal")
            ViewBag.MarshalBankAccount = await _marshalService.GetBankAccountAsync(user.Id);

        return View(user);
    }
    
    [HttpPost("profile")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(UpdateProfileModel model)
    {
        if (!IsAuthenticated) return RedirectToLogin();

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please fix the errors below.";
            return RedirectToAction("Profile");
        }

        var ok = await _passenger.UpdateUserProfileAsync(UserId!.Value, model);
        TempData[ok ? "Success" : "Error"] = ok
            ? "Profile updated successfully."
            : "Failed to update profile.";

        return RedirectToAction("Profile");
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddBankAccount(string accountNumber, string bankCode)
    {
        var userId = HttpContext.Session.GetInt32("userId");
        if (userId == null) return RedirectToAction("Login", "Auth");

        var result = await _marshalService.AddBankAccountAsync(userId.Value, accountNumber, bankCode);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction("Profile");
    }

    /// <summary>
    /// Finishes wallet setup for a confirmed account whose wallet could not be
    /// created at sign-up (wallet service unavailable). Safe to retry.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateWallet()
    {
        if (!IsAuthenticated) return RedirectToLogin();

        var result = await _passenger.CreateWalletIfMissingAsync(UserId!.Value);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;

        if (HttpContext.Session.GetString("Role") == "Marshal")
            return RedirectToAction("Wallet", "Marshal");

        return RedirectToAction("Wallet");
    }

    public IActionResult Error(int statusCode = 500)
    {
        // Only the status code is accepted from the caller: echoing free text from
        // the query string would let anyone render arbitrary copy (e.g. a fake
        // "session expired, sign in here" page) on our own domain.
        var (code, title, detail) = statusCode switch
        {
            404 => (404, "Page not found",
                "The page you are looking for might have been removed, had its name changed, or is temporarily unavailable."),
            403 => (403, "Access denied", "You do not have permission to view this page."),
            503 => (503, "Service unavailable",
                "We're experiencing technical difficulties. Please try again in a few minutes."),
            _ => (500, "Something went wrong",
                "Something went wrong on our end. Please try again later or contact support if the problem persists.")
        };

        Response.StatusCode = code;
        ViewBag.StatusCode = code;
        ViewBag.Title = title;
        ViewBag.Detail = detail;
        return View();
    }
}