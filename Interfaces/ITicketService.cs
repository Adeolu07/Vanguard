using _Tripfinity.Models;
using _Tripfinity.Models.Data.Response;
using _Tripfinity.Models.Tables;

namespace _Tripfinity.Interfaces;

public interface ITicketService
{
    // Issues an e-Ticket for a confirmed (paid) booking.
    Task<Ticket> IssueTicketAsync(Booking booking, string? vehicleId = null);

    // Marks a ticket validated when a marshal scans its QR token.
    Task<TicketValidationResult> ValidateTicketAsync(string qrToken, int marshalId);

    Task<Ticket?> GetTicketAsync(int ticketId);
    Task<Ticket?> GetTicketByReferenceAsync(string ticketReference);
    Task<Ticket?> GetTicketByBookingAsync(int bookingId);

    // Renders the ticket's QR token as a PNG image.
    byte[] GenerateQrCode(string qrToken);
}
