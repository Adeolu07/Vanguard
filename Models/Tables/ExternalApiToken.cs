using System;

namespace _Tripfinity.Models.Tables;

public class ExternalApiToken
{
    public int Id { get; set; }

    public required string Provider { get; set; }

    public required string Token { get; set; }

    public DateTime ExpiryDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}