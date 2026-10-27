using Tripfinity.Models.Data.Requests;
using Tripfinity.Models.Data.Response;

namespace Tripfinity.Interfaces;

public interface ICipService
{
    Task<NameEnquiryWrapper> AccountEnquiry(string accountNumber, string bankCode);
}