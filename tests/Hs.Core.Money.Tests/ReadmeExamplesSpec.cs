using System.Globalization;
using Hs.Core.Money.Extensions;

namespace Hs.Core.Money.Tests.ReadmeExamplesSpec;

/// <summary>Every example in README.md, so the documentation can't drift from the code.</summary>
public class GivenTheReadmeExamples
{
    private static readonly CultureInfo English = CultureInfo.GetCultureInfo("en-US");
    private static readonly CultureInfo Dutch = CultureInfo.GetCultureInfo("nl-NL");

    [Fact]
    public void ArithmeticStaysInOneCurrency()
    {
        var price = new Amount(1234.5m, "EUR");
        var dollars = new Amount(10m, "USD");

        (price * 1.21m).Should().Be(new Amount(1493.74m, "EUR")); // 1493.745 rounds to even
        var mixing = () => price + dollars;
        mixing.Should().Throw<InvalidCurrencyException>();
    }

    [Fact]
    public void DividingNeverLosesACent()
    {
        new Amount(0.05m, "EUR").SafeDivide(2).Select(a => a.Value).Should().Equal(0.02m, 0.03m);
        new Amount(1m, "EUR").SafeDivide([2, 3, 3]).Select(a => a.Value).Should().Equal(0.25m, 0.38m, 0.37m);
    }

    [Fact]
    public void Formatting()
    {
        var price = new Amount(1234.5m, "EUR");

        price.ToString("C", English).Should().Be("€1,234.50");
        price.ToString("I", English).Should().Be("EUR 1,234.50");
        price.ToString("F", English).Should().Be("1,234.50 Euro");
        price.ToString("C", Dutch).Should().Be("€ 1.234,50");
        price.ToString("W", English).Should().Be("one thousand two hundred and thirty-four euros and fifty cents");
        price.ToString("W", Dutch).Should().Be("duizend tweehonderdvierendertig euro en vijftig cent");
    }

    [Fact]
    public void ParsingAndExchange()
    {
        Amount.Parse("1.234,50", NumberStyles.Currency, Dutch, Currency.FromCode("EUR")).Should().Be(new Amount(1234.5m, "EUR"));

        var rate = new ExchangeRate("EUR", "USD", 1.2591m);
        rate.Convert(new Amount(100m, "EUR")).Should().Be(new Amount(125.91m, "USD"));
        ExchangeRate.Parse("EUR/USD 1.2591").Should().Be(rate);
    }

    [Fact]
    public void NumbersInWords()
    {
        21m.ToWords(English).Value.Should().Be("twenty-one");
        21m.ToWords(Dutch).Value.Should().Be("eenentwintig");
        (-1005.5m).ToWords(English).Value.Should().Be("minus one thousand and five point five");
    }
}
