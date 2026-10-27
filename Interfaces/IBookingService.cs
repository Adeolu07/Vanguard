using Tripfinity.Models.Tables;
using Tripfinity.Models.Data.Response;
using Tripfinity.Models.Enums;

namespace Tripfinity.Interfaces
{
    public interface IBookingService
    {
        
        Task<Booking?> GetBookingAsync(int id, TransportType transportType, int? ownerUserId = null);
        
        // IBookingService.cs additions:
        Task<object?> GetTripAsync(string type, int tripId);
        Task<BookingResult> BookAsync(string type, int tripId, int seats, int userId);
        
        // Cancels a booking; refunds (credits wallet) when eligible. requestingUserId
        // enforces ownership when supplied; pass null for system/marshal-initiated cancels.
        Task<BookingResult> CancelBookingAsync(int bookingId, int? requestingUserId, string reason);
    }
}
