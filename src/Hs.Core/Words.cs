namespace Hs.Core;

/// <summary>A number written out in English words, e.g. "one hundred and five".</summary>
public readonly record struct Words(string Value)
{
    /// <inheritdoc />
    public override string ToString() => Value;
}
