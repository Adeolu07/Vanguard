using _Tripfinity.Models.Tables;
using _Tripfinity.Models.ViewModels;

namespace _Tripfinity.Interfaces;

public interface IPassengerService
{
    Task<User?> GetPassengerAsync(int userId);
    
    Task<decimal> GetWalletBalanceAsync(string? walletId);
    
    Task<List<Booking>> GetUpcomingBookingsAsync(int userId);
    Task<TicketIndexViewModel> GetTicketIndexAsync(int userId);
    Task<BookingDetailViewModel?> GetBookingDetailAsync(int bookingId, int userId);
    Task<User?> GetUserByIdAsync(int userId);
    Task<bool> UpdateUserProfileAsync(int userId, UpdateProfileModel model);
    Task<TransactionsViewModel?> GetWalletTransactions(int userId, int page = 1);


}