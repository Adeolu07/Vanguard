using System.ComponentModel.DataAnnotations;

namespace Tripfinity.Models.Data.Requests;

public class ValidateTicketRequest
{
    [Required] public string QrToken { get; set; } = string.Empty;
}
