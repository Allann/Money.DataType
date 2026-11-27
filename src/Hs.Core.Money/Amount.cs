using System.Text.Json.Serialization;

namespace Hs.Core.Money;

/// <summary>Represents Money, an amount defined in a specific Currency.</summary>
/// <remarks>
/// The <see cref="Amount"/> structure allows development of applications that handle
/// various types of Currency. Amount will hold the <see cref="Currency"/> and Value of monetary values,
/// and ensure that two different currencies cannot be combined arithmetically.
/// </remarks>
[JsonConverter(typeof(AmountJsonConverter))]
public readonly partial record struct Amount
{
    /// <summary>Initializes a new instance of the <see cref="Amount"/> struct, based on the current culture.</summary>
    /// <param name="value">The Amount of money as <see langword="decimal"/>.</param>
    /// <remarks>The amount will be rounded to the number of decimal digits of the specified currency
    /// (<see cref="Money.Currency.DecimalDigits"/>). As rounding mode, MidpointRounding.ToEven is used
    /// (<see cref="MidpointRounding"/>). The behavior of this method follows IEEE Standard 754, section 4. This
    /// kind of rounding is sometimes called rounding to nearest, or banker's rounding. It minimizes rounding errors that
    /// result from consistently rounding a midpoint value in a single direction.</remarks>
    public Amount(decimal value)
        : this(value, Currency.CurrentCurrency)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="Amount"/> struct, based on a ISO 4217 Currency code.</summary>
    /// <param name="value">The Amount of money as <see langword="decimal"/>.</param>
    /// <param name="currency">A ISO 4217 Currency code, like EUR or USD.</param>
    /// <remarks>The amount will be rounded to the number of decimal digits of the specified currency
    /// (<see cref="Money.Currency.DecimalDigits"/>). As rounding mode, MidpointRounding.ToEven is used
    /// (<see cref="MidpointRounding"/>). The behavior of this method follows IEEE Standard 754, section 4. This
    /// kind of rounding is sometimes called rounding to nearest, or banker's rounding. It minimizes rounding errors that
    /// result from consistently rounding a midpoint value in a single direction.</remarks>
    public Amount(decimal value, string currency)
        : this(value, Currency.FromCode(currency))
    {
    }

    /// <summary>Initializes a new instance of the <see cref="Amount"/> struct, based on the current culture.</summary>
    /// <param name="value">The Amount of money as <see langword="decimal"/>.</param>
    /// <param name="rounding">The rounding mode.</param>
    /// <remarks>The amount will be rounded to the number of decimal digits of the specified currency
    /// (<see cref="Money.Currency.DecimalDigits"/>).</remarks>
    public Amount(decimal value, MidpointRounding rounding)
        : this(value, Currency.CurrentCurrency, rounding)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="Amount"/> struct.</summary>
    /// <param name="value">The Amount of money as <see langword="decimal"/>.</param>
    /// <param name="currency">The Currency of the money.</param>
    /// <remarks>The amount will be rounded to the number of decimal digits of the specified currency
    /// (<see cref="Money.Currency.DecimalDigits"/>). As rounding mode, MidpointRounding.ToEven is used
    /// (<see cref="MidpointRounding"/>). The behavior of this method follows IEEE Standard 754, section 4. This
    /// kind of rounding is sometimes called rounding to nearest, or banker's rounding. It minimizes rounding errors that
    /// result from consistently rounding a midpoint value in a single direction.</remarks>
    public Amount(decimal value, Currency currency)
        : this(value, currency, MidpointRounding.ToEven)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="Amount"/> struct, based on a ISO 4217 Currency code.</summary>
    /// <param name="value">The Amount of money as <see langword="decimal"/>.</param>
    /// <param name="currency">A ISO 4217 Currency code, like EUR or USD.</param>
    /// <param name="rounding">The rounding mode.</param>
    /// <remarks>The amount will be rounded to the number of decimal digits of the specified currency
    /// (<see cref="Money.Currency.DecimalDigits"/>).</remarks>
    public Amount(decimal value, string currency, MidpointRounding rounding)
        : this(value, Currency.FromCode(currency), rounding)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="Amount"/> struct.</summary>
    /// <param name="value">The Amount of money as <see langword="decimal"/>.</param>
    /// <param name="currency">The Currency of the money.</param>
    /// <param name="rounding">The rounding mode.</param>
    /// <remarks>The amount will be rounded to the number of decimal digits of the specified currency
    /// (<see cref="Money.Currency.DecimalDigits"/>).</remarks>
    public Amount(decimal value, Currency currency, MidpointRounding rounding)
    {
        Currency = currency;
        Value = Round(value, currency, rounding);
    }
    /// <summary>Initializes a new instance of the <see cref="Amount"/> struct, based on the current culture.</summary>
    /// <param name="value">The Amount of money as <see langword="double"/> or <see langword="float"/> (float is implicitly
    /// casted to double).</param>
    /// <remarks>This constructor will first convert to decimal by rounding the value to 15 significant digits using rounding
    /// to nearest. This is done even if the number has more than 15 digits and the less significant digits are zero.
    /// <para>The amount will be rounded to the number of decimal digits of the specified currency
    /// (<see cref="Money.Currency.DecimalDigits"/>). As rounding mode, MidpointRounding.ToEven is used
    /// (<see cref="MidpointRounding"/>). The behavior of this method follows IEEE Standard 754, section 4. This
    /// kind of rounding is sometimes called rounding to nearest, or banker's rounding. It minimizes rounding errors that
    /// result from consistently rounding a midpoint value in a single direction.</para></remarks>
    public Amount(double value)
        : this((decimal)value)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="Amount"/> struct, based on a ISO 4217 Currency code.</summary>
    /// <param name="value">The Amount of money as <see langword="double"/> or <see langword="float"/> (float is implicitly
    /// casted to double).</param>
    /// <param name="currency">A ISO 4217 Currency code, like EUR or USD.</param>
    /// <remarks>This constructor will first convert to decimal by rounding the value to 15 significant digits using rounding
    /// to nearest. This is done even if the number has more than 15 digits and the less significant digits are zero.
    /// <para>The amount will be rounded to the number of decimal digits of the specified currency
    /// (<see cref="Money.Currency.DecimalDigits"/>). As rounding mode, MidpointRounding.ToEven is used
    /// (<see cref="MidpointRounding"/>). The behavior of this method follows IEEE Standard 754, section 4. This
    /// kind of rounding is sometimes called rounding to nearest, or banker's rounding. It minimizes rounding errors that
    /// result from consistently rounding a midpoint value in a single direction.</para></remarks>
    public Amount(double value, string currency)
        : this((decimal)value, Currency.FromCode(currency))
    {
    }

    /// <summary>Initializes a new instance of the <see cref="Amount"/> struct.</summary>
    /// <param name="value">The Amount of money as <see langword="double"/> or <see langword="float"/> (float is implicitly
    /// casted to double).</param>
    /// <param name="currency">The Currency of the money.</param>
    /// <remarks>This constructor will first convert to decimal by rounding the value to 15 significant digits using rounding
    /// to nearest. This is done even if the number has more than 15 digits and the less significant digits are zero.
    /// <para>The amount will be rounded to the number of decimal digits of the specified currency
    /// (<see cref="Money.Currency.DecimalDigits"/>). As rounding mode, MidpointRounding.ToEven is used
    /// (<see cref="MidpointRounding"/>). The behavior of this method follows IEEE Standard 754, section 4. This
    /// kind of rounding is sometimes called rounding to nearest, or banker's rounding. It minimizes rounding errors that
    /// result from consistently rounding a midpoint value in a single direction.</para></remarks>
    public Amount(double value, Currency currency)
        : this((decimal)value, currency)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="Amount"/> struct.</summary>
    /// <param name="value">The Amount of money as <see langword="double"/> or <see langword="float"/> (float is implicitly
    /// casted to double).</param>
    /// <param name="currency">The Currency of the money.</param>
    /// <param name="rounding">The rounding mode.</param>
    /// <remarks>This constructor will first convert to decimal by rounding the value to 15 significant digits using rounding
    /// to nearest. This is done even if the number has more than 15 digits and the less significant digits are zero.
    /// <para>The amount will be rounded to the number of decimal digits of the specified currency
    /// (<see cref="Money.Currency.DecimalDigits"/>).</para></remarks>
    public Amount(double value, Currency currency, MidpointRounding rounding)
        : this((decimal)value, currency, rounding)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="Amount"/> struct, based on the current culture.</summary>
    /// <param name="value">The Amount of money as <see langword="long"/>, <see langword="int"/>, <see langword="short"/> or<see cref="byte"/>.</param>
    /// <remarks>The integral types are implicitly converted to long and the result evaluates to decimal. Therefore you can
    /// initialize a Money object using an integer literal, without the suffix, as follows:
    /// <code>Money money = new Money(10, "EUR");</code></remarks>
    public Amount(long value)
        : this((decimal)value)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="Amount"/> struct, based on a ISO 4217 Currency code.</summary>
    /// <param name="value">The Amount of money as <see langword="long"/>, <see langword="int"/>, <see langword="short"/> or<see cref="byte"/>.</param>
    /// <param name="currency">A ISO 4217 Currency code, like EUR or USD.</param>
    /// <remarks>The integral types are implicitly converted to long and the result evaluates to decimal. Therefore you can
    /// initialize a Money object using an integer literal, without the suffix, as follows:
    /// <code>Money money = new Money(10, "EUR");</code></remarks>
    public Amount(long value, string currency)
        : this((decimal)value, Currency.FromCode(currency))
    {
    }

    /// <summary>Initializes a new instance of the <see cref="Amount"/> struct.</summary>
    /// <param name="value">The Amount of money as <see langword="long"/>, <see langword="int"/>, <see langword="short"/> or<see cref="byte"/>.</param>
    /// <param name="currency">The Currency of the money.</param>
    /// <remarks>The integral types are implicitly converted to long and the result evaluates to decimal. Therefore you can
    /// initialize a Money object using an integer literal, without the suffix, as follows:
    /// <code>Money money = new Money(10, "EUR");</code></remarks>
    public Amount(long value, Currency currency)
        : this((decimal)value, currency)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="Amount"/> struct, based on the current culture.</summary>
    /// <param name="value">The Amount of money as <see langword="ulong"/>, <see langword="uint"/>, <see langword="ushort"/>
    /// or <see cref="byte"/>.</param>
    /// <remarks>The integral types are implicitly converted to long and the result evaluates to decimal. Therefore you can
    /// initialize a Money object using an integer literal, without the suffix, as follows:
    /// <code>Money money = new Money(10, "EUR");</code></remarks>
    public Amount(ulong value)
        : this((decimal)value)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="Amount"/> struct, based on a ISO 4217 Currency code.</summary>
    /// <param name="value">The Amount of money as <see langword="ulong"/>, <see langword="uint"/>, <see langword="ushort"/>
    /// or <see cref="byte"/>.</param>
    /// <param name="currency">A ISO 4217 Currency code, like EUR or USD.</param>
    /// <remarks>The integral types are implicitly converted to long and the result evaluates to decimal. Therefore you can
    /// initialize a Money object using an integer literal, without the suffix, as follows:
    /// <code>Money money = new Money(10, "EUR");</code></remarks>
    public Amount(ulong value, string currency)
        : this((decimal)value, Currency.FromCode(currency))
    {
    }

    /// <summary>Initializes a new instance of the <see cref="Amount"/> struct.</summary>
    /// <param name="value">The Amount of money as <see langword="ulong"/>, <see langword="uint"/>, <see langword="ushort"/>
    /// or <see cref="byte"/>.</param>
    /// <param name="currency">The Currency of the monetry instance.</param>
    /// <remarks>The integral types are implicitly converted to long and the result evaluates to decimal. Therefore you can
    /// initialize a Money object using an integer literal, without the suffix, as follows:
    /// <code>Money money = new Money(10, "EUR");</code></remarks>
    public Amount(ulong value, Currency currency)
        : this((decimal)value, currency)
    {
    }

    /// <summary>The amount, rounded to the currency's minor unit.</summary>
    public decimal Value { get; }

    /// <summary>The <see cref="Money.Currency"/> the amount is in.</summary>
    public Currency Currency { get; }

    /// <summary>Deconstructs the amount into its value and currency.</summary>
    public void Deconstruct(out decimal amount, out Currency currency)
    {
        amount = Value;
        currency = Currency;
    }

    private static decimal Round(decimal amount, Currency currency, MidpointRounding rounding) =>
        currency.DecimalDigits switch
        {
            DefaultCurrencies.NotApplicable => Math.Round(amount),
            DefaultCurrencies.Z07 => Math.Round(amount / 0.2m, 0, rounding) * 0.2m, // divided into five subunits rather than by a power of ten
            _ => Math.Round(amount, (int)currency.DecimalDigits, rounding),
        };

    private static void AssertIsSameCurrency(Amount left, Amount right)
    {
        if (left.Currency != right.Currency)
        {
            throw new InvalidCurrencyException(left.Currency, right.Currency);
        }
    }
}
