using _Tripfinity.Models;

namespace _Tripfinity.Interfaces
{
    public interface IBookingService
    {
        Task<Booking?> BookBusAsync(int tripId, int seats, string userEmail);

        Task<Booking?> BookRailwayAsync(int tripId, int seats, string userEmail);

        Task<Booking?> BookTaxiAsync(int tripId, int seats, string userEmail);

        Task<Booking?> GetBookingAsync(int id, string transportType);
    }
}
