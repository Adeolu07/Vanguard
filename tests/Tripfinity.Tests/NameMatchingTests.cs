using Tripfinity.Utilities;

namespace Tripfinity.Tests;

public class NameMatchingTests
{
    [Theory]
    [InlineData("ADEMOLA JOHN JOHNSON", "Ademola", "Johnson")]
    [InlineData("JOHNSON, ADEMOLA", "Ademola", "Johnson")]
    [InlineData("ademola o. johnson", "Ademola", "Johnson")]
    public void Matches_when_every_expected_token_appears(string accountName, string first, string last)
    {
        Assert.True(NameMatching.Matches(accountName, first, last));
    }

    [Theory]
    [InlineData("JANE DOE", "Ademola", "Johnson")]
    [InlineData("", "Ademola", "Johnson")]
    [InlineData("ADEMOLA JOHNSON", "", "")]
    public void Rejects_names_that_do_not_contain_the_expected_tokens(string accountName, string first, string last)
    {
        Assert.False(NameMatching.Matches(accountName, first, last));
    }
}
