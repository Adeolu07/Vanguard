using System.ComponentModel.DataAnnotations;

namespace _Tripfinity.Models.Tables;

public class Wallet
{
    public int Id { get; set; }

    [Required]
    public string CustomerId { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public int UserId { get; set; }

    public decimal Balance { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public bool IsActive { get; set; } = true;

    public User? User { get; set; }
}