using Tripfinity.Models.Data.Response;
using Tripfinity.Models.Tables;
using Tripfinity.Models.ViewModels;

namespace Tripfinity.Interfaces;

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

    // Creates the WalletStation wallet for a confirmed user who does not have one
    // yet (e.g. the wallet service was unavailable during email confirmation).
    Task<ServiceResult> CreateWalletIfMissingAsync(int userId);


}