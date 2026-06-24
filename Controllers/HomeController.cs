using System.Diagnostics;
using _Tripfinity.Interfaces;
using _Tripfinity.Models.ViewModels;
using _Tripfinity.Models.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace _Tripfinity.Controllers;

public class HomeController : ParentController
{
    private readonly AppDbContext _context;
    private readonly IBookingService _bookingService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(AppDbContext context, IBookingService bookingService,  ILogger<HomeController> logger)
    {
        _context = context;
        _bookingService = bookingService;
        _logger = logger;
    }
    
    public IActionResult Index() => View();
    public IActionResult Privacy() => View();

    // [Route("/home")]
    // [Route("/")]

    [HttpGet]
    public IActionResult Wallet()
    {
        _logger.LogInformation("GET /Wallet");
        if (!isAuthenticated)
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
        if (!isAuthenticated)
        {
            _logger.LogInformation("Not Authenticated");
            return RedirectToLogin();
        }
        
        var user = await _context.Users.FindAsync(UserId!.Value);
        ViewBag.FirstName = user?.FirstName ?? "Passenger";
        var madeBookings = await _context.Bookings
            .Include(b => b.BusTrip)
            .Include(b => b.TaxiTrip)
            .Include(b => b.RailwayTrip)
            .Where(b => b.UserId == UserId.Value)
            .OrderByDescending(b => b.BookingDate)
            .Take(10)
            .ToListAsync();
        return View(madeBookings);
    }


    // [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }

    
}