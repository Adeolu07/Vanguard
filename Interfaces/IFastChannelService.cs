using System.Net;
using _Tripfinity.Models.Data.Requests;
using _Tripfinity.Models.Data.Response;

namespace _Tripfinity.Interfaces;

public interface IFastChannelService
{
    Task<(HttpStatusCode StatusCode,FcAuthRes? Response)> Authentication(FcAuthReq request);
    Task<(HttpStatusCode StatusCode,FcSinglePostRes? Response)> SinglePostAsync(FcSinglePostReq request);
}