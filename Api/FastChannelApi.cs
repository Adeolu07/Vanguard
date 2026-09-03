using System.Net;
using _Tripfinity.Interfaces;
using _Tripfinity.Models.Data.Requests;
using Microsoft.AspNetCore.Mvc;

namespace _Tripfinity.Api;

[ApiController]
[Route("api/fastchannel")]
public class FastChannelApi : ControllerBase
{
    private readonly IFastChannelService _fastChannelService;
    public FastChannelApi(IFastChannelService fastChannelService)
    {
        _fastChannelService = fastChannelService;
    }

    [HttpPost("authentication")]
    public async Task<IActionResult> Authentication(FcAuthReq req)
    {
        var (statusCode, res) = await _fastChannelService.Authentication(req);
        return StatusCode((int)MapToHttpStatus(statusCode, res?.ResponseHeader.ResponseCode), res);
    }

    [HttpPost("singlepost")]
    public async Task<IActionResult> SinglePost(FcSinglePostReq req)
    {
        var (statusCode, res) = await _fastChannelService.SinglePostAsync(req);
        return StatusCode((int)MapToHttpStatus(statusCode, res?.ResponseHeader.ResponseCode), res);
    }

    private static HttpStatusCode MapToHttpStatus(HttpStatusCode upstreamStatus, string? responseCode)
    {
        // Transport-level failures (401, 400, 502, etc.) pass through unchanged.
        if (upstreamStatus != HttpStatusCode.OK)
            return upstreamStatus;

        // FastChannel responds with HTTP 200 even for business failures,
        // so map the business response code to the correct HTTP status.
        return responseCode switch
        {
            "00" => HttpStatusCode.OK,
            "01" => HttpStatusCode.BadRequest,           // Failed Transaction
            "03" => HttpStatusCode.Forbidden,            // Invalid Sender
            "05" => HttpStatusCode.BadRequest,           // Do not honor
            "06" => HttpStatusCode.BadRequest,           // Invalid or Missing Parameter
            "07" => HttpStatusCode.UnprocessableEntity,  // Invalid Account
            "08" => HttpStatusCode.UnprocessableEntity,  // Account Name Mismatch
            "09" => HttpStatusCode.Accepted,             // Pending Transaction
            "12" => HttpStatusCode.BadRequest,           // Invalid transaction
            "13" => HttpStatusCode.UnprocessableEntity,  // Invalid Amount
            _ => HttpStatusCode.BadRequest
        };
    }
}