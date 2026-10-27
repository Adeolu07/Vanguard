using Tripfinity.Models.Tables;

namespace Tripfinity.Models.ViewModels;

public class TicketIndexViewModel
{
    public List<Booking> Bookings { get; set; } = [];
    public List<Ticket> Tickets { get; set; } = [];
}