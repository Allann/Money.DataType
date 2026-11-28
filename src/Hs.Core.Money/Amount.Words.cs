using System.Globalization;
using Hs.Core.Money.Wording;

namespace Hs.Core.Money;

public readonly partial record struct Amount
{
    /// <summary>Writes this amount out in words in the language of the current culture, e.g. "five dollars and twenty cents".</summary>
    /// <remarks>Also available as the <c>"W"</c> format: <c>amount.ToString("W", culture)</c>.</remarks>
    public Words ToWords() => ToWords(CultureInfo.CurrentCulture);

    /// <summary>
    /// Writes this amount out in words in the language of <paramref name="culture"/>, rounded to the currency's minor unit,
    /// e.g. USD 1.05 is "one dollar and five cents" in en-AU and "een dollar en vijf cent" in nl-NL.
    /// </summary>
    /// <remarks>Currencies without unit names are written as the number followed by the currency name.</remarks>
    public Words ToWords(CultureInfo culture) => AmountWords.Write(this, culture);
}
