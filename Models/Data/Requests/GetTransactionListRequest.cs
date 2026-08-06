namespace _Tripfinity.Models.Data.Requests;

public class GetTransactionListRequest
{
    public required string CustomerId { get; set; }
    public required SearchDetails SearchDetails { get; set; }
}


public class SearchDetails
{
    public int Page { get; set; }
    public int ItemsPerPage { get; set; }
    public required DateRange DateRange { get; set; }
}

public class DateRange
{
    public required DateTime Start { get; set; }
    public required DateTime End { get; set; }
}