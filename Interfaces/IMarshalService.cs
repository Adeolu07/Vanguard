using _Tripfinity.Models.Tables;
using _Tripfinity.Models.ViewModels;

namespace _Tripfinity.Interfaces;

public interface IMarshalService
{
    Task<User?> GetMarshalAsync(int userId);
    Task<object?> GetMarshalTripsAsync(int marshalId, string vehicleType);
    Task<string?> GetMarshalWalletIdAsync(Booking booking);
    Task<MarshalWalletViewModel> GetWalletInfoAsync(int userId, int page = 1);
}