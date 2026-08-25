namespace _Tripfinity.Models.Data.Response;

public record WalletBalanceInfo(decimal Balance);
public record WalletTransactionInfo(string Type, decimal Amount, string Description, string TransactionId, string SessionId);
public record WalletAccountInfo(string AccountNumber, string BankName, string AccountName);
public record NameEnquiryInfo(string AccountNumber, string AccountName);