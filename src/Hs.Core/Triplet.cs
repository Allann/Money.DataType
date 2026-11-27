namespace Hs.Core;

/// <summary>
/// A group of three digits (1-999) and its position in the number,
/// where <see cref="Scale"/> is the power of a thousand (0 = units, 1 = thousands, 2 = millions, ...).
/// </summary>
internal readonly record struct Triplet(int Value, int Scale)
{
    public int Hundreds => Value / 100;

    public int Remainder => Value % 100;

    /// <summary>Splits a whole number into its non-zero triplets, most significant first.</summary>
    public static Triplet[] Split(decimal whole) => [.. Groups(whole, scale: 0).Where(t => t.Value > 0).Reverse()];

    private static IEnumerable<Triplet> Groups(decimal whole, int scale) =>
        whole < 1000
            ? [new Triplet((int)whole, scale)]
            : [new Triplet((int)(whole % 1000), scale), .. Groups(decimal.Truncate(whole / 1000), scale + 1)];
}
