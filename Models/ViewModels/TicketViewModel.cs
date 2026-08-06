using _Tripfinity.Models.Tables;

namespace _Tripfinity.Models.ViewModels;

public class TicketIndexViewModel
{
    public List<Booking> Bookings { get; set; } = [];
    public List<Ticket> Tickets { get; set; } = [];
}