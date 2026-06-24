using _Tripfinity.Interfaces;
using _Tripfinity.Models.Data;
using _Tripfinity.Models.Data.Requests;
using Microsoft.AspNetCore.Mvc;

namespace _Tripfinity.Controllers;

public class RailwayTripsController : ParentController
{
    private readonly IBookingService _bookingService; 
    private readonly AppDbContext _context;
    private readonly IWalletService _walletService;
    private readonly ILogger<RailwayTripsController> _logger;

    public RailwayTripsController(
        AppDbContext context,
        IBookingService bookingService,
        IWalletService walletService, ILogger<RailwayTripsController> logger)
    {
        _context = context;
        _bookingService = bookingService;
        _walletService = walletService;
        _logger = logger;
    }

    public IActionResult Index()
    {
        // return !isAuthenticated ? RedirectToLogin() : View();
        _logger.LogInformation("GET: /");
        if (!IsAuthenticated)
        {
            _logger.LogWarning("User not authenticated.");
            return RedirectToLogin();
        }
        _logger.LogInformation("User is authenticated.");
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Book(int tripId)
    {
        if (!IsAuthenticated)
        {
            _logger.LogWarning("Not authenticated.");
            return RedirectToLogin();
        }
        _logger.LogInformation("GET: /Book train");
        
        var trip = await _context.BusTrips.FindAsync(tripId);
        if (trip == null)
        {
            _logger.LogWarning("Train trip not found.");
            return NotFound();
        }
        ViewBag.UserId = UserId;
        return View("Book",trip);
    }

    [HttpPost]
    public async Task<IActionResult> Book(int tripId, int seats)
    {
        _logger.LogInformation("POST: /Book train");
        if (!IsAuthenticated)
        {
            _logger.LogWarning("Not authenticated.");
            return RedirectToLogin();
        } 
        var result = await _bookingService.BookRailwayAsync(tripId, seats, UserId!.Value);
        if (!result.Success)
        {
            _logger.LogWarning("Train booking error.");
            TempData["Error"] = result.Message;
            return RedirectToAction("Book", new{tripId});
        }
        TempData["Success"] = "Booking confirmed";
        return RedirectToAction("Confirmation", new{id = result.Booking!.Id});
    }

    [HttpGet]
    public async Task<IActionResult> Confirmation(int id)
    {
        _logger.LogInformation("GET: /Confirmation train");
        if (!IsAuthenticated)
        {
            _logger.LogWarning("Not authenticated.");
            return RedirectToLogin();
        }

        var booking = await _bookingService.GetBookingAsync(id, "Train");
        if (booking == null)
        {
            _logger.LogWarning("Train trip not found.");
            return NotFound();
        }
        ViewBag.PaymentTransaction = await FetchTransaction(_walletService, booking);
        return View("Confirmation",booking);
    }
}
