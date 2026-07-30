using _Tripfinity.Interfaces;
using _Tripfinity.Models.Data.Requests;
using _Tripfinity.Services;
using _Tripfinity.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace _Tripfinity.Controllers;

[Route("marshal")]
[MarshalOnly]
public class MarshalController : Controller
{
    private readonly ITripService _trip;
    private readonly ITicketService _ticket;
    private readonly IMarshalService _marshal;
    private readonly ILogger<MarshalController> _logger;

    public MarshalController(ITripService trip, ITicketService ticket, IMarshalService marshal,
        ILogger<MarshalController> logger)
    {
        _trip = trip;
        _ticket = ticket;
        _marshal = marshal;
        _logger = logger;
    }

    private int? MarshalId => HttpContext.Session.GetInt32("marshalId");
    private string? MarshalVehicleType => HttpContext.Session.GetString("marshalVehicleType");
    private string? MarshalVehicleId => HttpContext.Session.GetString("marshalVehicleId");
    private IActionResult RedirectToMarshalLogin() => RedirectToAction("MarshalSignIn", "Auth");

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        if (MarshalId is null)
            return RedirectToMarshalLogin();

        var marshal = await _marshal.GetMarshalAsync(MarshalId.Value);
        if (marshal is null)
            return RedirectToMarshalLogin();

        ViewBag.FirstName = marshal.FirstName;
        ViewBag.VehicleType = marshal.VehicleType;
        ViewBag.VehicleId = marshal.VehicleId;
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
        var trips = await _marshal.GetMarshalTripsAsync(MarshalId.Value, MarshalVehicleType);
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

        var result = await _ticket.ValidateTicketAsync(qrToken, MarshalId.Value);
        if (!result.Success)
        {
            TempData["Error"] = result.Message;
        }
        else
        {
            TempData["Success"] = $"Ticket {result.Ticket!.TicketReference} validated!";
            ViewBag.ValidatedTicket = result.Ticket;
        }

        return View("Scan");
    }
}