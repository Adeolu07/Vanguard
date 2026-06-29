using System.ComponentModel.DataAnnotations;

namespace _Tripfinity.Models;

public class AuthToken
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiryDate { get; set; }
}