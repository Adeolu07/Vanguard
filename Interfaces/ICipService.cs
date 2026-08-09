using _Tripfinity.Models.Data.Requests;
using _Tripfinity.Models.Data.Response;

namespace _Tripfinity.Interfaces;

public interface ICipService
{
    Task<NameEnquiryResponse> AccountEnquiry(NameEnquiryRequest request);
    Task<PostCreditResponse> PostCredit(PostCreditRequest pcRequest);
    Task<TransactionQueryResponse> TransactionQuery(TransactionQueryRequest tsqRequest);
}