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

    public string Digit(int digitValue) => Units[digitValue];

    public string Negative(string magnitude) => $"minus {magnitude}";

    public string WithFraction(string whole, IEnumerable<string> digits) => $"{whole} point {string.Join(" ", digits)}";

    public string Whole(decimal number) => Whole(DigitGroup.Split(number, 1000));

    private static string Whole(DigitGroup[] groups) => groups switch
    {
        [] => "zero",
        [_, .., { Scale: 0, Hundreds: 0 } last] => $"{Join(groups[..^1])} and {InWords(last)}",
        _ => Join(groups),
    };

    private static string Join(IEnumerable<DigitGroup> groups) => string.Join(" ", groups.Select(Group));

    private static string Group(DigitGroup digitGroup) =>
        digitGroup.Scale == 0 ? InWords(digitGroup) : $"{InWords(digitGroup)} {Scales[digitGroup.Scale]}";

    private static string InWords(DigitGroup group) => (group.Hundreds, group.Remainder) switch
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
