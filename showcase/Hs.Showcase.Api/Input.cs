using System.Globalization;
using Hs.Core.Money;

namespace Hs.Showcase.Api;

/// <summary>The result of reading one query parameter: its value, or why it couldn't be read.</summary>
internal readonly record struct Parsed<T>(T? Value, string? Error)
{
    public static Parsed<T> Ok(T value) => new(value, null);

    public static Parsed<T> Fail(string error) => new(default, error);
}

/// <summary>Reads the API's query parameters. Pure: each returns a value or an error, never throws.</summary>
internal static class Input
{
    public const int MaxShares = 100;

    public static Parsed<decimal> Number(string? text) =>
        decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var number)
            ? Parsed<decimal>.Ok(number)
            : Parsed<decimal>.Fail($"'{text}' isn't a number in the range of a decimal, e.g. 1234.56.");

    public static Parsed<CultureInfo> Culture(string? name)
    {
        try
        {
            return Parsed<CultureInfo>.Ok(CultureInfo.GetCultureInfo(name ?? Catalogue.DefaultCulture, predefinedOnly: true));
        }
        catch (CultureNotFoundException)
        {
            return Parsed<CultureInfo>.Fail($"'{name}' isn't a known culture, e.g. en-AU.");
        }
    }

    public static Parsed<Currency> Currency(string? code) =>
        Hs.Core.Money.Currency.GetAllCurrencies()
            .Where(currency => string.Equals(currency.Code, code?.Trim(), StringComparison.OrdinalIgnoreCase))
            .Select(Parsed<Currency>.Ok)
            .DefaultIfEmpty(Parsed<Currency>.Fail($"'{code}' isn't a known currency code, e.g. EUR."))
            .First();

    public static Parsed<int> Shares(int? shareCount) =>
        shareCount is >= 2 and <= MaxShares
            ? Parsed<int>.Ok(shareCount.Value)
            : Parsed<int>.Fail($"Shares must be from 2 to {MaxShares}.");

    /// <summary>The errors among <paramref name="checks"/>, by parameter name, as a validation problem wants them.</summary>
    public static Dictionary<string, string[]> Errors(params (string Name, string? Error)[] checks) =>
        checks.Where(check => check.Error is not null).ToDictionary(check => check.Name, check => new[] { check.Error! });
}
