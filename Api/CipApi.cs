using _Tripfinity.Interfaces;
using _Tripfinity.Models.Data.Requests;
using Microsoft.AspNetCore.Mvc;

namespace _Tripfinity.Api;

[Route("api/cip")]
[ApiController]
public class CipApi : ControllerBase
{
    
    private readonly ILogger<CipApi> _logger;
    private readonly ICipService _cipService;

    public CipApi(ILogger<CipApi> logger, ICipService cipService)
    {
        _logger = logger;
        _cipService = cipService;
    }
    
    public record NameEnquiryDto(string AccountNumber, string BankCode);

    public record PostCreditDto(decimal Amount, string AccountNumber, string AccountName, string BankCode);

    [HttpPost("nameenquiry")]
    public async Task<IActionResult> NameEnquiry([FromBody] NameEnquiryDto dto)
    {
        var response = await _cipService.AccountEnquiry(dto.AccountNumber, dto.BankCode);
        if (response.ResponseCode == "00")
            return Ok(response);
        return BadRequest(response);
    }
    
    [HttpPost("postcredit")]
    public async Task<IActionResult> PostCredit([FromBody] PostCreditDto dto)
    {
        
        var response = await _cipService.PostCredit(dto.Amount,dto.AccountNumber,dto.AccountName, dto.BankCode);
        if (response.ResponseCode == "00")
            return Ok(response);
        return BadRequest(response);
    }
    
    
    
    
}