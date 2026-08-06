using System.Diagnostics;
using _Tripfinity.Interfaces;
using _Tripfinity.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace _Tripfinity.Controllers;

public class HomeController : ParentController
{
    private readonly IPassengerService _passenger;
    private readonly ILogger<HomeController> _logger;

    public HomeController(IPassengerService passenger, ILogger<HomeController> logger)
    {
        _passenger = passenger;
        _logger = logger;
    }

    public IActionResult Index()
    {
        if (IsAuthenticated)
            return RedirectToAction("Dashboard");
        return View();
    }
    public IActionResult Privacy() => View();

    [HttpGet]
    public async Task<IActionResult> Wallet()
    {
        _logger.LogInformation("GET /Wallet");
        if (!IsAuthenticated)
            return RedirectToAction("Index", "Home");

        var user = await _passenger.GetPassengerAsync(UserId!.Value);
        ViewBag.WalletId = user!.UserWalletId;

        return View("~/Views/Wallet/Index.cshtml");
    }

    [HttpGet]
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

    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}