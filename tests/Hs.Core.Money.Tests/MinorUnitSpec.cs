using System.Globalization;
using Hs.Core.Money.Extensions;

namespace Hs.Core.Money.Tests.MinorUnitSpec;

/// <summary>Currencies whose minor unit isn't a power of ten, or that have none, round, format and divide correctly.</summary>
public class GivenACurrencyWithoutDecimalMinorUnits
{
    private static readonly Currency Gold = Currency.FromCode("XAU");
    private static readonly Currency Ariary = Currency.FromCode("MGA");

    [Theory]
    [InlineData(1.3, 1.2)] // 6.5 fifths rounds to even: 6
    [InlineData(1.5, 1.6)] // 7.5 fifths rounds to even: 8
    [InlineData(1.09, 1.0)]
    [InlineData(1.11, 1.2)]
    public void WhenAnAmountIsInFifths_ThenItRoundsToTheNearestFifth(double value, double expected) =>
        new Amount((decimal)value, Ariary).Value.Should().Be((decimal)expected);

    [Fact]
    public void WhenAnAmountHasNoMinorUnit_ThenItRoundsToWholeUnits() =>
        new Amount(2.5m, Gold).Value.Should().Be(2m);

    [Fact]
    public void WhenFormattingGold_ThenItShowsWholeUnits() =>
        new Amount(12m, Gold).ToString("C", CultureInfo.GetCultureInfo("en-US")).Should().Be("¤12");

    [Fact]
    public void WhenDividingGold_ThenNothingIsLost()
    {
        var shares = new Amount(10m, Gold).SafeDivide(3).ToList();

        shares.Select(share => share.Value).Should().Equal(3m, 3m, 4m);
    }

    [Fact]
    public void WhenDividingAriary_ThenEachShareIsAWholeNumberOfFifths()
    {
        var shares = new Amount(1m, Ariary).SafeDivide(3).ToList();

        shares.Select(share => share.Value).Should().Equal(0.4m, 0.4m, 0.2m);
        shares.Sum(share => share.Value).Should().Be(1m);
    }
}
