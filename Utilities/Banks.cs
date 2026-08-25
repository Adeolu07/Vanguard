namespace _Tripfinity.Utilities;

public record BankOption(string Name, string Code);

public static class Banks
{
    // CIP institution codes
    public static readonly IReadOnlyList<BankOption> All = new List<BankOption>
    {
        new("Novus Bank", "100067"),
        new("Access Bank", "000014"),
        new("Ecobank Nigeria", "000010"),
        new("Fidelity Bank", "000007"),
        new("First Bank of Nigeria", "000016"),
        new("First City Monument Bank", "000003"),
        new("Guaranty Trust Bank", "000013"),
        new("Moniepoint Microfinance Bank", "090405"),
        new("Opay Digital Services", "100004"),
        new("Palmpay", "100033"),
        new("Providus Bank", "000023"),
        new("Stanbic IBTC Bank", "000012"),
        new("Union Bank of Nigeria", "000018"),
        new("United Bank for Africa", "000004"),
        new("Wema Bank", "000017"),
        new("Zenith Bank", "000015"),
    };

    public static string? GetBankName(string? code) =>
        All.FirstOrDefault(b => string.Equals(b.Code, code, StringComparison.OrdinalIgnoreCase))?.Name;

    public static string? GetBankCode(string? name) =>
        All.FirstOrDefault(b => string.Equals(b.Name, name, StringComparison.OrdinalIgnoreCase))?.Code;
}