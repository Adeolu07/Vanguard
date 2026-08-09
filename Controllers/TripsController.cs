using _Tripfinity.Interfaces;
using _Tripfinity.Services;
using _Tripfinity.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace _Tripfinity.Controllers;

[Route("Trips/{type}")]
[RequireAuth]
public class TripsController : ParentController
{
    private readonly ITripListingService _listingService;
    private readonly IBookingService _bookingService;
    private readonly IWalletService _walletService;
    private readonly ILogger<TripsController> _logger;

    public TripsController(
        ITripListingService listingService,
        IBookingService bookingService,
        IWalletService walletService,
        ILogger<TripsController> logger)
    {
        _listingService = listingService;
        _bookingService = bookingService;
        _walletService = walletService;
        _logger = logger;
    }

    // GET /Trips/{type}
    public async Task<IActionResult> Index(string type, int page = 1, int pageSize = 4)
    {
        string? viewPath = type.ToLower() switch
        {
            "bus"     => "~/Views/BusTrips/Index.cshtml",
            "railway" => "~/Views/RailwayTrips/Index.cshtml",
            "taxi"    => "~/Views/TaxiTrips/Index.cshtml",
            _         => null
        };

        object? model = type.ToLower() switch
        {
            "bus"     => await _listingService.GetActiveBusTripsAsync(page, pageSize),
            "railway" => await _listingService.GetActiveRailwayTripsAsync(page, pageSize),
            "taxi"    => await _listingService.GetActiveTaxiTripsAsync(page, pageSize),
            _         => null
        };

        if (model is null || viewPath is null)
            return NotFound();

        return View(viewPath, model);
        
        
    }

    // GET /Trips/{type}/{tripId}/Book
    [HttpGet("{tripId}/Book")]
    public async Task<IActionResult> Book(string type, int tripId)
    {
        var trip = await _bookingService.GetTripAsync(type, tripId);
        if (trip == null) return NotFound();

        ViewBag.Trip = trip;
        ViewBag.UserId = UserId;
        return View($"~/Views/{type}Trips/Book.cshtml", trip);
    }

    // POST /Trips/{type}/{tripId}/Book
    [HttpPost("{tripId}/Book")]
    public async Task<IActionResult> Book(string type, int tripId, int seats)
    {
        _logger.LogInformation("POST: /Trips/{Type}/Book", type);

        var result = await _bookingService.BookAsync(type, tripId, seats, UserId!.Value);
        if (!result.Success)
        {
            TempData["Error"] = result.Message;
            return RedirectToAction("Book", new { type, tripId });
        }

        TempData["Success"] = "Booking confirmed";
        return RedirectToAction("Confirmation", new { type, id = result.Booking!.Id });
    }

    // GET /Trips/{type}/{id}/Confirmation
    [HttpGet("{id}/Confirmation")]
    public async Task<IActionResult> Confirmation(string type, int id)
    {
        var transport = Enum.Parse<TransportType>(type, true);
        var booking = await _bookingService.GetBookingAsync(id, transport);
        if (booking == null) return NotFound();

        ViewBag.PaymentTransaction = await FetchTransaction(_walletService, booking);
        return View(booking);
    }
}