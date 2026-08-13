using _Tripfinity.Models.Data.Requests;
using _Tripfinity.Models.Data.Response;

namespace _Tripfinity.Interfaces;

public interface ICipService
{
    Task<NameEnquiryWrapper> AccountEnquiry(string accountNumber, string bankCode);
}