using _Tripfinity.Interfaces;
using _Tripfinity.Models.Data;
using _Tripfinity.Models.Data.Requests;
using _Tripfinity.Models.Data.Response;
using _Tripfinity.Models.Enums;
using _Tripfinity.Models.Tables;
using _Tripfinity.Models.ViewModels;
using _Tripfinity.Services;
using _Tripfinity.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace _Tripfinity.Controllers;

[Route("marshal")]
[MarshalOnly]
public class MarshalController : Controller
{
    private readonly ITripService _trip;
    private readonly ITicketService _ticket;
    private readonly IMarshalService _marshalService;
    private readonly ILogger<MarshalController> _logger;
    private readonly AppDbContext _context;

    public MarshalController(ITripService trip, ITicketService ticket, IMarshalService marshalService,
        ILogger<MarshalController> logger, AppDbContext context)
    {
        _trip = trip;
        _ticket = ticket;
        _marshalService = marshalService;
        _logger = logger;
        _context = context;
    }

    private int? MarshalId => HttpContext.Session.GetInt32("marshalId");

    private string? MarshalVehicleType => HttpContext.Session.GetString("marshalVehicleType");
    private string? MarshalVehicleId => HttpContext.Session.GetString("marshalVehicleId");
    private IActionResult RedirectToMarshalLogin() => RedirectToAction("MarshalSignIn", "Auth");

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (MarshalId is null)
            return RedirectToMarshalLogin();
        
        var marshal = await _marshalService.GetMarshalAsync(MarshalId.Value);
        
