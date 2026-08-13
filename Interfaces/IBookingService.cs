using _Tripfinity.Models.Tables;
using _Tripfinity.Models.Data.Response;
using _Tripfinity.Models.Enums;

namespace _Tripfinity.Interfaces
{
    public interface IBookingService
    {
        
        Task<Booking?> GetBookingAsync(int id, TransportType transportType);
        
        // IBookingService.cs additions:
        Task<object?> GetTripAsync(string type, int tripId);
        Task<BookingResult> BookAsync(string type, int tripId, int seats, int userId);
        
        // Cancels a booking; refunds (credits wallet) when eligible. requestingUserId
        // enforces ownership when supplied; pass null for system/marshal-initiated cancels.
        Task<BookingResult> CancelBookingAsync(int bookingId, int? requestingUserId, string reason);
    }
}
