namespace Hs.Core.Languages;

/// <summary>
/// Japanese, in kanji: "千二百三十四" (1234), "一万二千三百四十五" (12345), "三点一四" (3.14).
/// Digits group in fours (万 10⁴, 億 10⁸, 兆 10¹² …), words aren't spaced, and "one" is left off
/// 十, 百 and 千 but kept before 万 and larger: 千 (1000) but 一万 (10000).
/// </summary>
internal sealed class Japanese : ILanguage
{
    private const int GroupSize = 10_000;

    private static readonly string[] Digits = ["零", "一", "二", "三", "四", "五", "六", "七", "八", "九"];

    // Powers of ten within a group of four, least significant first: ones, 十, 百, 千.
    private static readonly string[] Places = ["", "十", "百", "千"];

    // Powers of 万 (10⁴), up to 穣 (10²⁸), which covers decimal.MaxValue.
    private static readonly string[] Scales = ["", "万", "億", "兆", "京", "垓", "秭", "穣"];

    public string Digit(int digitValue) => Digits[digitValue];

    public string Negative(string magnitude) => $"マイナス{magnitude}";

    public string WithFraction(string whole, IEnumerable<string> digits) => $"{whole}点{string.Concat(digits)}";

    public string Whole(decimal number) =>
        DigitGroup.Split(number, GroupSize) switch
        {
            [] => Digits[0],
            var groups => string.Concat(groups.Select(group => InWords(group.Value) + Scales[group.Scale])),
        };

    /// <summary>Words 1-9999: each non-zero digit with its place, e.g. 1205 is 千 + 二百 + 五.</summary>
    private static string InWords(int value) =>
        string.Concat(Enumerable.Range(0, Places.Length).Reverse().Select(place => Place(value / Pow10(place) % 10, place)));

    private static string Place(int digitValue, int placeValue) => (digitValue, placeValue) switch
    {
        (0, _) => "",
        (_, 0) => Digits[digitValue],
        (1, _) => Places[placeValue],
        _ => Digits[digitValue] + Places[placeValue],
    };

    private static int Pow10(int exponent) => exponent switch { 0 => 1, 1 => 10, 2 => 100, _ => 1000 };
}