        HttpContext.Session.SetInt32("userId", marshal!.Id);
        ViewBag.FirstName = marshal!.FirstName;
        ViewBag.VehicleType = marshal!.VehicleType;
        ViewBag.VehicleId = marshal!.VehicleId;
        return View();
    }

    [HttpGet("create/{type}")]
    public IActionResult CreateTrip(string type)
    {
        if (MarshalId is null)
            return RedirectToMarshalLogin();

        var now = DateTime.Now.AddTicks(-(DateTime.Now.Ticks % TimeSpan.TicksPerMinute));
        return type.ToLower() switch
        {
            "bus" => View("CreateBus", new CreateBusTripRequest { DepartureTime = now }),
            "railway" => View("CreateRailway", new CreateRailwayTripRequest { DepartureTime = now }),
            "taxi" => View("CreateTaxi", new CreateTaxiTripRequest { PickupTime = now }),
            _ => View("Error")
        };
    }

    [HttpPost("create/bus")]
    public async Task<IActionResult> CreateBus(CreateBusTripRequest request)
    {
        if (MarshalId is null || MarshalVehicleId is null)
            return RedirectToMarshalLogin();

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Invalid input.";
            return View("CreateBus", request);
        }

        await _trip.CreateBusTripAsync(request, MarshalId.Value, MarshalVehicleId);
        TempData["Success"] = "Bus trip created.";
        return RedirectToAction("MyTrips");
    }

    [HttpPost("create/railway")]
    public async Task<IActionResult> CreateRailway(CreateRailwayTripRequest request)
    {
        if (MarshalId is null || MarshalVehicleId is null)
            return RedirectToMarshalLogin();

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Invalid input.";
            return View("CreateRailway", request);
        }

        await _trip.CreateRailwayTripAsync(request, MarshalId.Value, MarshalVehicleId);
        TempData["Success"] = "Railway trip created.";
        return RedirectToAction("MyTrips");
    }

    [HttpPost("create/taxi")]
    public async Task<IActionResult> CreateTaxi(CreateTaxiTripRequest req)
    {
        if (MarshalId is null || MarshalVehicleId is null)
            return RedirectToMarshalLogin();

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Invalid input.";
            return View("CreateTaxi", req);
        }

        await _trip.CreateTaxiTripAsync(req, MarshalId.Value, MarshalVehicleId);
        TempData["Success"] = "Taxi trip created.";
        return RedirectToAction("MyTrips");
    }
    
    

    [HttpGet("trips")]
    public async Task<IActionResult> MyTrips()
    {
        if (MarshalId is null || MarshalVehicleType is null)
            return RedirectToMarshalLogin();

        ViewBag.VehicleType = MarshalVehicleType;
        var trips = await _marshalService.GetMarshalTripsAsync(MarshalId.Value, MarshalVehicleType);
        return View("MyTrips", trips ?? new List<object>());
    }

    [HttpPost("trips/cancel")]
    public async Task<IActionResult> CancelTrip(string transportType, int tripId, string reason)
    {
        if (MarshalId is null)
            return RedirectToMarshalLogin();

        if (!Enum.TryParse<TransportType>(transportType, out var type))
        {
            TempData["Error"] = "Invalid transport type.";
            return RedirectToAction("MyTrips");
        }

        var ok = await _trip.CancelTripAsync(type, tripId, MarshalId.Value, reason);
        TempData[ok ? "Success" : "Error"] = ok ? "Trip cancelled." : "Trip not found.";
        return RedirectToAction("MyTrips");
    }

    [HttpGet("scan")]
    public IActionResult Scan()
    {
        if (MarshalId is null)
            return RedirectToMarshalLogin();
        return View("Scan");
    }

    [HttpPost("scan")]
    public async Task<IActionResult> Scan(string qrToken)
    {
        if (MarshalId is null)
            return RedirectToMarshalLogin();

        if (string.IsNullOrWhiteSpace(qrToken))
        {
            TempData["Error"] = "No QR code provided.";
            return View("Scan");
        }
        
        var result = await _ticket.ValidateTicketAsync(qrToken, MarshalId.Value, MarshalVehicleId!);

        if (!result.Success)
        {
            // Handle duplicate scans separately for clearer feedback
            if (result.Ticket?.Status == TicketStatus.Validated)
            {
                TempData["Error"] = $"{result.Message} (Validated by Marshal {result.Ticket.ValidatedByMarshalId})";
                ViewBag.ValidatedTicket = result.Ticket;
            }
            else
            {
                TempData["Error"] = result.Message;
            }
        }
        else
        {
            TempData["Success"] = $"Ticket {result.Ticket!.TicketReference} validated successfully!";
            ViewBag.ValidatedTicket = result.Ticket;
        }

        return View("Scan");
    }

    [HttpGet("wallet")]
    public async Task<IActionResult> Wallet(int page = 1)
    {
        var userId = HttpContext.Session.GetInt32("userId");
        if (userId == null) 
            return RedirectToAction("MarshalSignIn", "Auth");

        var model = await _marshalService.GetWalletInfoAsync(userId.Value, page);
        
        if (string.IsNullOrEmpty(model.WalletId))
        {
            // Wallet not ready yet – build minimal fallback
            var marshal = await _marshalService.GetMarshalAsync(userId.Value) ?? new User
            {
                FirstName = "Marshal",
                LastName = "",
                Email = "",
            };

            model = new MarshalWalletViewModel
            {
                WalletId = marshal.UserWalletId ?? "Not available",
                Balance = 0,
                Transactions = new List<TransactionDetailsList>(),
                CurrentPage = 1,
                TotalPages = 1,
                HasNext = false,
                HasPrevious = false
            };

            TempData["ErrorMessage"] = "Your wallet is being set up. Please check back soon, or contact support if this persists.";
        }

        return View(model);
    }
    
    [HttpGet("trips/{id:int}")]
