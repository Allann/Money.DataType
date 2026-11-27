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

    public string Minus => "min";

    public string Point => "komma";

    public string Digit(int digit) => Units[digit];

    public string Whole(Triplet[] triplets) =>
        triplets is [] ? "nul" : string.Join(" ", triplets.Select(Group));

    private static string Group(Triplet triplet) => triplet switch
    {
        { Scale: 0 } => InWords(triplet),
        { Scale: 1, Value: 1 } => "duizend",
        { Scale: 1 } => $"{InWords(triplet)}duizend",
        _ => $"{InWords(triplet)} {Scales[triplet.Scale]}",
    };

    private static string InWords(Triplet triplet) =>
        Hundreds(triplet.Hundreds) + (triplet.Remainder == 0 ? "" : BelowHundred(triplet.Remainder));

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
