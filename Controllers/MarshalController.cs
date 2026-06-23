using _Tripfinity.Interfaces;
using _Tripfinity.Models;
using _Tripfinity.Models.Data;
using _Tripfinity.Models.Data.Requests;
using _Tripfinity.Models.Tables;
using _Tripfinity.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace _Tripfinity.Controllers;

[ApiController]
[Route("api/marshal")]
public class MarshalController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ITripService _tripService;
    private readonly ITicketService _ticketService;
    private readonly AppDbContext _context;
    private readonly ILogger<MarshalController> _logger;

    public MarshalController(
        IAuthService authService,
        ITripService tripService,
        ITicketService ticketService,
        AppDbContext context,
        ILogger<MarshalController> logger)
    {
        _authService = authService;
        _tripService = tripService;
        _ticketService = ticketService;
        _context = context;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] MarshalRegisterRequest request)
    {
        var result = await _authService.RegisterMarshalAsync(request);

        if (!result.Success)
            return BadRequest(new ErrorResponse { Success = false, Message = result.Message, ErrorCode = "400" });

        return Ok(new
        {
            success = true,
            message = result.Message,
            data = new
            {
                marshalId = result.User!.Id,
                vehicleId = result.User.VehicleId,
                result.User.VehicleType,
                result.User.Email
            }
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.SignInAsync(request.Email, request.Password);

        if (result is null || !result.Success)
            return Unauthorized(new ErrorResponse
            {
                Success = false,
                Message = result?.Message ?? "Invalid credentials",
                ErrorCode = "401"
            });

        if (result.User!.Role != "Marshal")
            return Unauthorized(new ErrorResponse
            {
                Success = false,
                Message = "This account is not a marshal account",
                ErrorCode = "401"
            });

        _authService.SetUserSession(HttpContext, result.User);

        return Ok(new
        {
            success = true,
            message = "Marshal signed in",
            data = new { marshalId = result.User.Id, result.User.VehicleId, result.User.VehicleType }
        });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        _authService.ClearUserSession(HttpContext);
        return Ok(new { success = true, message = "Signed out" });
    }

    [HttpPost("trips/bus")]
    public async Task<IActionResult> CreateBusTrip([FromBody] BusTrip trip)
    {
        var marshal = await GetMarshalAsync();
        if (marshal == null) return MarshalUnauthorized();

        var created = await _tripService.CreateBusTripAsync(trip);
        return Ok(new { success = true, message = "Bus trip created", data = created });
    }

    [HttpPost("trips/railway")]
    public async Task<IActionResult> CreateRailwayTrip([FromBody] RailwayTrip trip)
    {
        var marshal = await GetMarshalAsync();
        if (marshal == null) return MarshalUnauthorized();

        var created = await _tripService.CreateRailwayTripAsync(trip);
        return Ok(new { success = true, message = "Railway trip created", data = created });
    }

    [HttpPost("trips/taxi")]
    public async Task<IActionResult> CreateTaxiTrip([FromBody] TaxiTrip trip)
    {
        var marshal = await GetMarshalAsync();
        if (marshal == null) return MarshalUnauthorized();

        var created = await _tripService.CreateTaxiTripAsync(trip);
        return Ok(new { success = true, message = "Taxi trip created", data = created });
    }

    [HttpPost("trips/cancel")]
    public async Task<IActionResult> CancelTrip([FromBody] CancelTripRequest request)
    {
        var marshal = await GetMarshalAsync();
        if (marshal == null) 
            return MarshalUnauthorized();

        var ok = await _tripService.CancelTripAsync(request.TransportType, request.TripId, marshal.Id, request.Reason);
        if (!ok)
            return NotFound(new ErrorResponse
            {
                Success = false,
                Message = $"{request.TransportType} trip {request.TripId} not found",
                ErrorCode = "404"
            });

        return Ok(new { success = true, message = "Trip cancelled and affected bookings refunded where eligible" });
    }

    [HttpPost("tickets/validate")]
    public async Task<IActionResult> ValidateTicket([FromBody] ValidateTicketRequest request)
    {
        var marshal = await GetMarshalAsync();
        if (marshal == null) return MarshalUnauthorized();

        var result = await _ticketService.ValidateTicketAsync(request.QrToken, marshal.Id);
        if (!result.Success)
            return BadRequest(new ErrorResponse { Success = false, Message = result.Message, ErrorCode = "400" });

        return Ok(new
        {
            success = true,
            message = result.Message,
            data = new
            {
                result.Ticket!.TicketReference,
                result.Ticket.TransportType,
                result.Ticket.TripTime,
                result.Ticket.Status,
                result.Ticket.ValidatedAt
            }
        });
    }

    private async Task<User?> GetMarshalAsync()
    {
        var userId = HttpContext.Session.GetInt32("userId");
        if (userId == null) return null;

        var user = await _context.Users.FindAsync(userId.Value);
        return user is { Role: "Marshal" } ? user : null;
    }

    private IActionResult MarshalUnauthorized() =>
        Unauthorized(new ErrorResponse
        {
            Success = false,
            Message = "Marshal authentication required",
            ErrorCode = "401"
        });
}
