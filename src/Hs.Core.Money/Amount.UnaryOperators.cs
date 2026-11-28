namespace Hs.Core.Money;

/// <summary>Represents Money, an amount defined in a specific Currency.</summary>
// This partial keeps operators grouped in their own file.
public readonly partial record struct Amount // NOSONAR
{
    /// <summary>Implements the operator +.</summary>
    /// <param name="money">The money.</param>
    /// <returns>The result of the operator.</returns>
    public static Amount operator +(Amount money) => Plus(money);

    /// <summary>Implements the operator -.</summary>
    /// <param name="money">The money.</param>
    /// <returns>The result of the operator.</returns>
    public static Amount operator -(Amount money) => Negate(money);

    /// <summary>Implements the operator ++.</summary>
    /// <param name="money">The money.</param>
    /// <returns>The result of the operator.</returns>
    public static Amount operator ++(Amount money) => Increment(money);

    /// <summary>Implements the operator --.</summary>
    /// <param name="money">The money.</param>
    /// <returns>The result of the operator.</returns>
    public static Amount operator --(Amount money) => Decrement(money);

    /// <summary>Pluses the specified money.</summary>
    /// <param name="money">The money.</param>
    /// <returns>The result.</returns>
    public static Amount Plus(Amount money) => new(+money.Value, money.Currency);

    /// <summary>Negates the specified money.</summary>
    /// <param name="money">The money.</param>
    /// <returns>The result.</returns>
    public static Amount Negate(Amount money) => new(-money.Value, money.Currency);

    /// <summary>Increments the specified money.</summary>
    /// <param name="money">The money.</param>
    /// <returns>The result.</returns>
    public static Amount Increment(Amount money) => Add(money, new Amount(money.Currency.MinorUnit, money.Currency));

    /// <summary>Decrements the specified money.</summary>
    /// <param name="money">The money.</param>
    /// <returns>The result.</returns>
    public static Amount Decrement(Amount money) => Subtract(money, new Amount(money.Currency.MinorUnit, money.Currency));
}
