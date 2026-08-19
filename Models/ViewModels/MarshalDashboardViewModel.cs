using _Tripfinity.Models.Tables;

namespace _Tripfinity.Models.ViewModels;

public class MarshalDashboardViewModel
{
    public int MarshalId { get; set; }
    public string FirstName { get; set; } = "";
    public string VehicleType { get; set; } = "";
    public string VehicleId { get; set; } = "";
    public List<Ticket>? Tickets { get; set; } = new();
}