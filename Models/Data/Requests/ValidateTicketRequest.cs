using System.ComponentModel.DataAnnotations;

namespace _Tripfinity.Models.Data.Requests;

public class ValidateTicketRequest
{
    [Required] public string QrToken { get; set; } = string.Empty;
}
