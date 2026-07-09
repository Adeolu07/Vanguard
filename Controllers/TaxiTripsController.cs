using _Tripfinity.Interfaces;
using _Tripfinity.Models.Data;
using _Tripfinity.Services;
using Microsoft.AspNetCore.Mvc;

namespace _Tripfinity.Controllers;
public class TaxiTripsController : ParentController
{
    private readonly IBookingService _bookingService;
    private readonly AppDbContext _context;
    private readonly IWalletService _walletService;
    private readonly ILogger<TaxiTripsController> _logger;
    

    public TaxiTripsController(AppDbContext context, IBookingService bookingService, IWalletService walletService,  ILogger<TaxiTripsController> logger)
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
        _logger.LogInformation("GET: /Book taxi");
        
        var trip = await _context.TaxiTrips.FindAsync(tripId);
        if (trip == null)
        {
            _logger.LogWarning("Taxi trip not found.");
            return NotFound();
        }
        ViewBag.Trip = trip;
        ViewBag.UserId = UserId;
        return View("Book",trip);
    }

    [HttpPost]
    public async Task<IActionResult> Book(int tripId, int seats)
    {
        _logger.LogInformation("POST: /Book taxi");
        if (!IsAuthenticated)
        {
            _logger.LogWarning("Not authenticated.");
           return RedirectToLogin();
        } 
        var result = await _bookingService.BookTaxiAsync(tripId, seats, UserId!.Value);
        if (!result.Success)
        {
            _logger.LogWarning("Taxi booking error.");
            TempData["Error"] = result.Message;
            return RedirectToAction("Book", new{tripId});
        }
        TempData["Success"] = "Booking confirmed";
        return RedirectToAction("Confirmation", new{id = result.Booking!.Id});
    }

    [HttpGet]
    public async Task<IActionResult> Confirmation(int id)
    {
        _logger.LogInformation("GET: /Confirmation taxi");
        if (!IsAuthenticated)
        {
            _logger.LogWarning("Not authenticated.");
            return RedirectToLogin();
        }

        var booking = await _bookingService.GetBookingAsync(id, TransportType.Taxi);
        if (booking == null)
        {
            _logger.LogWarning("Taxi trip not found.");
            return NotFound();
        }
        ViewBag.PaymentTransaction = await FetchTransaction(_walletService, booking);
        return View("Confirmation",booking);
    }
}
