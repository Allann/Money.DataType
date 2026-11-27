namespace Hs.Core.Languages;

/// <summary>The grammar of one language: how it words digits, signs and whole numbers.</summary>
internal interface ILanguage
{
    string Minus { get; }

    string Point { get; }

    string Digit(int digit);

    /// <summary>Words a whole number given as its non-zero triplets, most significant first (empty for zero).</summary>
    string Whole(Triplet[] triplets);
}
