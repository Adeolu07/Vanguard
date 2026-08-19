namespace _Tripfinity.Utilities;

public record BankOption(string Name, string Code);

public static class Banks
{
    // NIBSS/NIP bank codes — adjust if your CIP provider expects a different format.
    public static readonly IReadOnlyList<BankOption> All = new List<BankOption>
    {
        new("Novus Bank", "100067"),
        new("Access Bank", "044"),
        new("Ecobank Nigeria", "050"),
        new("Fidelity Bank", "070"),
        new("First Bank of Nigeria", "011"),
        new("First City Monument Bank", "214"),
        new("Guaranty Trust Bank", "058"),
        new("Moniepoint Microfinance Bank", "50515"),
        new("Opay Digital Services", "999992"),
        new("Palmpay", "999991"),
        new("Providus Bank", "101"),
        new("Stanbic IBTC Bank", "221"),
        new("Standard Chartered Bank", "068"),
        new("Union Bank of Nigeria", "032"),
        new("United Bank for Africa", "033"),
        new("Wema Bank", "035"),
        new("Zenith Bank", "057"),
    };

    public static string? GetBankName(string? code) =>
        All.FirstOrDefault(b => string.Equals(b.Code, code, StringComparison.OrdinalIgnoreCase))?.Name;

    public static string? GetBankCode(string? name) =>
        All.FirstOrDefault(b => string.Equals(b.Name, name, StringComparison.OrdinalIgnoreCase))?.Code;
}