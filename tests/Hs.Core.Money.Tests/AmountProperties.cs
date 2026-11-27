using System.ComponentModel;
using System.Globalization;
using CsCheck;
using Hs.Core.Money.Extensions;

namespace Hs.Core.Money.Tests.Properties;

/// <summary>Laws that must hold for every amount, checked over generated inputs (CsCheck shrinks any counterexample).</summary>
public class AmountProperties
{
    // A spread of minor units: hundredths, none (yen), thousandths (dinar), fifths (ariary) and "N.A." (gold).
    private static readonly Gen<Currency> Currencies =
        Gen.OneOf("USD", "JPY", "BHD", "MGA", "XAU", "EUR").Select(Currency.FromCode);

    // Large enough to cross every digit group; small enough that sums and products stay inside decimal.
    private static readonly Gen<decimal> Values = Gen.Decimal[-1_000_000_000_000m, 1_000_000_000_000m];

    private static readonly Gen<Amount> Amounts =
        from currency in Currencies
        from value in Values
        select new Amount(value, currency);

    private static readonly CultureInfo[] Cultures =
        [CultureInfo.GetCultureInfo("en-US"), CultureInfo.GetCultureInfo("nl-NL"), CultureInfo.GetCultureInfo("ja-JP"), CultureInfo.InvariantCulture];

    private static bool IsWholeNumberOf(decimal value, decimal unit) => value % unit == 0;

    [Fact]
    public void AnAmountIsAlwaysAWholeNumberOfMinorUnits() =>
        Amounts.Sample(amount => IsWholeNumberOf(amount.Value, amount.Currency.MinorUnit));

    [Fact]
    public void RoundingAnAmountAgainChangesNothing() =>
        Amounts.Sample(amount => new Amount(amount.Value, amount.Currency) == amount);

    [Fact]
    public void DividingIntoSharesNeverLosesOrCreatesMoney() =>
        Amounts.Select(Gen.Int[2, 20]).Sample((amount, shares) =>
        {
            var parts = amount.SafeDivide(shares).ToList();

            return parts.Count == shares
                && parts.Sum(part => part.Value) == amount.Value
                && parts.All(part => part.Currency == amount.Currency && IsWholeNumberOf(part.Value, amount.Currency.MinorUnit));
        });

    [Fact]
    public void DividingByRatiosNeverLosesOrCreatesMoney() =>
        Amounts.Select(Gen.Int[1, 50].Array[1, 10]).Sample((amount, ratios) =>
        {
            var parts = amount.SafeDivide(ratios).ToList();

            return parts.Count == ratios.Length
                && parts.Sum(part => part.Value) == amount.Value
                && parts.All(part => IsWholeNumberOf(part.Value, amount.Currency.MinorUnit));
        });

    [Fact]
    public void AddingThenSubtractingGivesTheOriginal() =>
        Currencies.SelectMany(currency => Values.Select(Values, (a, b) => (new Amount(a, currency), new Amount(b, currency))))
            .Sample((a, b) => a + b - b == a && a + b == b + a);

    [Fact]
    public void SubtractingFromADecimalIsTheNegationOfSubtractingTheDecimal() =>
        Amounts.Select(Values).Sample((amount, value) => value - amount == -(amount - value));

    [Fact]
    public void NegatingTwiceGivesTheOriginal() =>
        Amounts.Sample(amount => -(-amount) == amount);

    [Fact]
    public void FormattingThenParsingGivesTheOriginal() =>
        Amounts.Select(Gen.OneOf(Cultures)).Sample((amount, culture) =>
            Amount.Parse(amount.ToString("C", culture), NumberStyles.Currency, culture, amount.Currency) == amount);

    [Fact]
    public void JsonRoundTripsAnyAmount() =>
        Amounts.Sample(amount => amount.ToJson().FromJson<Amount>() == amount);

    [Fact]
    public void TheTypeConverterRoundTripsAnyAmount()
    {
        var converter = TypeDescriptor.GetConverter(typeof(Amount));
        Amounts.Sample(amount => (Amount)converter.ConvertFromInvariantString(converter.ConvertToInvariantString(amount)!)! == amount);
    }

    [Fact]
    public void AnyAmountCanBeWrittenInWords() =>
        Amounts.Select(Gen.OneOf(Cultures)).Sample((amount, culture) => amount.ToWords(culture).Value.Length > 0);
}
