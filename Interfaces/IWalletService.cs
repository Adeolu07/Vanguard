using Tripfinity.Models.Data.Requests;
using Tripfinity.Models.Data.Response;
using Tripfinity.Models.ViewModels;

namespace Tripfinity.Interfaces;

public interface IWalletService
{
    Task<CreateWalletResponse> CreateWalletAsync(CreateWalletRequest createWallet);
    Task<WalletTransaction> CreditWalletAsync(CreditWalletRequest creditWallet);
    Task<WalletTransaction> DebitWalletAsync(DebitWalletRequest debitWallet);
    Task<GetBalanceResponse> GetBalanceAsync(GetBalanceRequest getBalance);
    Task<GetTransactionResponse> GetTransactionAsync(GetTransactionRequest getTransaction);
    Task<WalletNameEnquiryResponse> WalletNameEnquiryAsync(WalletNameEnquiryRequest request);
    Task<RefundResponse> RefundAsync(RefundRequest refund);
    Task<GetTransactionListResponse> GetTransactionList(GetTransactionListRequest request);
    Task EnsureAuthenticatedAsync();
    Task<MarshalWalletViewModel> BuildWalletInfoAsync(string walletId, int page);

}