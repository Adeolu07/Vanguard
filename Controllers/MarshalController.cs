using _Tripfinity.Interfaces;
using _Tripfinity.Models.Data.Requests;
using _Tripfinity.Models.Data.Response;
using _Tripfinity.Models.Tables;
using _Tripfinity.Models.ViewModels;
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
    private readonly IMarshalService _marshalService;
    private readonly ILogger<MarshalController> _logger;
    private readonly ICipService _cipService;
    private const string InstitutionId = "000966";

    public MarshalController(ITripService trip, ITicketService ticket, ICipService cipService, IMarshalService marshalService,
        ILogger<MarshalController> logger)
    {
        _trip = trip;
        _ticket = ticket;
        _cipService = cipService;
        _marshalService = marshalService;
        _logger = logger;
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
    
    
    
   
    
    [HttpPost]
    public async Task<IActionResult> NameEnquiry(string bankCode, string accountNumber)
    {
        var userId = HttpContext.Session.GetInt32("userId");
        if (userId == null) return Unauthorized();

        var sessionId = Guid.NewGuid().ToString("N");
        var request = new NameEnquiryRequest
        {
            SessionId = sessionId,
            DestinationInstitutionId = bankCode,
            AccountId = accountNumber
        };

        var result = await _cipService.AccountEnquiry(request);
        return Json(new { success = result.ResponseCode == "00", accountName = result.AccountName, message = result.ResponseMessage });
    }

    [HttpGet("wallet")]
    public async Task<IActionResult> Wallet(int page = 1)
    {
        var userId = HttpContext.Session.GetInt32("userId");
        if (userId == null) 
            return RedirectToAction("MarshalSignIn", "Auth");

        var model = await _marshalService.GetWalletInfoAsync(userId.Value, page);
        if (model == null)
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
                Transactions = new List<_Tripfinity.Models.Data.Response.TransactionDetailsList>(),
                CurrentPage = 1,
                TotalPages = 1,
                HasNext = false,
                HasPrevious = false
            };

            TempData["ErrorMessage"] = "Your wallet is being set up. Please check back soon, or contact support if this persists.";
        }

        return View(model);
    }
    
    [HttpPost]
    public async Task<IActionResult> Cashout(decimal amount, string bankCode, string accountNumber, string accountName)
    {
        var userId = HttpContext.Session.GetInt32("userId");
        if (userId == null) return RedirectToAction("MarshalSignIn", "Auth");

        var marshal = await _marshalService.GetMarshalAsync(userId.Value);
        if (marshal == null || string.IsNullOrEmpty(marshal.UserWalletId))
            return RedirectToAction("MarshalSignIn", "Auth");

        var sessionId = Guid.NewGuid().ToString("N");
        var paymentRef = Guid.NewGuid().ToString("N");

        var postCreditRequest = new PostCreditRequest
        {
            SessionId = sessionId,
            PaymentRef = paymentRef,
            DestinationInstitutionId = bankCode,
            CreditAccount = accountNumber,
            CreditAccountName = accountName,
            SourceAccountId = marshal.UserWalletId,
            SourceAccountName = $"{marshal.FirstName} {marshal.LastName}",
            Narration = $"Tripfinity marshal cashout",
            Channel = "Online",
            Group = "Tripfinity",
            Sector = "Transport",
            Amount = amount
        };

        var result = await _cipService.PostCredit(postCreditRequest);
        if (result.ResponseCode == "00")
        {
            TempData["SuccessMessage"] = $"Cashout of ₦{amount:N2} to {accountName} ({accountNumber}) initiated successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = $"Cashout failed: {result.ResponseMessage}";
        }

        return RedirectToAction("Wallet");
    }
    
    

    private static string GenerateSessionId()
    {
        var timestamp = DateTime.Now.ToString("yyMMddHHmmss");
        var random = Random.Shared.Next(100_000_000, 999_999_999).ToString("D12")[..12];
        return $"{InstitutionId}{timestamp}{random}";
    }

}