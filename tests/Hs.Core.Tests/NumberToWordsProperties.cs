using System.Globalization;
using CsCheck;

namespace Hs.Core.Tests;

/// <summary>Laws for every decimal, in every supported language, checked over generated inputs.</summary>
public class NumberToWordsProperties
{
    private static readonly Gen<CultureInfo> Languages =
        Gen.OneOf(CultureInfo.GetCultureInfo("en-AU"), CultureInfo.GetCultureInfo("nl-NL"), CultureInfo.GetCultureInfo("ja-JP"));

    // How each language marks a negative number and a decimal point.
    private static readonly Dictionary<string, (string Minus, string Point)> Marks = new()
    {
        ["en"] = ("minus ", "point"),
        ["nl"] = ("min ", "komma"),
        ["ja"] = ("マイナス", "点"),
    };

    // The whole range, with extra weight on whole numbers and on values near the limits.
    private static readonly Gen<decimal> Numbers = Gen.Frequency(
        (4, Gen.Decimal),
        (4, Gen.Long.Select(n => (decimal)n)),
        (1, Gen.OneOf(decimal.MinValue, decimal.MaxValue, 0m, 0.0000000000000000000000000001m)));

    [Fact]
    public void EveryNumberHasWords() =>
        Numbers.Select(Languages).Sample((number, culture) => !string.IsNullOrWhiteSpace(number.ToWords(culture).Value));

    [Fact]
    public void ANegativeNumberIsMinusItsMagnitude() =>
        Numbers.Where(n => n < 0).Select(Languages).Sample((number, culture) =>
            number.ToWords(culture).Value == Marks[culture.TwoLetterISOLanguageName].Minus + (-number).ToWords(culture).Value);

    [Fact]
    public void DifferentWholeNumbersHaveDifferentWords() =>
        Gen.Long.Select(Gen.Long, Languages).Where((a, b, _) => a != b).Sample((a, b, culture) =>
            ((decimal)a).ToWords(culture) != ((decimal)b).ToWords(culture));

    [Fact]
    public void WholeNumbersHaveNoPoint() =>
        Gen.Long.Select(Languages).Sample((number, culture) =>
            !((decimal)number).ToWords(culture).Value.Contains(Marks[culture.TwoLetterISOLanguageName].Point, StringComparison.Ordinal));
}
