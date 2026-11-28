using Hs.Core.Money;

namespace Hs.Showcase.Api;

/// <summary>What the showcase offers: the languages Hs.Core writes, and the currencies Hs.Core.Money can name in them.</summary>
internal static class Catalogue
{
    public const string DefaultCulture = "en-AU";

    public static readonly IReadOnlyList<LanguageOption> Languages =
    [
        new("en-AU", "English", "English"),
        new("nl-NL", "Dutch", "Nederlands"),
        new("ja-JP", "Japanese", "日本語"),
    ];

    /// <summary>The currencies whose units (dollars and cents, yen, ...) are named in every language, so a cheque reads naturally.</summary>
    public static readonly IReadOnlyList<CurrencyOption> ChequeCurrencies =
        [.. new[] { "AUD", "CAD", "EUR", "GBP", "HKD", "JPY", "NZD", "SGD", "USD" }.Select(code => Describe(Currency.FromCode(code)))];

    public static CurrencyOption Describe(Currency currency) =>
        new(currency.Code, currency.EnglishName, currency.Symbol, currency.DecimalDigits, currency.MinorUnit);
}
