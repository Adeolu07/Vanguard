using _Tripfinity.Models.Tables;
using _Tripfinity.Models.Data.Response;
using _Tripfinity.Services;

namespace _Tripfinity.Interfaces
{
    public interface IBookingService
    {
        Task<BookingResult> BookBusAsync(int tripId, int seats, int? userId);

        Task<BookingResult> BookRailwayAsync(int tripId, int seats, int? userId);

        Task<BookingResult> BookTaxiAsync(int tripId, int seats, int? userId);

        Task<Booking?> GetBookingAsync(int id, TransportType transportType);

        Task<List<Booking>> GetRecentBookings(int userId, string transportType);

        // Cancels a booking; refunds (credits wallet) when eligible. requestingUserId
        // enforces ownership when supplied; pass null for system/marshal-initiated cancels.
        Task<BookingResult> CancelBookingAsync(int bookingId, int? requestingUserId, string reason);
    }
}
