using Tripfinity.Interfaces;
using Tripfinity.Models.Data.Response;
using Tripfinity.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace Tripfinity.Api;

// Used by the marshal profile page to verify a payout account before saving it.
[Route("api/cip")]
[ApiController]
[IgnoreAntiforgeryToken]
[ApiSessionAuth(Role = ApiRoles.Marshal)]
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
    public async Task<IActionResult> NameEnquiry(NameEnquiryDto dto)
    {
        if (dto is null || dto.AccountNumber is null || dto.BankCode is null
            || dto.AccountNumber.Length != 10 || !dto.AccountNumber.All(char.IsDigit)
            || dto.BankCode.Length is < 6 or > 10 || !dto.BankCode.All(char.IsDigit))
        {
            return BadRequest(ApiResponse<NameEnquiryInfo>.Fail("Enter a valid 10-digit account number and bank."));
        }

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