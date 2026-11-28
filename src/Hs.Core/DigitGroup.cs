namespace Hs.Core;

/// <summary>
/// A group of digits within a whole number, and its position: in groups of a thousand (English, Dutch)
/// 1,234,567 is 1 · 234 · 567; in groups of ten thousand (Japanese) it is 123 · 4567.
/// </summary>
/// <param name="Value">The group's value, 1 up to the group size minus one.</param>
/// <param name="Scale">The power of the group size: 0 for the units, 1 for thousands (or 万), and so on.</param>
internal readonly record struct DigitGroup(int Value, int Scale)
{
    /// <summary>The hundreds digit, for groups of a thousand.</summary>
    public int Hundreds => Value / 100;

    /// <summary>The last two digits.</summary>
    public int Remainder => Value % 100;

    /// <summary>Splits a whole number into its non-zero groups of <paramref name="size"/>, most significant first.</summary>
    public static DigitGroup[] Split(decimal whole, int size) =>
        [.. Groups(whole, size, scale: 0).Where(group => group.Value > 0).Reverse()];

    private static IEnumerable<DigitGroup> Groups(decimal whole, int size, int scale) =>
        whole < size
            ? [new DigitGroup((int)whole, scale)]
            : [new DigitGroup((int)(whole % size), scale), .. Groups(decimal.Truncate(whole / size), size, scale + 1)];
}
