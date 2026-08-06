using System.Diagnostics;
using _Tripfinity.Interfaces;
using _Tripfinity.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace _Tripfinity.Controllers;

public class HomeController : ParentController
{
    private readonly IPassengerService _passenger;
    private readonly ILogger<HomeController> _logger;
    private readonly IWalletService _walletService;

    public HomeController(IPassengerService passenger, ILogger<HomeController> logger, IWalletService walletService)
    {
        _passenger = passenger;
        _logger = logger;
        _walletService = walletService;
    }

    public IActionResult Index()
    {
        if (IsAuthenticated)
            return RedirectToAction("Dashboard");
        return View();
    }
    public IActionResult Privacy() => View();
    
    public async Task<IActionResult> Wallet(int page = 1)
    {
        _logger.LogInformation("GET /Wallet");
        if (!IsAuthenticated)
            return RedirectToAction("Index", "Home");
        
        var user = await _passenger.GetPassengerAsync(UserId!.Value);
        // if (user is null) return RedirectToLogin();
        ViewBag.WalletId = user!.UserWalletId;
        var transactions = await _passenger.GetWalletTransactions(UserId!.Value, page);
        return View("~/Views/Wallet/Index.cshtml", transactions);
    }
    
    public async Task<IActionResult> Dashboard()
    {
        if (!IsAuthenticated) 
            return RedirectToLogin();

        var user = await _passenger.GetPassengerAsync(UserId!.Value);
        if (user is null) 
            return RedirectToLogin();

        ViewBag.FirstName = user.FirstName;
        ViewBag.WalletBalance = await _passenger.GetWalletBalanceAsync(user.UserWalletId);
        
        
        var bookings = await _passenger.GetUpcomingBookingsAsync(user.Id);
        return View(bookings);
    }
    
    public async Task<IActionResult> Profile()
    {
        if (!IsAuthenticated) return RedirectToLogin();

        var user = await _passenger.GetUserByIdAsync(UserId!.Value);
        if (user is null) return RedirectToLogin();

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
    
   

    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}