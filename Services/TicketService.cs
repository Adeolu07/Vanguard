using _Tripfinity.Interfaces;
using _Tripfinity.Models;
using _Tripfinity.Models.Data;
using _Tripfinity.Models.Data.Response;
using _Tripfinity.Models.Enums;
using _Tripfinity.Models.Tables;
using Microsoft.EntityFrameworkCore;
using QRCoder;

namespace _Tripfinity.Services;

public class TicketService : ITicketService
{
    private readonly AppDbContext _context;
    private readonly ILogger<TicketService> _logger;

    public TicketService(AppDbContext context, ILogger<TicketService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Ticket> IssueTicketAsync(Booking booking, string? vehicleId = null)
    {
        _logger.LogInformation("Issuing ticket for booking {BookingId}", booking.Id);

        var tripTime = ResolveTripTime(booking);
        var ticketReference = $"TKT-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

        var qrBytes = GenerateQrCode(ticketReference);
        var qrBase64 = Convert.ToBase64String(qrBytes);

        var ticket = new Ticket
        {
            TicketReference = ticketReference,
            BookingId = booking.Id,
            PassengerId = booking.UserId,
            VehicleId = vehicleId,
            TransportType = booking.TransportType,
            TripTime = tripTime,
            Fare = booking.TotalAmount,
            Status = TicketStatus.Issued,
            IssuedAt = DateTime.Now,
            QrCodeBase64 = qrBase64
        };

        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Ticket {Reference} issued for booking {BookingId}", ticket.TicketReference, booking.Id);
        return ticket;
    }

    public async Task<TicketValidationResult> ValidateTicketAsync(string ticketReference, int marshalId, string expectedVehicleId)
    {
        // Look it up in the DB
        var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.TicketReference == ticketReference);

        if (ticket == null)
            return new TicketValidationResult { Success = false, Message = "Ticket not found" };

        if (ticket.VehicleId != expectedVehicleId)
        {
            return new TicketValidationResult
            {
                Success = false,
                Message = "You are not authorized to validate this ticket. It belongs to a different vehicle.",
                Ticket = ticket
            };
        }
        //  cancelled tickets
        if (ticket.Status == TicketStatus.Cancelled)
            return new TicketValidationResult
            {
                Success = false,
                Message = "Ticket has been cancelled",
                Ticket = ticket
            };

        //  duplicate scans
        if (ticket.Status == TicketStatus.Validated)
            return new TicketValidationResult
            {
                Success = false,
                Message = $"Ticket already validated at {ticket.ValidatedAt:g} by Marshal {ticket.ValidatedByMarshalId}",
                Ticket = ticket
            };

        //  Mark ticket as validated
        ticket.Status = TicketStatus.Validated;
        ticket.ValidatedAt = DateTime.Now;
        ticket.ValidatedByMarshalId = marshalId;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Ticket {Reference} validated by marshal {MarshalId}", ticket.TicketReference, marshalId);

        return new TicketValidationResult
        {
            Success = true,
            Message = "Ticket validated successfully",
            Ticket = ticket
        };
    }

    public async Task<Ticket?> GetTicketAsync(int ticketId)
    {
        return await _context.Tickets.Include(t => t.Booking).FirstOrDefaultAsync(t => t.Id == ticketId);
    }

    public async Task<Ticket?> GetTicketByReferenceAsync(string ticketReference)
    {
        return await _context.Tickets.Include(t => t.Booking)
            .FirstOrDefaultAsync(t => t.TicketReference == ticketReference);
    }

    public async Task<Ticket?> GetTicketByBookingAsync(int bookingId)
    {
        return await _context.Tickets.FirstOrDefaultAsync(t => t.BookingId == bookingId);
    }

    public byte[] GenerateQrCode(string ticketReference)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(ticketReference, QRCodeGenerator.ECCLevel.Q);
        var png = new PngByteQRCode(data);
        return png.GetGraphic(20);
    }

    private static DateTime ResolveTripTime(Booking booking)
    {
        if (booking.BusTrip != null) return booking.BusTrip.DepartureTime;
        if (booking.RailwayTrip != null) return booking.RailwayTrip.DepartureTime;
        if (booking.TaxiTrip != null) return booking.TaxiTrip.PickupTime;
        return DateTime.Now;
    }
}