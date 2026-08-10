namespace _Tripfinity.Models.ViewModels;

public class TripDetailViewModel
{
    public int TripId { get; set; }
    public string TransportType { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<TripPassenger> Passengers { get; set; } = new();
}

public class TripPassenger
{
    public string PassengerName { get; set; } = string.Empty;
    public int Seats { get; set; }
    public string BookingStatus { get; set; } = string.Empty;
    public bool HasTicket { get; set; }
    public string TicketStatus { get; set; } = "None";
}