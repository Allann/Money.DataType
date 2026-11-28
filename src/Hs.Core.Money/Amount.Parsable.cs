using System.Globalization;

namespace Hs.Core.Money;

/// <summary>Parsing an <see cref="Amount"/> from text such as "€ 765,43" or "USD 1,234.56".</summary>
// This partial keeps parsing members grouped in their own file.
public readonly partial record struct Amount // NOSONAR
{
    /// <summary>The result of a failed TryParse: zero in XXX, ISO 4217's "no currency".</summary>
    private static readonly Amount None = new(0, Currency.FromCode(Currency.NoCurrency));

    /// <summary>
    /// Parses an amount in the current culture's number format. The currency is the one whose symbol or code is written
    /// in <paramref name="value"/>, or the current culture's when none is.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null or whitespace.</exception>
    /// <exception cref="FormatException"><paramref name="value"/> isn't a number, or its symbol is unknown or matches several currencies.</exception>
    /// <exception cref="OverflowException"><paramref name="value"/> is outside the range of <see cref="decimal"/>.</exception>
    public static Amount Parse(string? value)
    {
        ThrowIfNullOrWhiteSpace(value);
        var (currency, error) = CurrencyIn(value);
        return currency is { } found ? Parse(value, found) : throw new FormatException(error);
    }

    /// <summary>Parses an amount in <paramref name="currency"/>, in the current culture's number format.</summary>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null or whitespace.</exception>
    /// <exception cref="FormatException"><paramref name="value"/> isn't a number in that format.</exception>
    /// <exception cref="OverflowException"><paramref name="value"/> is outside the range of <see cref="decimal"/>.</exception>
    public static Amount Parse(string value, Currency currency) => Parse(value, NumberStyles.Currency, null, currency);

    /// <summary>Parses an amount in <paramref name="currency"/>, with the given number style and format provider.</summary>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null or whitespace.</exception>
    /// <exception cref="FormatException"><paramref name="value"/> isn't a number in that format.</exception>
    /// <exception cref="OverflowException"><paramref name="value"/> is outside the range of <see cref="decimal"/>.</exception>
    public static Amount Parse(string value, NumberStyles style, IFormatProvider? provider, Currency currency)
    {
        ThrowIfNullOrWhiteSpace(value);
        return new Amount(decimal.Parse(value, style, CurrencyFormat(currency, provider)), currency);
    }

    /// <summary>Tries to parse an amount as <see cref="Parse(string?)"/> does; on failure the result is zero XXX.</summary>
    public static bool TryParse(string? value, out Amount result)
    {
        result = None;
        return !string.IsNullOrWhiteSpace(value)
            && CurrencyIn(value).Currency is { } currency
            && TryParse(value, currency, out result);
    }

    /// <summary>Tries to parse an amount in <paramref name="currency"/>; on failure the result is zero XXX.</summary>
    public static bool TryParse(string? value, Currency currency, out Amount result) =>
        TryParse(value, NumberStyles.Currency, null, currency, out result);

    /// <summary>Tries to parse an amount in <paramref name="currency"/> with the given number style and format provider; on failure the result is zero XXX.</summary>
    public static bool TryParse(string? value, NumberStyles style, IFormatProvider? provider, Currency currency, out Amount result)
    {
        var parsed = decimal.TryParse(value, style, CurrencyFormat(currency, provider), out var amount);
        result = parsed ? new Amount(amount, currency) : None;
        return parsed;
    }

    /// <summary>
    /// The currency written in <paramref name="value"/>: the current culture's when no symbol or code is written (or it's the
    /// current culture's own), otherwise the one currency with that symbol or code. On failure, the reason why.
    /// </summary>
    private static (Currency? Currency, string? Error) CurrencyIn(string value)
    {
        var current = Currency.CurrentCurrency;
        var written = string.Concat(value.Where(IsCurrencyCharacter));
        if (written.Length == 0 || written == current.Symbol || written == current.Code)
        {
            return (current, null);
        }

        return Currency.GetAllCurrencies().Where(c => c.Symbol == written || c.Code == written).Take(2).ToArray() switch
        {
            [var only] => (only, null),
            [] => (null, $"{written} is an unknown currency sign or code."),
            _ => (null, $"Currency sign {written} matches with multiple known currencies. Specify currency or culture explicit."),
        };
    }

    /// <summary>Throws <see cref="ArgumentNullException"/> for empty text too, as Parse always has.</summary>
    private static void ThrowIfNullOrWhiteSpace([System.Diagnostics.CodeAnalysis.NotNull] string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentNullException(nameof(value));
        }
    }

    private static bool IsCurrencyCharacter(char character) =>
        !char.IsDigit(character) && !char.IsWhiteSpace(character) && character is not ('.' or ',' or '(' or ')' or '+' or '-');
}
