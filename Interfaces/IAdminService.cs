using _Tripfinity.Models.Tables;

namespace _Tripfinity.Interfaces;

public interface IAdminService
{
    Task<string?> GetAdminWalletIdAsync();

}