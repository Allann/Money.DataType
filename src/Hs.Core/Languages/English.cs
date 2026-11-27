namespace Hs.Core.Languages;

/// <summary>British English, short scale: "one thousand and five", "two billion".</summary>
internal sealed class English : ILanguage
{
    private static readonly string[] Units =
    [
        "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine",
        "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen",
    ];

    private static readonly string[] Tens =
        ["", "", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety"];

    private static readonly string[] Scales =
        ["", "thousand", "million", "billion", "trillion", "quadrillion", "quintillion", "sextillion", "septillion", "octillion"];

    public string Minus => "minus";

    public string Point => "point";

    public string Digit(int digit) => Units[digit];

    public string Whole(Triplet[] triplets) => triplets switch
    {
        [] => "zero",
        [_, .., { Scale: 0, Hundreds: 0 } last] => $"{Join(triplets[..^1])} and {InWords(last)}",
        _ => Join(triplets),
    };

    private static string Join(IEnumerable<Triplet> triplets) => string.Join(" ", triplets.Select(Group));

    private static string Group(Triplet triplet) =>
        triplet.Scale == 0 ? InWords(triplet) : $"{InWords(triplet)} {Scales[triplet.Scale]}";

    private static string InWords(Triplet triplet) => (triplet.Hundreds, triplet.Remainder) switch
    {
        (0, var remainder) => BelowHundred(remainder),
        (var hundreds, 0) => $"{Units[hundreds]} hundred",
        (var hundreds, var remainder) => $"{Units[hundreds]} hundred and {BelowHundred(remainder)}",
    };

    private static string BelowHundred(int number) => number switch
    {
        < 20 => Units[number],
        _ when number % 10 == 0 => Tens[number / 10],
        _ => $"{Tens[number / 10]}-{Units[number % 10]}",
    };
}
