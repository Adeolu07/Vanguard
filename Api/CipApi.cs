using _Tripfinity.Interfaces;
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
        _logger.LogInformation("Name Enquiry API call");
        var response = await _cipService.AccountEnquiry(dto.AccountNumber, dto.BankCode);
        if (response?.Data?.ResponseCode == "00")
            return Ok(response);
        _logger.LogWarning("Failed Name Enquiry");
        return BadRequest(response);
    }
}