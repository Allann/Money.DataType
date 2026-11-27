using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Hs.Core.Money.Wording
{
    /// <summary>The name of a currency unit, singular and plural, e.g. "dollar" and "dollars".</summary>
    internal readonly record struct UnitName(string One, string Many)
    {
        public string For(decimal count) => count == 1 ? One : Many;
    }

    /// <summary>The names of a currency's major and minor units, e.g. dollars and cents.</summary>
    internal readonly record struct CurrencyUnits(UnitName Major, UnitName Minor);

    /// <summary>How one language words money: its connectives and the unit names it knows, keyed by ISO 4217 code.</summary>
    internal sealed record MoneyLanguage(string Minus, string And, IReadOnlyDictionary<string, CurrencyUnits> Units)
    {
        // Declared first: static fields initialise in order, and the languages below use it.
        private static readonly string[] DollarCurrencies = ["AUD", "CAD", "HKD", "NZD", "SGD", "USD"];

        private static readonly MoneyLanguage English =new("minus", "and", WithDollars(
            new("dollar", "dollars"), new("cent", "cents"),
            new Dictionary<string, CurrencyUnits>
            {
                ["EUR"] = new(new("euro", "euros"), new("cent", "cents")),
                ["GBP"] = new(new("pound", "pounds"), new("penny", "pence")),
                ["JPY"] = new(new("yen", "yen"), new("sen", "sen")),
            }));

        // Dutch doesn't pluralise money units: "vijf dollar", "twintig cent".
        private static readonly MoneyLanguage Dutch = new("min", "en", WithDollars(
            new("dollar", "dollar"), new("cent", "cent"),
            new Dictionary<string, CurrencyUnits>
            {
                ["EUR"] = new(new("euro", "euro"), new("cent", "cent")),
                ["GBP"] = new(new("pond", "pond"), new("penny", "pence")),
                ["JPY"] = new(new("yen", "yen"), new("sen", "sen")),
            }));

        private static readonly Dictionary<string, MoneyLanguage> Languages = new()
        {
            ["en"] = English,
            ["nl"] = Dutch,
        };

        /// <summary>The language of <paramref name="culture"/>, falling back to English as <see cref="NumberToWords"/> does.</summary>
        public static MoneyLanguage Of(CultureInfo culture) =>
            Languages.GetValueOrDefault(culture.TwoLetterISOLanguageName, English);

        private static Dictionary<string, CurrencyUnits> WithDollars(UnitName dollar, UnitName cent, Dictionary<string, CurrencyUnits> others) =>
            DollarCurrencies
                .Select(code => KeyValuePair.Create(code, new CurrencyUnits(dollar, cent)))
                .Concat(others)
                .ToDictionary();
    }
}
