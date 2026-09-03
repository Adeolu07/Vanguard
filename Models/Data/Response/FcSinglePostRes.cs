namespace _Tripfinity.Models.Data.Response;

public class FcSinglePostRes
{
    public required ResponseHeader ResponseHeader { get; set; }
    public string? TraceId { get; set; }
    public string? BatchId { get; set; }
    public decimal AmountDebited { get; set; }
    public decimal TransactionAmount { get; set; }
    public decimal TransactionCharge { get; set; }
    public decimal Vat { get; set; }
}