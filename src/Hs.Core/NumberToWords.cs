using System.Globalization;
using Hs.Core.Languages;

namespace Hs.Core;

/// <summary>
/// Writes numbers out in words in the language of a culture, covering the full range of <see cref="decimal"/>.
/// Supported languages: English, Dutch and Japanese; any other culture falls back to English.
/// </summary>
public static class NumberToWords
{
    private static readonly ILanguage Fallback = new English();

    private static readonly Dictionary<string, ILanguage> Languages = new()
    {
        ["en"] = Fallback,
        ["nl"] = new Dutch(),
        ["ja"] = new Japanese(),
    };

    /// <summary>Writes <paramref name="number"/> out in words in the language of the current culture.</summary>
    public static Words ToWords(this decimal number) => number.ToWords(CultureInfo.CurrentCulture);

    /// <summary>
    /// Writes <paramref name="number"/> out in words in the language of <paramref name="culture"/>,
    /// e.g. <c>21</c> is "twenty-one" in en-AU and "eenentwintig" in nl-NL.
    /// </summary>
    public static Words ToWords(this decimal number, CultureInfo culture) =>
        new(Signed(number, LanguageOf(culture)));

    private static ILanguage LanguageOf(CultureInfo culture) =>
        Languages.GetValueOrDefault(culture.TwoLetterISOLanguageName, Fallback);

    private static string Signed(decimal number, ILanguage language) =>
        number < 0 ? language.Negative(Unsigned(-number, language)) : Unsigned(number, language);

    private static string Unsigned(decimal number, ILanguage language)
    {
        var whole = decimal.Truncate(number);
        var fraction = number - whole;
        var words = language.Whole(whole);

        return fraction == 0 ? words : language.WithFraction(words, FractionDigits(fraction).Select(language.Digit));
    }

    private static int[] FractionDigits(decimal fraction)
    {
        if (fraction == 0) return [];

        var shifted = fraction * 10;
        var digit = decimal.Truncate(shifted);
        return [(int)digit, .. FractionDigits(shifted - digit)];
    }
}