public async Task<IActionResult> TripDetail(int id)
{
    if (MarshalId is null || MarshalVehicleType is null)
        return RedirectToMarshalLogin();

    var type = MarshalVehicleType;
    var marshalId = MarshalId.Value;

    object? trip;
    IEnumerable<Booking> bookings;
    switch (type.ToLower())
    {
        case "bus":
            trip = await _context.BusTrips.FirstOrDefaultAsync(t => t.Id == id && t.MarshalId == marshalId);
            if (trip == null) return NotFound();
            // bookings for this trip
            bookings = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.BusTrip)
                .Where(b => b.BusTripId == id)
                .ToListAsync();
            break;
        case "railway":
            trip = await _context.RailwayTrips.FirstOrDefaultAsync(t => t.Id == id && t.MarshalId == marshalId);
            if (trip == null) return NotFound();
            bookings = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.RailwayTrip)
                .Where(b => b.RailwayTripId == id)
                .ToListAsync();
            break;
        case "taxi":
            trip = await _context.TaxiTrips.FirstOrDefaultAsync(t => t.Id == id && t.MarshalId == marshalId);
            if (trip == null) return NotFound();
            bookings = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.TaxiTrip)
                .Where(b => b.TaxiTripId == id)
                .ToListAsync();
            break;
        default:
            return BadRequest("Unknown vehicle type");
    }

    var model = new TripDetailViewModel
    {
        TripId = id,
        TransportType = type,
        Route = trip switch
        {
            BusTrip b => $"{b.From} → {b.Destination}",
            RailwayTrip r => $"{r.From} → {r.Destination}",
            TaxiTrip t => $"{t.PickupLocation} → {t.DropoffLocation}",
            _ => ""
        },
        DepartureTime = trip switch
        {
            BusTrip b => b.DepartureTime,
            RailwayTrip r => r.DepartureTime,
            TaxiTrip t => t.PickupTime,
            _ => DateTime.MinValue
        },
        Status = trip switch
        {
            BusTrip b => b.Status.ToString(),
            RailwayTrip r => r.Status.ToString(),
            TaxiTrip t => t.Status.ToString(),
            _ => ""
        },
        Passengers = bookings.Select(b => new TripPassenger
        {
            PassengerName = b.User != null ? $"{b.User.FirstName} {b.User.LastName}" : "Unknown",
            Seats = b.NumberOfSeats,
            BookingStatus = b.Status.ToString(),
            HasTicket = _context.Tickets.Any(t => t.BookingId == b.Id),
            TicketStatus = _context.Tickets
                .Where(t => t.BookingId == b.Id)
                .Select(t => t.Status.ToString())
                .FirstOrDefault() ?? "None"
        }).ToList()
    };

    return View("TripDetail", model);
}

    [HttpPost("trips/{id:int}/commence")]
    public async Task<IActionResult> CommenceTrip(int id)
    {
        if (MarshalId is null || MarshalVehicleType is null)
            return RedirectToMarshalLogin();

        if (!Enum.TryParse<TransportType>(MarshalVehicleType, out var transportType))
            return BadRequest("Invalid vehicle type");

        var success = await _trip.CommenceTripAsync(transportType, id, MarshalId.Value);
        if (!success)
        {
            TempData["Error"] = "Unable to commence trip. It may already be in progress or cancelled.";
            return RedirectToAction("TripDetail", new { id });
        }

        TempData["Success"] = "Trip commenced successfully. Unvalidated tickets are now expired.";
        return RedirectToAction("TripDetail", new { id });
    }
    
    // Add after the CommenceTrip action:

    [HttpPost("trips/{id:int}/end")]
    public async Task<IActionResult> EndTrip(int id)
    {
        if (MarshalId is null || MarshalVehicleType is null)
            return RedirectToMarshalLogin();

        if (!Enum.TryParse<TransportType>(MarshalVehicleType, out var transportType))
            return BadRequest("Invalid vehicle type");

        var success = await _trip.EndTripAsync(transportType, id, MarshalId.Value);
        if (!success)
        {
            TempData["Error"] = "Unable to end trip. It may already be completed or was never commenced.";
            return RedirectToAction("TripDetail", new { id });
        }

        TempData["Success"] = "Trip ended successfully.";
        return RedirectToAction("TripDetail", new { id });
    }

}