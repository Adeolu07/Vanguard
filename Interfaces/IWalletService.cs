using _Tripfinity.Models.Data.Requests;
using _Tripfinity.Models.Data.Response;
using Microsoft.AspNetCore.Mvc;

namespace _Tripfinity.Interfaces;

public interface IWalletService
{
    Task<AuthenticationResponse> AuthenticationAsync(AuthenticationRequest request);
    Task<CreateWalletResponse> CreateWalletAsync(CreateWalletRequest createWallet);
    Task<CreditWalletResponse> CreditWalletAsync(CreditWalletRequest creditWallet);
    Task<DebitWalletResponse> DebitWalletAsync(DebitWalletRequest debitWallet);
    Task<GetBalanceResponse> GetBalanceAsync(GetBalanceRequest getBalance);
    Task<GetTransactionResponse> GetTransactionAsync(GetTransactionRequest getTransaction);
    Task<RefundResponse> RefundAsync(RefundRequest refund);
    
}