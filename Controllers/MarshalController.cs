using System.Net;
using _Tripfinity.Interfaces;
using _Tripfinity.Models.Data.Requests;
using _Tripfinity.Models.Enums;
using _Tripfinity.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace _Tripfinity.Controllers;

[Route("marshal")]
[MarshalOnly]
public class MarshalController : Controller
{
    private readonly ITripService _tripService;
    private readonly ITicketService _ticketService;
    private readonly IMarshalService _marshalService;
    private readonly IFastChannelService _fastChannelService;

    public MarshalController(ITripService tripService, ITicketService ticketService, IMarshalService marshalService,
        IFastChannelService fastChannelService)
    {
        _tripService = tripService;;
        _ticketService = ticketService;
        _marshalService = marshalService;
        _fastChannelService = fastChannelService;
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

        var dashboard = await _marshalService.GetMarshalDashboardAsync(MarshalId.Value);
        if (dashboard is null)
            return RedirectToMarshalLogin();
        
        HttpContext.Session.SetInt32("userId", dashboard!.MarshalId);
        ViewBag.FirstName = dashboard!.FirstName;
        ViewBag.VehicleType = dashboard!.VehicleType;
        ViewBag.VehicleId = dashboard!.VehicleId;
        return View(dashboard);
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

        var busTrip = await _tripService.CreateBusTripAsync(request, MarshalId.Value, MarshalVehicleId);
        if (busTrip is null)
        {
            TempData["Error"] = "Available seats cannot exceed total seats";
            return View("CreateBus", request);
        }
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

        var railwayTrip = await _tripService.CreateRailwayTripAsync(request, MarshalId.Value, MarshalVehicleId);
        if (railwayTrip is null)
        {
            TempData["Error"] = "Available seats cannot exceed total seats.";
            return View("CreateRailway", request);
        }
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

        var taxiTrip = await _tripService.CreateTaxiTripAsync(req, MarshalId.Value, MarshalVehicleId);
        if (taxiTrip is null)
        {
            TempData["Error"] = "Passengers cannot exceed 4.";
            return View("CreateTaxi", req);
        }
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

        var ok = await _tripService.CancelTripAsync(type, tripId, MarshalId.Value, reason);
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
        
        var result = await _ticketService.ValidateTicketAsync(qrToken, MarshalId.Value, MarshalVehicleId!);

        if (!result.Success)
        { // Handle duplicate scans separately for clearer feedback
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
        return View(model);
    }
    
    [HttpGet("wallet/bankaccount")]
    public async Task<IActionResult> BankAccount()
    {
        var userId = HttpContext.Session.GetInt32("userId");
        if (userId == null) return Unauthorized(new { message = "Not signed in." });

        var account = await _marshalService.GetBankAccountAsync(userId.Value);
        if (account == null) return NotFound(new { message = "No bank account linked. Add one from your profile." });

        return Ok(new
        {
            accountNumber = account.AccountNumber,
            accountName = account.AccountName,
            bankName = account.BankName,
            bankCode = account.BankCode
        });
    }
    
        [HttpPost("wallet/cashout")]
        public async Task<IActionResult> CashOut([FromBody] CashoutRequest request)
        {
            var userId = HttpContext.Session.GetInt32("userId");
            if (userId == null) return Unauthorized(new { message = "Not signed in." });

            if (request.Amount <= 0) return BadRequest(new { message = "Enter a valid amount" });

            // Server-side balance check — never trust the amount from the client.
            var wallet = await _marshalService.GetWalletInfoAsync(userId.Value);

            if (wallet.Balance < request.Amount)
                return BadRequest(new { message = "Insufficient balance." });

            var account = await _marshalService.GetBankAccountAsync(userId.Value);

            if (account is null)
                return BadRequest(new { message = "No bank account is linked. Add one from your profile" });

            // The FastChannel service builds traceId, timestamp and signature internally.
            var details = new TransactionDetails
            {
                CreditAccount = account.AccountNumber,
                CreditAccountName = account.AccountName,
                CreditBankCode = account.BankCode,
                Narration = "Marshal wallet cashout",
                Amount = request.Amount
            };

            var (statusCode, response) = await _fastChannelService.SinglePostAsync(details);
            if (statusCode != HttpStatusCode.OK || response?.ResponseHeader?.ResponseCode != "00")
                return BadRequest(new { message = response?.ResponseHeader?.ResponseMessage ?? "Cash out failed." });

            var result = await _marshalService.CashOutAsync(userId.Value, request.Amount);
            if (!result.Success) return BadRequest(new { message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }
    
        [HttpGet("trips/{id:int}")]
        public async Task<IActionResult> TripDetail(int id)
        {
            if (MarshalId is null || MarshalVehicleType is null) return RedirectToMarshalLogin();

            var model = await _marshalService.GetTripDetailAsync(id, MarshalId.Value, MarshalVehicleType);
            if (model is null) return NotFound();

            return View("TripDetail", model);
        }
        
        [HttpPost("trips/{id:int}/commence")]
        public async Task<IActionResult> CommenceTrip(int id)
        {
            if (MarshalId is null || MarshalVehicleType is null) return RedirectToMarshalLogin();
            if (!Enum.TryParse<TransportType>(MarshalVehicleType, out var transportType)) return BadRequest("Invalid vehicle type");

            var success = await _tripService.CommenceTripAsync(transportType, id, MarshalId.Value);
            if (!success)
            {
                TempData["Error"] = "Unable to commence trip. It may already be in progress or cancelled.";
                return RedirectToAction("TripDetail", new { id });
            }

            TempData["Success"] = "Trip commenced successfully. Unvalidated tickets are now expired.";
            return RedirectToAction("TripDetail", new { id });
        }
        
    
        [HttpPost("trips/{id:int}/end")]
        public async Task<IActionResult> EndTrip(int id)
        {
            if (MarshalId is null || MarshalVehicleType is null) return RedirectToMarshalLogin();
            if (!Enum.TryParse<TransportType>(MarshalVehicleType, out var transportType)) return BadRequest("Invalid vehicle type");

            var success = await _tripService.EndTripAsync(transportType, id, MarshalId.Value);
            if (!success)
            {
                TempData["Error"] = "Unable to end trip. It may already be completed or was never commenced.";
                return RedirectToAction("TripDetail", new { id });
            }

            TempData["Success"] = "Trip ended successfully.";
            return RedirectToAction("TripDetail", new { id });
        }
}