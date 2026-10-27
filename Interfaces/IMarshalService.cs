using Tripfinity.Models.Data.Response;
using Tripfinity.Models.Tables;
using Tripfinity.Models.ViewModels;

namespace Tripfinity.Interfaces;

public interface IMarshalService
{
    Task<User?> GetMarshalAsync(int userId);
    Task<object?> GetMarshalTripsAsync(int marshalId, string vehicleType);
    Task<string?> GetMarshalWalletIdAsync(Booking booking);
    Task<MarshalWalletViewModel> GetWalletInfoAsync(int userId, int page = 1);
    
    Task<MarshalDashboardViewModel?> GetMarshalDashboardAsync(int marshalId);

    // Payout account
    Task<MarshalBankAccount?> GetBankAccountAsync(int userId);
    Task<ServiceResult> AddBankAccountAsync(int userId, string accountNumber, string bankCode);
    Task<ServiceResult> CashOutAsync(int userId, decimal amount);

    // Trip detail
    Task<TripDetailViewModel?> GetTripDetailAsync(int tripId, int marshalId, string vehicleType);
}