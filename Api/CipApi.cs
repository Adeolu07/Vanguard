using _Tripfinity.Interfaces;
using _Tripfinity.Models.Data.Response;
using Microsoft.AspNetCore.Mvc;

namespace _Tripfinity.Api;

[Route("api/cip")]
[ApiController]
public class CipApi : ControllerBase
{
    public record NameEnquiryDto(string AccountNumber, string BankCode);
    private readonly ILogger<CipApi> _logger;
    private readonly ICipService _cipService;

    public CipApi(ILogger<CipApi> logger, ICipService cipService)
    {
        _logger = logger;
        _cipService = cipService;
    }
    
    [HttpPost("nameenquiry")]
    public async Task<IActionResult> NameEnquiry([FromBody] NameEnquiryDto dto)
    {
        try
        {
            var response = await _cipService.AccountEnquiry(dto.AccountNumber, dto.BankCode);

            if (response?.Data == null || response.Data.ResponseCode != "00")
                return Ok(ApiResponse<NameEnquiryInfo>.Fail(response?.Message ?? "Name enquiry failed."));

            return Ok(ApiResponse<NameEnquiryInfo>.Ok(new NameEnquiryInfo(
                response.Data.AccountNumber,
                response.Data.AccountName)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CIP name enquiry failed");
            return StatusCode(502, ApiResponse<NameEnquiryInfo>.Fail("Account enquiry service unavailable."));
        }
    }
}