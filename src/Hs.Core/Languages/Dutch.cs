namespace Hs.Core.Languages;

/// <summary>
/// Dutch, long scale: "eenentwintig", "duizend vijf", "twee miljard".
/// Follows the Taalunie spelling: one word up to a thousand, a space after "duizend", "miljoen", etc.
/// </summary>
internal sealed class Dutch : ILanguage
{
    private static readonly string[] Units =
    [
        "nul", "een", "twee", "drie", "vier", "vijf", "zes", "zeven", "acht", "negen",
        "tien", "elf", "twaalf", "dertien", "veertien", "vijftien", "zestien", "zeventien", "achttien", "negentien",
    ];

    private static readonly string[] Tens =
        ["", "", "twintig", "dertig", "veertig", "vijftig", "zestig", "zeventig", "tachtig", "negentig"];

    private static readonly string[] Scales =
        ["", "duizend", "miljoen", "miljard", "biljoen", "biljard", "triljoen", "triljard", "quadriljoen", "quadriljard"];

    public string Digit(int digit) => Units[digit];

    public string Negative(string magnitude) => $"min {magnitude}";

    public string WithFraction(string whole, IEnumerable<string> digits) => $"{whole} komma {string.Join(" ", digits)}";

    public string Whole(decimal whole) => Whole(DigitGroup.Split(whole, 1000));

    private static string Whole(DigitGroup[] groups) =>
        groups is [] ? "nul" : string.Join(" ", groups.Select(Group));

    private static string Group(DigitGroup group) => group switch
    {
        { Scale: 0 } => InWords(group),
        { Scale: 1, Value: 1 } => "duizend",
        { Scale: 1 } => $"{InWords(group)}duizend",
        _ => $"{InWords(group)} {Scales[group.Scale]}",
    };

    private static string InWords(DigitGroup group) =>
        Hundreds(group.Hundreds) + (group.Remainder == 0 ? "" : BelowHundred(group.Remainder));

    private static string Hundreds(int hundreds) => hundreds switch
    {
        0 => "",
        1 => "honderd",
        _ => $"{Units[hundreds]}honderd",
    };

    /// <summary>Units come before tens, joined by "en" (or "ën" after a vowel): 21 is "eenentwintig", 22 is "tweeëntwintig".</summary>
    private static string BelowHundred(int number) => number switch
    {
        < 20 => Units[number],
        _ when number % 10 == 0 => Tens[number / 10],
        _ => $"{Units[number % 10]}{(Units[number % 10].EndsWith('e') ? "ën" : "en")}{Tens[number / 10]}",
    };
}
