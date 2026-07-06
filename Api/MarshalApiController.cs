using _Tripfinity.Interfaces;
using _Tripfinity.Models;
using _Tripfinity.Models.Data;
using _Tripfinity.Models.Data.Requests;
using _Tripfinity.Models.Tables;
using _Tripfinity.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace _Tripfinity.Api;

[Route("api/marshal")]
[ApiController]
public class MarshalApiController : ControllerBase
{
    private readonly ITripService _tripService;
    private readonly ITicketService _ticketService;
    private readonly AppDbContext _context;
    private readonly ILogger<MarshalApiController> _logger;

    public MarshalApiController(
        ITripService tripService,
        ITicketService ticketService,
        AppDbContext context,
        ILogger<MarshalApiController> logger)
    {
        _tripService = tripService;
        _ticketService = ticketService;
        _context = context;
        _logger = logger;
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
