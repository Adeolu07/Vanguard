using _Tripfinity.Interfaces;
using _Tripfinity.Models.Data.Requests;
using _Tripfinity.Models.Tables;
using _Tripfinity.Services;
using Microsoft.AspNetCore.Mvc;

namespace _Tripfinity.Controllers;

public class MarshalController : ParentController
{
    private readonly ITripService _trip;
    private readonly ITicketService _ticket;
    private readonly IMarshalService _marshal;

    public MarshalController(ITripService trip, ITicketService ticket, IMarshalService marshal)
    {
        _trip = trip;
        _ticket = ticket;
        _marshal = marshal;
    }

    private async Task<User?> RequireMarshal() =>
        IsAuthenticated ? await _marshal.GetMarshalAsync(UserId!.Value) : null;

    public async Task<IActionResult> Index()
    {
        var marshal = await RequireMarshal();
        
        if (marshal is null)
            return RedirectToLogin();
        
        ViewBag.FirstName = marshal.FirstName;
        ViewBag.VehicleType = marshal.VehicleType;
        ViewBag.VehicleId = marshal.VehicleId;
        
        return View();
    }

    [HttpGet("marshal/create/{type}")]
    public async Task<IActionResult> CreateTrip(string type)
    {
        var marshal = await RequireMarshal();
        if (marshal is null)
            return RedirectToLogin();

        switch (type.ToLower())
        {
            case "bus":
                return View("CreateBus", new CreateBusTripRequest());
            case "railway":
                return View("CreateRailway", new CreateRailwayTripRequest());
            case "taxi":
                return View("CreateTaxi", new CreateTaxiTripRequest());
            default:
                return View("Error");
        }
    }

    [HttpPost("marshal/create/bus")]
    public async Task<IActionResult> CreateBus(CreateBusTripRequest request)
    {
        var marshal = await RequireMarshal();
        if (marshal is null) 
            return RedirectToLogin();
        
        if (!ModelState.IsValid) 
        { 
            TempData["Error"] = "Invalid input."; 
            return View("CreateBus", request); 
        }
        
        await _trip.CreateBusTripAsync(request, marshal.Id, marshal.VehicleId!);
        TempData["Success"] = "Bus trip created.";
        
        return RedirectToAction("MyTrips");
    }

    [HttpPost("marshal/create/railway")]
    public async Task<IActionResult> CreateRailway(CreateRailwayTripRequest request)
    {
        var marshal = await RequireMarshal();
        
        if (marshal is null) 
            return RedirectToLogin();

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Invalid input."; 
            return View("CreateRailway", request);
        }
        
        await _trip.CreateRailwayTripAsync(request, marshal.Id, marshal.VehicleId!);
        
        TempData["Success"] = "Railway trip created.";
        return RedirectToAction("MyTrips");
    }

    [HttpPost("marshal/create/taxi")]
    public async Task<IActionResult> CreateTaxi(CreateTaxiTripRequest req)
    {
        var marshal = await RequireMarshal();
        if (marshal is null) 
            return RedirectToLogin();
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Invalid input.";
            return View("CreateTaxi", req);
        }
        await _trip.CreateTaxiTripAsync(req, marshal.Id, marshal.VehicleId!);
        
        TempData["Success"] = "Taxi trip created.";
        return RedirectToAction("MyTrips");
    }

    [HttpGet("marshal/trips")]
    public async Task<IActionResult> MyTrips()
    {
        var m = await RequireMarshal();
        if (m is null) 
            return RedirectToLogin();
        
        ViewBag.VehicleType = m.VehicleType;
        var trips = await _marshal.GetMarshalTripsAsync(m.Id, m.VehicleType!);
        
        return View("MyTrips", trips);
    }

    [HttpPost("marshal/trips/cancel")]
    public async Task<IActionResult> CancelTrip(string transportType, int tripId, string reason)
    {
        var m = await RequireMarshal();
        if (m is null) 
            return RedirectToLogin();
        if (!Enum.TryParse<TransportType>(transportType, out var type))
        {
            TempData["Error"] = "Invalid type."; 
            return RedirectToAction("MyTrips"); 
        }
        
        var ok = await _trip.CancelTripAsync(type, tripId, m.Id, reason);
        
        TempData[ok ? "Success" : "Error"] = ok ? "Trip cancelled." : "Trip not found.";
        return RedirectToAction("MyTrips");
    }

    [HttpGet("marshal/scan")]
    public async Task<IActionResult> Scan() 
        => await RequireMarshal() is null ? RedirectToLogin() : View("Scan");

    [HttpPost("marshal/scan")]
    public async Task<IActionResult> Scan(string qrToken)
    {
        var marshal = await RequireMarshal();
        if (marshal is null) 
            return RedirectToLogin();
        if (string.IsNullOrWhiteSpace(qrToken))
        {
            TempData["Error"] = "No QR code.";
            return View("Scan");
        }
        var result = await _ticket.ValidateTicketAsync(qrToken, marshal.Id);

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