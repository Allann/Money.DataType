using System.Globalization;

namespace Hs.Core.Money;

/// <summary>The rate at which money in a base currency converts into a quote currency, e.g. EUR/USD 1.2591.</summary>
/// <remarks>See https://en.wikipedia.org/wiki/Exchange_rate.</remarks>
public readonly record struct ExchangeRate
{
    /// <summary>The result of a failed <see cref="TryParse"/>: XXX/XXX 0, where XXX is ISO 4217's "no currency".</summary>
    private static readonly ExchangeRate None = new(Currency.FromCode(Currency.NoCurrency));

    /// <summary>Initializes a new exchange rate.</summary>
    /// <exception cref="ArgumentNullException"><paramref name="baseCurrency"/> or <paramref name="quoteCurrency"/> is the default currency.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="rate"/> is negative.</exception>
    /// <exception cref="ArgumentException">The base and quote currency are the same.</exception>
    public ExchangeRate(Currency baseCurrency, Currency quoteCurrency, decimal rate)
    {
        if (baseCurrency == default)
        {
            throw new ArgumentNullException(nameof(baseCurrency));
        }

        if (quoteCurrency == default)
        {
            throw new ArgumentNullException(nameof(quoteCurrency));
        }

        ArgumentOutOfRangeException.ThrowIfNegative(rate);
        if (baseCurrency == quoteCurrency)
        {
            throw new ArgumentException("The base and quote currency can't be equal.", nameof(quoteCurrency));
        }

        BaseCurrency = baseCurrency;
        QuoteCurrency = quoteCurrency;
        Value = rate;
    }

    /// <summary>Initializes a new exchange rate, rounding <paramref name="rate"/> to six decimals.</summary>
    public ExchangeRate(Currency baseCurrency, Currency quoteCurrency, double rate)
        : this(baseCurrency, quoteCurrency, rate, 6)
    {
    }

    /// <summary>Initializes a new exchange rate, rounding <paramref name="rate"/> to <paramref name="numberOfDecimals"/>.</summary>
    public ExchangeRate(Currency baseCurrency, Currency quoteCurrency, double rate, int numberOfDecimals)
        : this(baseCurrency, quoteCurrency, Math.Round((decimal)rate, numberOfDecimals))
    {
    }

    /// <summary>Initializes a new exchange rate between two ISO 4217 currency codes.</summary>
    public ExchangeRate(string baseCode, string quoteCode, decimal rate)
        : this(Currency.FromCode(baseCode), Currency.FromCode(quoteCode), rate)
    {
    }

    /// <summary>Initializes a new exchange rate between two ISO 4217 currency codes, rounding <paramref name="rate"/> to six decimals.</summary>
    public ExchangeRate(string baseCode, string quoteCode, double rate)
        : this(Currency.FromCode(baseCode), Currency.FromCode(quoteCode), rate, 6)
    {
    }

    /// <summary>Initializes a new exchange rate between two ISO 4217 currency codes, rounding <paramref name="rate"/> to <paramref name="numberOfDecimals"/>.</summary>
    public ExchangeRate(string baseCode, string quoteCode, double rate, int numberOfDecimals)
        : this(Currency.FromCode(baseCode), Currency.FromCode(quoteCode), rate, numberOfDecimals)
    {
    }

    private ExchangeRate(Currency none)
    {
        BaseCurrency = none;
        QuoteCurrency = none;
    }

    /// <summary>The currency being converted from.</summary>
    public Currency BaseCurrency { get; }

    /// <summary>The currency being converted to.</summary>
    public Currency QuoteCurrency { get; }

    /// <summary>How many units of <see cref="QuoteCurrency"/> one unit of <see cref="BaseCurrency"/> buys.</summary>
    public decimal Value { get; }

    /// <summary>Parses a currency pair and rate such as "EUR/USD 1.2591" or "EURUSD 1.2591", using the current culture's number format.</summary>
    /// <exception cref="ArgumentNullException"><paramref name="rate"/> is null.</exception>
    /// <exception cref="FormatException"><paramref name="rate"/> isn't a valid currency pair and rate.</exception>
    public static ExchangeRate Parse(string rate)
    {
        ArgumentNullException.ThrowIfNull(rate);
        return TryParse(rate, out var exchangeRate)
            ? exchangeRate
            : throw new FormatException("Rate is not in the correct format. Currencies are the same or the rate is not a number.");
    }

    /// <summary>Tries to parse a currency pair and rate such as "EUR/USD 1.2591"; on failure the result is XXX/XXX 0.</summary>
    public static bool TryParse(string? rate, out ExchangeRate result)
    {
        result = None;
        var text = rate?.Trim();
        if (text is not { Length: >= 7 })
        {
            return false;
        }

        var quoteStart = text[3] == '/' ? 4 : 3;
        if (text.Length < quoteStart + 3
            || !CurrencyRegistry.TryGet(text[..3], out var baseCurrency)
            || !CurrencyRegistry.TryGet(text.Substring(quoteStart, 3), out var quoteCurrency)
            || baseCurrency == quoteCurrency
            || !decimal.TryParse(text[(quoteStart + 3)..], NumberStyles.Number, NumberFormatInfo.CurrentInfo, out var value)
            || value < 0)
        {
            return false;
        }

        result = new ExchangeRate(baseCurrency, quoteCurrency, value);
        return true;
    }

    /// <summary>Deconstructs the exchange rate into its currencies and rate.</summary>
    public void Deconstruct(out Currency baseCurrency, out Currency quoteCurrency, out decimal rate)
    {
        baseCurrency = BaseCurrency;
        quoteCurrency = QuoteCurrency;
        rate = Value;
    }

    /// <summary>Converts <paramref name="money"/> from either currency of the pair into the other.</summary>
    /// <exception cref="ArgumentException"><paramref name="money"/> is in neither currency of the pair.</exception>
    public Amount Convert(Amount money)
    {
        if (money.Currency == BaseCurrency)
        {
            return new Amount(money.Value * Value, QuoteCurrency);
        }

        return money.Currency == QuoteCurrency
            ? new Amount(money.Value / Value, BaseCurrency)
            : throw new ArgumentException("Money should have the same currency as the base currency or the quote currency.", nameof(money));
    }

    /// <summary>The pair and rate, e.g. "EUR/USD 1.2591".</summary>
    public override string ToString() => $"{BaseCurrency.Code}/{QuoteCurrency.Code} {Value}";
}
