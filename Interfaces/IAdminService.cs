using _Tripfinity.Models.Tables;
using _Tripfinity.Models.ViewModels;

namespace _Tripfinity.Interfaces;

public interface IAdminService
{
    Task<string?> GetAdminWalletIdAsync();
    Task<MarshalWalletViewModel> GetAdminWalletInfoAsync(string walletId, int page);
    Task<bool> IsAdminAsync(int userId);
    Task<List<T>> GetAllTripsAsync<T>() where T : class, ITrip;
    Task<List<Booking>> GetAllBookingsAsync();

}