using _Tripfinity.Models.Data.Requests;
using _Tripfinity.Models.Data.Response;
using _Tripfinity.Models.Tables;

namespace _Tripfinity.Interfaces;

public interface IWalletService
{
    Task<CreateWalletResponse> CreateWalletAsync(CreateWalletRequest createWallet);
    Task<CreditWalletResponse> CreditWalletAsync(CreditWalletRequest creditWallet);
    Task<DebitWalletResponse> DebitWalletAsync(DebitWalletRequest debitWallet);
    Task<GetBalanceResponse> GetBalanceAsync(GetBalanceRequest getBalance);
    Task<GetTransactionResponse> GetTransactionAsync(GetTransactionRequest getTransaction);
    Task<RefundResponse> RefundAsync(RefundRequest refund);
    Task EnsureAuthenticatedAsync();

}