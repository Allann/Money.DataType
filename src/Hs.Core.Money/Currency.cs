using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json.Serialization;

namespace Hs.Core.Money;

/// <summary>A unit of exchange; the currency of an <see cref="Amount" />, e.g. the euro or the US dollar.</summary>
/// <remarks>
/// Currencies are identified by a <see cref="Code"/> within a <see cref="Namespace"/>; the defaults are the ISO 4217
/// currencies, current and historic. Use <see cref="CurrencyBuilder"/> to register custom ones.
/// </remarks>
[DebuggerDisplay("{" + nameof(Code) + "}")]
[TypeConverter(typeof(CurrencyTypeConverter))]
[JsonConverter(typeof(CurrencyJsonConverter))]
public readonly record struct Currency
{
    internal const string IsoNamespace = "ISO-4217";

    /// <summary>The most digits a <see cref="decimal"/> can hold after the decimal point.</summary>
    internal const int MaxDecimalDigits = 28;

    /// <summary>The generic currency sign (¤), used when a currency has no sign of its own.</summary>
    /// <remarks>See https://en.wikipedia.org/wiki/Currency_sign_(typography).</remarks>
    public static string GenericCurrencySign => "¤";

    /// <summary>
    /// Initializes a new currency. Missing names and numbers become empty, a missing symbol becomes <see cref="GenericCurrencySign"/>.
    /// The minor unit defaults to 10 to the power of minus the decimal digits (0.01 for two digits); pass it only when it isn't
    /// a power of ten, e.g. 0.2 for a currency divided into fifths.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="code"/> or <paramref name="namespace"/> is null or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="decimalDigits"/> is outside 0-28, or <paramref name="minorUnit"/> isn't positive.</exception>
    internal Currency(
        string code,
        string? number,
        int decimalDigits,
        string? englishName,
        string? symbol,
        string @namespace = IsoNamespace,
        DateTime? validTo = null,
        DateTime? validFrom = null,
        decimal? minorUnit = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(@namespace);
        ArgumentOutOfRangeException.ThrowIfNegative(decimalDigits);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(decimalDigits, MaxDecimalDigits);
        if (minorUnit is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minorUnit), "The minor unit must be positive.");
        }

        Code = code;
        Number = number ?? string.Empty;
        DecimalDigits = decimalDigits;
        MinorUnit = minorUnit ?? PowerOfTen(-decimalDigits);
        EnglishName = englishName ?? string.Empty;
        Symbol = symbol ?? GenericCurrencySign;
        Namespace = @namespace;
        ValidTo = validTo;
        ValidFrom = validFrom;
    }

    /// <summary>The currency of the current culture's region.</summary>
    public static Currency CurrentCurrency
    {
        get
        {
            var culture = CultureInfo.CurrentCulture;
            var region = culture.IsNeutralCulture ? RegionInfo.CurrentRegion : new RegionInfo(culture.Name);
            return FromCode(region.ISOCurrencySymbol, IsoNamespace);
        }
    }

    /// <summary>The currency symbol, e.g. "€" or "$".</summary>
    public string Symbol { get; }

    /// <summary>The English name of the currency, e.g. "Euro".</summary>
    public string EnglishName { get; }

    /// <summary>The three-character currency code, e.g. "EUR".</summary>
    public string Code { get; }

    /// <summary>The numeric ISO 4217 currency number, e.g. "978"; empty when the currency has none.</summary>
    public string Number { get; }

    /// <summary>The namespace the <see cref="Code"/> belongs to, e.g. "ISO-4217".</summary>
    public string Namespace { get; }

    /// <summary>The number of digits shown after the decimal separator, e.g. 2 for the euro and 0 for the yen.</summary>
    /// <remarks>
    /// Pseudo-currencies such as gold, which ISO 4217 lists without minor units ("N.A."), have 0.
    /// The Malagasy ariary and Mauritanian ouguiya are divided into fifths, so have 1 digit and a <see cref="MinorUnit"/> of 0.2.
    /// </remarks>
    public int DecimalDigits { get; }

    /// <summary>The first day the currency is valid, if it has one.</summary>
    public DateTime? ValidFrom { get; }

    /// <summary>The last day the currency is valid, if it has one.</summary>
    public DateTime? ValidTo { get; }

    /// <summary>The major currency unit, always 1.</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Part of a currency's shape, alongside MinorUnit; static would break callers.")]
    public decimal MajorUnit => 1;

    /// <summary>The smallest unit of the currency, e.g. 0.01 for the euro, 1 for the yen and 0.2 for the Malagasy ariary.</summary>
    public decimal MinorUnit { get; }

    /// <summary>Whether the currency is valid today.</summary>
    public bool IsValid => IsValidOn(DateTime.Today);

    /// <summary>Gets an ISO 4217 currency by its code, preferring current currencies over historic ones.</summary>
    /// <param name="code">An ISO 4217 currency code, like EUR or USD.</param>
    /// <exception cref="ArgumentNullException"><paramref name="code"/> is null or whitespace.</exception>
    /// <exception cref="ArgumentException"><paramref name="code"/> is not a known currency code.</exception>
    public static Currency FromCode(string code) =>
        CurrencyRegistry.TryGet(code, out var currency)
            ? currency
            : throw new ArgumentException($"{code} is an unknown currency code", nameof(code));

    /// <summary>Gets a currency by its code within a namespace.</summary>
    /// <param name="code">A currency code, like EUR or USD.</param>
    /// <param name="namespace">A namespace, like ISO-4217.</param>
    /// <exception cref="ArgumentNullException"><paramref name="code"/> or <paramref name="namespace"/> is null or whitespace.</exception>
    /// <exception cref="ArgumentException"><paramref name="code"/> is not known in <paramref name="namespace"/>.</exception>
    public static Currency FromCode(string code, string @namespace) =>
        CurrencyRegistry.TryGet(code, @namespace, out var currency)
            ? currency
            : throw new ArgumentException($"{code} is an unknown {@namespace} currency code", nameof(code));

    /// <summary>Gets the currency used in <paramref name="region"/>.</summary>
    /// <exception cref="ArgumentNullException"><paramref name="region"/> is null.</exception>
    public static Currency FromRegion(RegionInfo region)
    {
        ArgumentNullException.ThrowIfNull(region);
        return FromCode(region.ISOCurrencySymbol, IsoNamespace);
    }

    /// <summary>Gets the currency used in the region of <paramref name="culture"/>.</summary>
    /// <exception cref="ArgumentNullException"><paramref name="culture"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="culture"/> is neutral, so has no region.</exception>
    public static Currency FromCulture(CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(culture);
        return culture.IsNeutralCulture
            ? throw new ArgumentException($"Culture {culture.Name} is a neutral culture, from which no region information can be extracted.", nameof(culture))
            : FromRegion(culture.Name);
    }

    /// <summary>Gets the currency used in a region, given its ISO 3166 two-letter code or a specific culture name.</summary>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is null or whitespace.</exception>
    public static Currency FromRegion(string name) =>
        string.IsNullOrWhiteSpace(name) ? throw new ArgumentNullException(nameof(name)) : FromRegion(new RegionInfo(name));

    /// <summary>Gets all registered currencies.</summary>
    public static IEnumerable<Currency> GetAllCurrencies() => CurrencyRegistry.GetAllCurrencies();

    /// <summary>Deconstructs the currency into its code, number and symbol.</summary>
    public void Deconstruct(out string code, out string number, out string symbol)
    {
        code = Code;
        number = Number;
        symbol = Symbol;
    }

    /// <summary>Whether the currency is valid on <paramref name="date"/>.</summary>
    public bool IsValidOn(DateTime date) =>
        (!ValidFrom.HasValue || ValidFrom <= date) && (!ValidTo.HasValue || ValidTo >= date);

    /// <summary>The currency code, e.g. "EUR".</summary>
    public override string ToString() => Code;

    /// <summary>Rounds <paramref name="amount"/> to a whole number of <see cref="MinorUnit"/>s.</summary>
    internal decimal Round(decimal amount, MidpointRounding rounding) =>
        MinorUnit == PowerOfTen(-DecimalDigits)
            ? Math.Round(amount, DecimalDigits, rounding)
            : Math.Round(amount / MinorUnit, rounding) * MinorUnit;

    /// <summary>10 to the power of <paramref name="exponent"/> (0 or negative), exactly: <c>new decimal(1, 0, 0, false, 2)</c> is 0.01.</summary>
    private static decimal PowerOfTen(int exponent) => new(1, 0, 0, false, (byte)-exponent);
}
