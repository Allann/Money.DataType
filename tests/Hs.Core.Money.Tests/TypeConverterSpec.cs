using System.ComponentModel;
using System.Globalization;

namespace Hs.Core.Money.Tests.TypeConverterSpec;

/// <summary>Converters are found through <see cref="TypeDescriptor"/>, as configuration and model binding find them.</summary>
public class GivenACurrencyAsText
{
    private readonly TypeConverter _converter = TypeDescriptor.GetConverter(typeof(Currency));

    [Fact]
    public void WhenAskingForTheConverter_ThenItIsTheCurrencyConverter() =>
        _converter.Should().BeOfType<CurrencyTypeConverter>();

    [Fact]
    public void WhenConvertingAnIsoCode_ThenItIsThatCurrency() =>
        _converter.ConvertFromInvariantString("EUR").Should().Be(Currency.FromCode("EUR"));

    [Fact]
    public void WhenConvertingAnIsoCurrency_ThenItIsJustItsCode() =>
        _converter.ConvertToInvariantString(Currency.FromCode("EUR")).Should().Be("EUR");

    [Theory]
    [InlineData("EUR;")]
    [InlineData("EUR; ")]
    public void WhenTheNamespaceIsEmpty_ThenItIsAnIsoCode(string text) =>
        _converter.ConvertFromInvariantString(text).Should().Be(Currency.FromCode("EUR"));

    [Fact]
    public void WhenConvertingACustomCurrency_ThenTheNamespaceRoundTrips()
    {
        var bitcoin = new CurrencyBuilder("BTC9", "converter-spec") { EnglishName = "Bitcoin", DecimalDigits = 8 }.Register();
        try
        {
            var text = _converter.ConvertToInvariantString(bitcoin);

            text.Should().Be("BTC9;converter-spec");
            _converter.ConvertFromInvariantString(text!).Should().Be(bitcoin);
        }
        finally
        {
            CurrencyBuilder.Unregister("BTC9", "converter-spec");
        }
    }

    [Fact]
    public void WhenTheCodeIsUnknown_ThenItThrows()
    {
        Action action = () => _converter.ConvertFromInvariantString("ZZZ");

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WhenConvertingFromSomethingOtherThanText_ThenItIsNotSupported()
    {
        _converter.CanConvertFrom(typeof(int)).Should().BeFalse();
        Action action = () => _converter.ConvertFrom(42);

        action.Should().Throw<NotSupportedException>();
    }
}

public class GivenAnAmountAsText
{
    private readonly TypeConverter _converter = TypeDescriptor.GetConverter(typeof(Amount));

    [Fact]
    public void WhenAskingForTheConverter_ThenItIsTheAmountConverter() =>
        _converter.Should().BeOfType<AmountTypeConverter>();

    [Fact]
    public void WhenConvertingInvariantText_ThenItIsThatAmount() =>
        _converter.ConvertFromInvariantString("1234.56 EUR").Should().Be(new Amount(1234.56m, "EUR"));

    [Fact]
    public void WhenConvertingWithACulture_ThenItsNumberFormatIsUsed() =>
        _converter.ConvertFrom(null, CultureInfo.GetCultureInfo("nl-NL"), "1234,56 EUR").Should().Be(new Amount(1234.56m, "EUR"));

    [Fact]
    public void WhenConvertingToText_ThenItRoundTrips()
    {
        var amount = new Amount(-765.43m, "USD");

        var text = _converter.ConvertToInvariantString(amount);

        text.Should().Be("-765.43 USD");
        _converter.ConvertFromInvariantString(text!).Should().Be(amount);
    }

    [Theory]
    [InlineData("1234.56")]
    [InlineData("1234.56 EUR extra")]
    public void WhenTheTextIsNotANumberAndCode_ThenItThrows(string text)
    {
        Action action = () => _converter.ConvertFromInvariantString(text);

        action.Should().Throw<FormatException>();
    }
}
