using System.Globalization;

namespace Hs.Core.Money.Wording;

/// <summary>Writes an <see cref="Amount"/> out in words, e.g. "one thousand dollars and five cents".</summary>
internal static class AmountWords
{
    public static Words Write(Amount amount, CultureInfo culture)
    {
        var language = MoneyLanguage.Of(culture);

        return language.Units.TryGetValue(amount.Currency.Code, out var units)
            ? new Words(InUnits(Split(amount), units, language, culture))
            : new Words($"{amount.Value.ToWords(culture)} {amount.Currency.EnglishName ?? amount.Currency.Code}");
    }

    /// <summary>
    /// Splits into whole major and minor units, e.g. 12.34 USD is (12, 34).
    /// An <see cref="Amount"/> is already rounded to its currency's minor unit when constructed.
    /// </summary>
    private static Parts Split(Amount amount)
    {
        var minorPerMajor = amount.Currency.MajorUnit / amount.Currency.MinorUnit;
        var minorUnits = Math.Abs(amount.Value) * minorPerMajor;

        return new Parts(
            IsNegative: amount.Value < 0,
            Major: decimal.Truncate(minorUnits / minorPerMajor),
            Minor: minorUnits % minorPerMajor);
    }

    private static string InUnits(Parts parts, CurrencyUnits units, MoneyLanguage language, CultureInfo culture)
    {
        var words = parts switch
        {
            { Major: 0, Minor: > 0 } => Count(parts.Minor, units.Minor, language, culture),
            { Minor: 0 } => Count(parts.Major, units.Major, language, culture),
            _ => $"{Count(parts.Major, units.Major, language, culture)}{language.And}{Count(parts.Minor, units.Minor, language, culture)}",
        };

        return parts.IsNegative ? language.Minus + words : words;
    }

    private static string Count(decimal unitCount, UnitName unit, MoneyLanguage language, CultureInfo culture) =>
        $"{unitCount.ToWords(culture)}{language.Space}{unit.For(unitCount)}";

    private readonly record struct Parts(bool IsNegative, decimal Major, decimal Minor);
}
