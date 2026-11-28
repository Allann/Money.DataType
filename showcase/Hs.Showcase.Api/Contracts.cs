namespace Hs.Showcase.Api;

/// <summary>A language the showcase offers, and the culture that selects it.</summary>
/// <param name="Culture">The culture name, e.g. "nl-NL".</param>
/// <param name="Name">The language's English name, e.g. "Dutch".</param>
/// <param name="NativeName">The language's own name for itself, e.g. "Nederlands".</param>
public sealed record LanguageOption(string Culture, string Name, string NativeName);

/// <summary>A currency, as the showcase presents it.</summary>
/// <param name="Code">The ISO 4217 currency code, e.g. "EUR".</param>
/// <param name="Name">The currency's English name, e.g. "Euro".</param>
/// <param name="Symbol">The currency symbol, e.g. "$" or "¥".</param>
/// <param name="DecimalDigits">The number of fractional digits used by the currency.</param>
/// <param name="MinorUnit">The smallest amount of the currency, e.g. 0.01 for the euro or 1 for the yen.</param>
public sealed record CurrencyOption(string Code, string Name, string Symbol, int DecimalDigits, decimal MinorUnit);

/// <summary>A number and its words.</summary>
/// <param name="Number">The number, formatted with invariant culture, e.g. "1234.5".</param>
/// <param name="Words">The number written in the selected language.</param>
public sealed record NumberInWords(string Number, string Words);

/// <summary>An amount of money in figures and in words.</summary>
/// <param name="Value">The amount after rounding to the currency's minor unit, invariant-formatted, e.g. "1235" for 1234.56 JPY.</param>
/// <param name="Currency">The ISO 4217 currency code, e.g. "JPY".</param>
/// <param name="Figures">The amount formatted for the culture, e.g. "¥1,235".</param>
/// <param name="Words">The amount in words, in the culture's language.</param>
public sealed record AmountInWords(string Value, string Currency, string Figures, string Words);

/// <summary>An amount divided into shares that add up to it exactly.</summary>
/// <param name="Total">The rounded total, invariant-formatted.</param>
/// <param name="Currency">The ISO 4217 currency code.</param>
/// <param name="Shares">Each share, invariant-formatted.</param>
public sealed record Division(string Total, string Currency, IReadOnlyList<string> Shares);
