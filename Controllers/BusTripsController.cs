using _Tripfinity.Interfaces;
using _Tripfinity.Models.Data;
using _Tripfinity.Models.Data.Requests;
using Microsoft.AspNetCore.Mvc;

namespace _Tripfinity.Controllers;

public class BusTripsController : ParentController
{
    private readonly IBookingService _bookingService;
    private readonly AppDbContext _context;
    private readonly IWalletService _walletService;
    private readonly ILogger<BusTripsController> _logger;
    

    public BusTripsController(AppDbContext context, IBookingService bookingService, IWalletService walletService,  ILogger<BusTripsController> logger)
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
        if (!isAuthenticated)
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
        if (!isAuthenticated)
        {
            _logger.LogWarning("Not authenticated.");
            return RedirectToLogin();
        }
        _logger.LogInformation("GET: /Book bus");
        
        var trip = await _context.BusTrips.FindAsync(tripId);
        if (trip == null)
        {
            _logger.LogWarning("Bus trip not found.");
            return NotFound();
        }
        ViewBag.UserId = UserId;
        return View("Book",trip);
    }

    [HttpPost]
    public async Task<IActionResult> Book(int tripId, int seats)
    {
        _logger.LogInformation("POST: /Book bus");
        if (!isAuthenticated)
        {
            _logger.LogWarning("Not authenticated.");
           return RedirectToLogin();
        } 
        var result = await _bookingService.BookBusAsync(tripId, seats, UserId!.Value);
        if (!result.Success)
        {
            _logger.LogWarning("Bus booking error.");
            TempData["Error"] = result.Message;
            return RedirectToAction("Book", new{tripId});
        }
        TempData["Success"] = "Booking confirmed";
        return RedirectToAction("Confirmation", new{id = result.Booking!.Id});
    }

    [HttpGet]
    public async Task<IActionResult> Confirmation(int id)
    {
        _logger.LogInformation("GET: /Confirmation bus");
        if (!isAuthenticated)
        {
            _logger.LogWarning("Not authenticated.");
            return RedirectToLogin();
        }

        var booking = await _bookingService.GetBookingAsync(id, "Bus");
        if (booking == null)
        {
            _logger.LogWarning("Bus trip not found.");
            return NotFound();
        }
        ViewBag.PaymentTransaction = await FetchTransaction(_walletService, booking);
        return View("Confirmation",booking);
    }
}
