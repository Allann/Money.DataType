using System.Globalization;

namespace Hs.Core.Money;

/// <summary>
/// Formatting an <see cref="Amount"/>. The number format comes from the format provider (or the current culture),
/// with the symbol and decimal digits of the amount's <see cref="Currency"/>.
/// </summary>
/// <remarks>
/// Format strings, each with an optional precision such as <c>"C3"</c>:
/// <list type="table">
/// <item><term>C, G or none</term><description>Currency with its symbol: "€ 1.234,56".</description></item>
/// <item><term>I</term><description>Currency with its ISO code: "EUR 1.234,56".</description></item>
/// <item><term>F</term><description>Number followed by the English currency name: "1.234,56 Euro".</description></item>
/// <item><term>W</term><description>In words, in the format provider's language: "one thousand two hundred and thirty-four euros and fifty-six cents".</description></item>
/// <item><term>anything else</term><description>A standard or custom numeric format applied to the value, e.g. "N2".</description></item>
/// </list>
/// </remarks>
public readonly partial record struct Amount : IFormattable
{
    // NumberFormatInfo currency patterns, with a space between the code and the number for the "I" format.
    // Positive: $n → $ n, n$ → n $.  Negative: ($n) → ($ n), -$n → -$ n, and so on; patterns 8-16 already have a space.
    private static readonly int[] SpacedPositivePattern = [2, 3, 2, 3];
    private static readonly int[] SpacedNegativePattern = [14, 9, 12, 11, 15, 8, 13, 10, 8, 9, 10, 11, 12, 13, 14, 15, 16];

    /// <summary>The amount in the current culture's format, with the currency's symbol, e.g. "$1,234.56".</summary>
    public override string ToString() => ToString(null, null);

    /// <summary>The amount in the current culture's number format, using <paramref name="format"/>.</summary>
    public string ToString(string? format) => ToString(format, null);

    /// <summary>The amount in the number format of <paramref name="formatProvider"/>, with the currency's symbol.</summary>
    public string ToString(IFormatProvider? formatProvider) => ToString(null, formatProvider);

    /// <summary>The amount using <paramref name="format"/> in the number format of <paramref name="formatProvider"/>.</summary>
    /// <exception cref="FormatException"><paramref name="format"/> isn't a valid format string.</exception>
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        var (specifier, precision) = Specifier(format);
        return specifier switch
        {
            "" or "G" when precision == "" => Value.ToString("C", CurrencyFormat(Currency, formatProvider)),
            "I" => Value.ToString($"C{precision}", CurrencyFormat(Currency, formatProvider, withIsoCode: true)),
            "F" => $"{Value.ToString($"N{(precision == "" ? Currency.DecimalDigits : precision)}", CurrencyFormat(Currency, formatProvider))} {Currency.EnglishName}",
            "W" => ToWords(formatProvider as CultureInfo ?? CultureInfo.CurrentCulture).Value,
            _ => Value.ToString(format, CurrencyFormat(Currency, formatProvider)),
        };
    }

    /// <summary>
    /// The number format of <paramref name="provider"/> (or the current culture) with the symbol, or ISO code,
    /// and decimal digits of <paramref name="currency"/>.
    /// </summary>
    internal static NumberFormatInfo CurrencyFormat(Currency currency, IFormatProvider? provider, bool withIsoCode = false)
    {
        var format = (NumberFormatInfo)NumberFormatInfo.GetInstance(provider).Clone();
        format.CurrencyDecimalDigits = currency.DecimalDigits;
        format.CurrencySymbol = (withIsoCode ? currency.Code : currency.Symbol) ?? string.Empty;

        if (withIsoCode)
        {
            format.CurrencyPositivePattern = SpacedPositivePattern[format.CurrencyPositivePattern];
            format.CurrencyNegativePattern = SpacedNegativePattern[format.CurrencyNegativePattern];
        }

        return format;
    }

    /// <summary>Splits a format into its letter and precision, e.g. "I2" into ("I", "2"); other formats come back whole, with no precision.</summary>
    private static (string Specifier, string Precision) Specifier(string? format) => format switch
    {
        null or "" => ("", ""),
        [var letter, .. var digits] when char.IsAsciiLetter(letter) && digits.All(char.IsAsciiDigit) => (letter.ToString(), digits),
        _ => (format, ""),
    };
}
