using Tripfinity.Models.Tables;
using Tripfinity.Models.ViewModels;

namespace Tripfinity.Interfaces;

public interface IAdminService
{
    Task<string?> GetAdminWalletIdAsync();
    Task<MarshalWalletViewModel> GetAdminWalletInfoAsync(string walletId, int page);
    Task<bool> IsAdminAsync(int userId);
    Task<List<T>> GetAllTripsAsync<T>() where T : class, ITrip;
    Task<List<Booking>> GetAllBookingsAsync();

}