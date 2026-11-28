namespace Hs.Core.Money;

/// <summary>Conversions between <see cref="Amount"/> and the numeric types.</summary>
/// <remarks>
/// Conversions from a number to an <see cref="Amount"/> use the currency of the current culture,
/// see <see cref="Currency.CurrentCurrency"/>. Conversions to a number drop the <see cref="Currency"/>.
/// </remarks>
// This partial keeps conversions grouped in their own file.
public readonly partial record struct Amount // NOSONAR
{
    /// <summary>Converts to a <see cref="double"/>, which may lose precision and drops the currency.</summary>
    public static explicit operator double(Amount money) => (double)money.Value;

    /// <summary>Converts to a <see cref="long"/>, truncating any fraction and dropping the currency.</summary>
    public static explicit operator long(Amount money) => (long)money.Value;

    /// <summary>Converts to a <see cref="decimal"/>, dropping the currency.</summary>
    public static explicit operator decimal(Amount money) => money.Value;

    /// <summary>Converts a <see cref="long"/> to an amount in the current culture's currency.</summary>
    public static implicit operator Amount(long money) => new(money);

    /// <summary>Converts a <see cref="ulong"/> to an amount in the current culture's currency.</summary>
    public static implicit operator Amount(ulong money) => new(money);

    /// <summary>Converts a <see cref="byte"/> to an amount in the current culture's currency.</summary>
    public static implicit operator Amount(byte money) => new(money);

    /// <summary>Converts a <see cref="ushort"/> to an amount in the current culture's currency.</summary>
    public static implicit operator Amount(ushort money) => new(money);

    /// <summary>Converts a <see cref="uint"/> to an amount in the current culture's currency.</summary>
    public static implicit operator Amount(uint money) => new(money);

    /// <summary>Converts a <see cref="double"/> to an amount in the current culture's currency.</summary>
    public static explicit operator Amount(double money) => new((decimal)money);

    /// <summary>Converts a <see cref="decimal"/> to an amount in the current culture's currency.</summary>
    public static implicit operator Amount(decimal money) => new(money);

    /// <summary>Converts to a <see cref="float"/>, which may lose precision and drops the currency.</summary>
    public static float ToSingle(Amount money) => (float)money.Value;

    /// <summary>Converts to a <see cref="double"/>, which may lose precision and drops the currency.</summary>
    public static double ToDouble(Amount money) => (double)money;

    /// <summary>Converts to a <see cref="decimal"/>, dropping the currency.</summary>
    public static decimal ToDecimal(Amount money) => money.Value;

    /// <summary>Converts a <see cref="long"/> to an amount in the current culture's currency.</summary>
    public static Amount FromInt64(long money) => money;

    /// <summary>Converts a <see cref="ulong"/> to an amount in the current culture's currency.</summary>
    public static Amount FromUInt64(ulong money) => money;

    /// <summary>Converts a <see cref="byte"/> to an amount in the current culture's currency.</summary>
    public static Amount FromByte(byte money) => money;

    /// <summary>Converts a <see cref="ushort"/> to an amount in the current culture's currency.</summary>
    public static Amount FromUInt16(ushort money) => money;

    /// <summary>Converts a <see cref="uint"/> to an amount in the current culture's currency.</summary>
    public static Amount FromUInt32(uint money) => money;

    /// <summary>Converts a <see cref="double"/> to an amount in the current culture's currency.</summary>
    public static Amount FromDouble(double money) => (Amount)money;

    /// <summary>Converts a <see cref="decimal"/> to an amount in the current culture's currency.</summary>
    public static Amount FromDecimal(decimal money) => money;
}
