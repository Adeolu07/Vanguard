using _Tripfinity.Models.Tables;
using _Tripfinity.Models.ViewModels;

namespace _Tripfinity.Interfaces;

public interface IPassengerService
{
    /// <summary>Returns the passenger user if authenticated and role is Passenger; otherwise null.</summary>
    Task<User?> GetPassengerAsync(int userId);

    /// <summary>Returns wallet balance for the given wallet id, or 0 on failure.</summary>
    Task<decimal> GetWalletBalanceAsync(string? walletId);

    /// <summary>Returns the 10 most recent upcoming bookings for the user.</summary>
    Task<List<Booking>> GetUpcomingBookingsAsync(int userId);
    // Add these two method signatures to the existing IPassengerService interface

    Task<TicketIndexViewModel> GetTicketIndexAsync(int userId);
    Task<BookingDetailViewModel?> GetBookingDetailAsync(int bookingId, int userId);
}