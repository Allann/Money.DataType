namespace Hs.Core.Languages;

/// <summary>
/// The grammar of one language: how it words whole numbers, digits, negatives and fractions.
/// Languages differ in more than vocabulary (how digits group, whether words are spaced), so each owns all of it.
/// </summary>
internal interface ILanguage
{
    /// <summary>Words a non-negative whole number, e.g. "one thousand and five".</summary>
    string Whole(decimal whole);

    /// <summary>Words a single digit after the decimal point.</summary>
    string Digit(int digit);

    /// <summary>Words a negative number, given the words for its magnitude, e.g. "minus five".</summary>
    string Negative(string magnitude);

    /// <summary>Joins the words for the whole part and each fraction digit, e.g. "twelve point zero five".</summary>
    string WithFraction(string whole, IEnumerable<string> digits);
}
