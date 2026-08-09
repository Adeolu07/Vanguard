using _Tripfinity.Models.Data.Requests;
using _Tripfinity.Models.Data.Response;

namespace _Tripfinity.Interfaces;

public interface ICipService
{
    Task<NameEnquiryResponse> AccountEnquiry(string accountNumber, string bankCode);
    Task<PostCreditResponse> PostCredit(decimal amount, string accountNumber, string accountName, string bankCode);
    Task<TransactionQueryResponse> TransactionQuery(TransactionQueryRequest tsqRequest);
}