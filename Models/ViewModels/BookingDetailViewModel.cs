using _Tripfinity.Models.Tables;

namespace _Tripfinity.Models.ViewModels;

public class BookingDetailViewModel
{
    public required Booking Booking { get; set; }
    public Ticket? Ticket { get; set; }
    public string QrCodeText => Ticket?.TicketReference ?? $"TKT-{Booking.Id}";
}