using System.Net;
using Tripfinity.Models.Data.Requests;
using Tripfinity.Models.Data.Response;
using TransactionDetails = Tripfinity.Models.Data.Requests.TransactionDetails;

namespace Tripfinity.Interfaces;

public interface IFastChannelService
{
    Task<(HttpStatusCode StatusCode,FcAuthRes? Response)> Authentication(FcAuthReq request);
    Task<(HttpStatusCode StatusCode,FcSinglePostRes? Response)> SinglePostAsync(TransactionDetails details);
}