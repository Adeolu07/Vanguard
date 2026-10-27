using Tripfinity.Models;
using Tripfinity.Models.Data.Response;
using Tripfinity.Models.Tables;
using Tripfinity.Services;

namespace Tripfinity.Interfaces;

public interface ITicketService
{
    // Issues an e-Ticket for a confirmed (paid) booking.
    Task<Ticket> IssueTicketAsync(Booking booking, string? vehicleId);

    // Marks a ticket validated when a marshal scans its QR code. The code must
    // carry a signed payload produced by GenerateQrCode.
    Task<TicketValidationResult> ValidateTicketAsync(string ticketCode, int marshalId, string expectedVehicleId);

    Task<Ticket?> GetTicketAsync(int ticketId);
    Task<Ticket?> GetTicketByReferenceAsync(string ticketReference);
    Task<Ticket?> GetTicketByBookingAsync(int bookingId);

    // Renders a signed QR payload for the ticket as a PNG image.
    byte[] GenerateQrCode(string ticketReference);
}
