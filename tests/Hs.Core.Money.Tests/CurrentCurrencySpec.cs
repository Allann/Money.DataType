using System.Globalization;
using Hs.Core.Money.Tests.Helpers;

namespace Hs.Core.Money.Tests.CurrentCurrencySpec;

public class GivenTheCurrentCulture
{
    [Fact]
    [UseCulture("en-AU")]
    public void WhenItIsSpecific_ThenTheCurrencyIsItsRegions() =>
        Currency.CurrentCurrency.Should().Be(Currency.FromCode("AUD"));

    [Fact]
    [UseCulture("")]
    public void WhenItIsInvariant_ThenItIsNoCurrency()
    {
        // CI runners and containers often run with the invariant culture (LANG=C).
        CultureInfo.CurrentCulture.Should().Be(CultureInfo.InvariantCulture);

        Currency.CurrentCurrency.Code.Should().Be("XXX");
    }
}
