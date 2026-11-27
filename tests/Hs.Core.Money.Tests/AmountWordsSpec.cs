using System.Globalization;
using Hs.Core.Money.Tests.Helpers;
using AwesomeAssertions;
using Xunit;

namespace Hs.Core.Money.Tests.AmountWordsSpec
{
    public class GivenIWantMoneyInWords
    {
        private static readonly CultureInfo English = CultureInfo.GetCultureInfo("en-AU");
        private static readonly CultureInfo Dutch = CultureInfo.GetCultureInfo("nl-NL");

        public static TheoryData<decimal, string, string> InEnglish => new()
        {
            { 0m, "USD", "zero dollars" },
            { 1m, "USD", "one dollar" },
            { 0.01m, "USD", "one cent" },
            { 0.5m, "AUD", "fifty cents" },
            { 1.01m, "USD", "one dollar and one cent" },
            { 1234.56m, "AUD", "one thousand two hundred and thirty-four dollars and fifty-six cents" },
            { 12.345m, "USD", "twelve dollars and thirty-four cents" }, // Amount rounds to even on construction
            { 0.999m, "USD", "one dollar" },
            { -5.25m, "USD", "minus five dollars and twenty-five cents" },
            { -0.001m, "USD", "zero dollars" },
            { 2.01m, "GBP", "two pounds and one penny" },
            { 7.5m, "EUR", "seven euros and fifty cents" },
            { 765.4321m, "JPY", "seven hundred and sixty-five yen" },
        };

        public static TheoryData<decimal, string, string> InDutch => new()
        {
            { 1m, "USD", "een dollar" },
            { 1234.56m, "USD", "duizend tweehonderdvierendertig dollar en zesenvijftig cent" },
            { 7.5m, "EUR", "zeven euro en vijftig cent" },
            { -0.2m, "EUR", "min twintig cent" },
        };

        [Theory]
        [MemberData(nameof(InEnglish))]
        public void WhenWrittenInEnglish_ThenUnitsArePluralisedAndJoinedByAnd(decimal value, string code, string expected) =>
            new Amount(value, Currency.FromCode(code)).ToWords(English).Should().Be(new Words(expected));

        [Theory]
        [MemberData(nameof(InDutch))]
        public void WhenWrittenInDutch_ThenUnitsStaySingular(decimal value, string code, string expected) =>
            new Amount(value, Currency.FromCode(code)).ToWords(Dutch).Should().Be(new Words(expected));

        [Fact]
        public void WhenTheCurrencyHasNoUnitNames_ThenTheNumberIsFollowedByTheCurrencyName() =>
            new Amount(12.5m, Currency.FromCode("CHF")).ToWords(English).Value.Should().Be("twelve point five Swiss franc");

        [Fact]
        [UseCulture("nl-NL")]
        public void WhenNoCultureIsGiven_ThenTheCurrentCultureIsUsed() =>
            new Amount(1m, Currency.FromCode("USD")).ToWords().Value.Should().Be("een dollar");

        [Fact]
        public void WhenFormattedWithW_ThenTheAmountIsWrittenInWords() =>
            new Amount(1.05m, Currency.FromCode("USD")).ToString("W", Dutch).Should().Be("een dollar en vijf cent");

        [Fact]
        [UseCulture("en-AU")]
        public void WhenFormattedWithWAndANumberFormat_ThenTheCurrentCultureLanguageIsUsed() =>
            new Amount(1.05m, Currency.FromCode("USD")).ToString("W", NumberFormatInfo.InvariantInfo)
                .Should().Be("one dollar and five cents");
    }
}
