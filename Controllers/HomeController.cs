using System.Diagnostics;
using _Tripfinity.Interfaces;
using _Tripfinity.Models.ViewModels;
using _Tripfinity.Models.Data;
using _Tripfinity.Models.Data.Requests;   
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace _Tripfinity.Controllers;

public class HomeController(AppDbContext context, IWalletService walletService, ILogger<HomeController> logger)
    : ParentController
{
    public IActionResult Index() => View();
    public IActionResult Privacy() => View();

    [HttpGet]
    public IActionResult Wallet()
    {
        logger.LogInformation("GET /Wallet");
        if (IsAuthenticated) return View("~/Views/Wallet/Index.cshtml");
        logger.LogInformation("Not Authenticated");
        return RedirectToAction("Index", "Home");
    }
    
    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        logger.LogInformation("GET /Dashboard");
        if (!IsAuthenticated)
        {
            logger.LogInformation("Not Authenticated");
            return RedirectToLogin();
        }
        var user = await context.Users.FindAsync(UserId!.Value);
        if (user == null)
        {
            logger.LogInformation("User not found");
            return RedirectToLogin();
        }

        if (user.Role != "Passenger")
        {
            logger.LogWarning("User is not Passenger");
            return RedirectToAction("Index", "Marshal");
        }
        
        ViewBag.FirstName = user.FirstName;

        var balanceResponse = await walletService.GetBalanceAsync(
            new GetBalanceRequest { CustomerId = user.UserWalletId! }
        );
        ViewBag.WalletBalance = balanceResponse.Balance; 

        var madeBookings = await context.Bookings
            .Include(b => b.BusTrip)
            .Include(b => b.TaxiTrip)
            .Include(b => b.RailwayTrip)
            .Where(b => b.UserId == UserId.Value && b.BookingDate > DateTime.Now)
            .OrderByDescending(b => b.BookingDate)
            .Take(10)
            .ToListAsync();

        return View(madeBookings);
    }

    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}
