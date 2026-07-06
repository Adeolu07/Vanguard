using System.Diagnostics;
using _Tripfinity.Interfaces;
using _Tripfinity.Models.ViewModels;
using _Tripfinity.Models.Data;
using _Tripfinity.Models.Data.Requests;   
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace _Tripfinity.Controllers;

public class HomeController : ParentController
{
    private readonly AppDbContext _context;
    private readonly IBookingService _bookingService;
    private readonly IWalletService _walletService;   
    private readonly ILogger<HomeController> _logger;

    public HomeController(AppDbContext context, IBookingService bookingService, IWalletService walletService, ILogger<HomeController> logger)
    {
        _context = context;
        _bookingService = bookingService;
        _walletService = walletService;
        _logger = logger;
    }

    public IActionResult Index() => View();
    public IActionResult Privacy() => View();

    [HttpGet]
    public IActionResult Wallet()
    {
        _logger.LogInformation("GET /Wallet");
        if (!IsAuthenticated)
        {
            _logger.LogInformation("Not Authenticated");
            return RedirectToAction("Index", "Home");
        }
        return View("~/Views/Wallet/Index.cshtml");
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        _logger.LogInformation("GET /Dashboard");
        if (!IsAuthenticated)
        {
            _logger.LogInformation("Not Authenticated");
            return RedirectToLogin();
        }

       
        var user = await _context.Users.FindAsync(UserId!.Value);

        if (user == null)
        {
            _logger.LogInformation("User not found");
            return RedirectToLogin();
        }

        if (user.Role != "Passenger")
        {
            _logger.LogWarning("User is not Passenger");
            return RedirectToAction("Index", "Marshal");
        }
        
        ViewBag.FirstName = user.FirstName ?? "Passenger";

        var balanceResponse = await _walletService.GetBalanceAsync(
            new GetBalanceRequest { CustomerId = user.UserWalletId }
        );
        ViewBag.WalletBalance = balanceResponse?.Balance ?? 0m; 

        var madeBookings = await _context.Bookings
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
