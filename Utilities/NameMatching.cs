namespace _Tripfinity.Utilities;

public static class NameMatching
{
    public static bool Matches(string accountName, string firstName, string lastName)
    {
        var expected = Normalize($"{firstName} {lastName}");
        var actual = Normalize(accountName);

        if (string.IsNullOrWhiteSpace(expected) || string.IsNullOrWhiteSpace(actual))
            return false;

        // Every name token must appear in the returned account name (handles ordering).
        return expected.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .All(actual.Contains);
    }

    private static string Normalize(string input)
    {
        var upper = (input ?? string.Empty).ToUpperInvariant();
        var cleaned = new string(upper.Where(c => char.IsLetterOrDigit(c) || c == ' ').ToArray());
        return string.Join(" ", cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}