using Tripfinity.Interfaces;
using Tripfinity.Models;
using Tripfinity.Models.Data;
using Tripfinity.Models.Data.Response;
using Tripfinity.Models.Enums;
using Tripfinity.Models.Tables;
using Tripfinity.Utilities;
using Microsoft.EntityFrameworkCore;
using QRCoder;

namespace Tripfinity.Services;

public class TicketService : ITicketService
{
    private readonly AppDbContext _context;
    private readonly ILogger<TicketService> _logger;
    private readonly TicketQrPayload _qrPayload;
    private readonly bool _acceptLegacyUnsignedQr;

    public TicketService(AppDbContext context, ILogger<TicketService> logger, TicketQrPayload qrPayload,
        IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _qrPayload = qrPayload;
        _acceptLegacyUnsignedQr = configuration.GetValue<bool>("Tickets:AcceptLegacyUnsignedQr");
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

    public async Task<TicketValidationResult> ValidateTicketAsync(string ticketCode, int marshalId, string expectedVehicleId)
    {
        if (!_qrPayload.TryUnprotect(ticketCode, out var ticketReference))
        {
            // Tickets issued before QR signing carry the bare reference in the
            // image. Honouring them is an explicit, opt-in migration escape hatch.
            if (_acceptLegacyUnsignedQr && LooksLikeLegacyReference(ticketCode))
            {
                _logger.LogWarning("Accepting legacy unsigned QR payload for marshal {MarshalId}", marshalId);
                ticketReference = ticketCode.Trim();
            }
            else
            {
                _logger.LogWarning("Rejected an invalid or unsigned ticket code for marshal {MarshalId}", marshalId);
                return new TicketValidationResult
                {
                    Success = false,
                    Message = "Ticket code is not valid. Ask the passenger to show their ticket again."
                };
            }
        }

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

        //  only freshly issued tickets may be boarded
        if (ticket.Status != TicketStatus.Issued)
            return new TicketValidationResult
            {
                Success = false,
                Message = $"Ticket can no longer be used ({ticket.Status}).",
                Ticket = ticket
            };

        //  the booking behind the ticket must still be live
        var booking = await _context.Bookings.AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == ticket.BookingId);

        if (booking is null || booking.Status != BookingStatus.Confirmed)
            return new TicketValidationResult
            {
                Success = false,
                Message = "Ticket is not attached to a confirmed booking.",
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
        // The QR carries a signed payload, not the raw reference: knowing the
        // reference format must not be enough to produce a scannable code.
        var payload = _qrPayload.Protect(ticketReference);

        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
        var png = new PngByteQRCode(data);
        return png.GetGraphic(20);
    }

    private static bool LooksLikeLegacyReference(string? value) =>
        value is not null
        && value.Length is > 3 and <= 50
        && value.StartsWith("TKT-", StringComparison.OrdinalIgnoreCase)
        && value[4..].All(char.IsLetterOrDigit);

    private static DateTime ResolveTripTime(Booking booking)
    {
        if (booking.BusTrip != null) return booking.BusTrip.DepartureTime;
        if (booking.RailwayTrip != null) return booking.RailwayTrip.DepartureTime;
        if (booking.TaxiTrip != null) return booking.TaxiTrip.PickupTime;
        return DateTime.Now;
    }
}