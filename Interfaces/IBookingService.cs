using _Tripfinity.Models;

namespace _Tripfinity.Interfaces
{
    public interface IBookingService
    {
        Task<Booking?> BookBusAsync(int tripId, int seats, int? userId);

        Task<Booking?> BookRailwayAsync(int tripId, int seats, int? userId);

        Task<Booking?> BookTaxiAsync(int tripId, int seats, int? userId);

        Task<Booking?> GetBookingAsync(int id, string transportType);
        Task<List<Booking>> GetRecentBookings(int id, string transportType);
        
    }
}



// function to get a single booking/trip to be made
// function to get past 5 bookings (trips)
