using System.Globalization;
using AwesomeAssertions;
using Xunit;

namespace Hs.Core.Tests;

public class NumberToWordsSpec
{
    private static readonly CultureInfo English = CultureInfo.GetCultureInfo("en-AU");
    private static readonly CultureInfo Dutch = CultureInfo.GetCultureInfo("nl-NL");

    [Fact]
    public void ToWords_UsesTheCurrentCultureByDefault()
    {
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = Dutch;
            21m.ToWords().Should().Be(new Words("eenentwintig"));
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Theory]
    [InlineData("en-US")]
    [InlineData("en-GB")]
    [InlineData("fr-FR")]
    [InlineData("")]
    public void ToWords_FallsBackToEnglishForOtherCultures(string culture) =>
        21m.ToWords(CultureInfo.GetCultureInfo(culture)).Should().Be(new Words("twenty-one"));

    [Fact]
    public void ToWords_PicksTheLanguageRegardlessOfRegion() =>
        21m.ToWords(CultureInfo.GetCultureInfo("nl-BE")).Should().Be(new Words("eenentwintig"));

    [Fact]
    public void Words_ToStringIsTheText() =>
        new Words("seven").ToString().Should().Be("seven");

    public class InEnglish
    {
        public static TheoryData<decimal, string> Numbers => new()
        {
            { 0m, "zero" },
            { 7m, "seven" },
            { 13m, "thirteen" },
            { 20m, "twenty" },
            { 42m, "forty-two" },
            { 100m, "one hundred" },
            { 105m, "one hundred and five" },
            { 999m, "nine hundred and ninety-nine" },
            { 1_000m, "one thousand" },
            { 1_005m, "one thousand and five" },
            { 1_100m, "one thousand one hundred" },
            { 12_345m, "twelve thousand three hundred and forty-five" },
            { 1_000_000m, "one million" },
            { 1_000_001m, "one million and one" },
            { 2_000_300_000m, "two billion three hundred thousand" },
            { 0.5m, "zero point five" },
            { 12.05m, "twelve point zero five" },
            { 1.50m, "one point five" },
            { 3.14159m, "three point one four one five nine" },
            { -1m, "minus one" },
            { -12.5m, "minus twelve point five" },
        };

        [Theory]
        [MemberData(nameof(Numbers))]
        public void ToWords_WritesTheNumberOut(decimal number, string expected) =>
            number.ToWords(English).Should().Be(new Words(expected));

        [Fact]
        public void ToWords_CoversTheFullDecimalRange() =>
            decimal.MaxValue.ToWords(English).Value.Should().Be(
                "seventy-nine octillion two hundred and twenty-eight septillion one hundred and sixty-two sextillion " +
                "five hundred and fourteen quintillion two hundred and sixty-four quadrillion three hundred and thirty-seven trillion " +
                "five hundred and ninety-three billion five hundred and forty-three million nine hundred and fifty thousand " +
                "three hundred and thirty-five");

        [Fact]
        public void ToWords_WritesTheSmallestFractionDigitByDigit() =>
            0.0000000000000000000000000001m.ToWords(English).Value.Should().Be(
                "zero point " + string.Join(" ", Enumerable.Repeat("zero", 27)) + " one");
    }

    public class InDutch
    {
        public static TheoryData<decimal, string> Numbers => new()
        {
            { 0m, "nul" },
            { 1m, "een" },
            { 13m, "dertien" },
            { 20m, "twintig" },
            { 21m, "eenentwintig" },
            { 22m, "tweeëntwintig" },
            { 45m, "vijfenveertig" },
            { 100m, "honderd" },
            { 105m, "honderdvijf" },
            { 999m, "negenhonderdnegenennegentig" },
            { 1_000m, "duizend" },
            { 1_005m, "duizend vijf" },
            { 2_345m, "tweeduizend driehonderdvijfenveertig" },
            { 100_000m, "honderdduizend" },
            { 1_000_000m, "een miljoen" },
            { 2_000_300_000m, "twee miljard driehonderdduizend" },
            { 0.5m, "nul komma vijf" },
            { -12.5m, "min twaalf komma vijf" },
        };

        [Theory]
        [MemberData(nameof(Numbers))]
        public void ToWords_WritesTheNumberOut(decimal number, string expected) =>
            number.ToWords(Dutch).Should().Be(new Words(expected));

        [Fact]
        public void ToWords_CoversTheFullDecimalRange() =>
            decimal.MaxValue.ToWords(Dutch).Value.Should().Be(
                "negenenzeventig quadriljard tweehonderdachtentwintig quadriljoen honderdtweeënzestig triljard " +
                "vijfhonderdveertien triljoen tweehonderdvierenzestig biljard driehonderdzevenendertig biljoen " +
                "vijfhonderddrieënnegentig miljard vijfhonderddrieënveertig miljoen negenhonderdvijftigduizend " +
                "driehonderdvijfendertig");
    }
}
