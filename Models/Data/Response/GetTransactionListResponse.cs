using Newtonsoft.Json;

namespace _Tripfinity.Models.Data.Response;

public class GetTransactionListResponse
{
    public required ResponseHeader ResponseHeader { get; set; }
    public required Pagination? Pagination { get; set; }
    // [JsonProperty("transactionDetailsList")]
    public List<TransactionDetailsList>? TransactionDetailsList { get; set; }
}

public class Pagination
{
    public int CurrentPage { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasPrevious { get; set; }
    public bool HasNext { get; set; }
}

public class TransactionDetailsList
{
    public string? TranType { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public string? TransactionId { get; set; }
    public string? SessionId { get; set; }
}
