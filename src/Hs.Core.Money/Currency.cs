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

    /// <summary>The generic currency sign (¤), used when a currency has no sign of its own.</summary>
    /// <remarks>See https://en.wikipedia.org/wiki/Currency_sign_(typography).</remarks>
    public static string GenericCurrencySign => "¤";

    internal static readonly CurrencyRegistry Registry = new();

    /// <summary>Initializes a new currency. Missing names and numbers become empty, a missing symbol becomes <see cref="GenericCurrencySign"/>.</summary>
    /// <exception cref="ArgumentException"><paramref name="code"/> or <paramref name="namespace"/> is null or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="decimalDigits"/> is negative (other than "not applicable").</exception>
    internal Currency(
        string code,
        string? number,
        double decimalDigits,
        string? englishName,
        string? symbol,
        string @namespace = IsoNamespace,
        DateTime? validTo = null,
        DateTime? validFrom = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(@namespace);
        if (decimalDigits < 0 && decimalDigits != DefaultCurrencies.NotApplicable)
        {
            throw new ArgumentOutOfRangeException(nameof(decimalDigits), "DecimalDigits must be greater than or equal to zero.");
        }

        Code = code;
        Number = number ?? string.Empty;
        DecimalDigits = decimalDigits;
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

    /// <summary>The number of digits after the decimal separator.</summary>
    /// <remarks>
    /// <para>
    /// For example, the default number of fraction digits for the US Dollar and Euro is 2, while for the Japanese Yen it's 0.
    /// In the case of pseudo-currencies, such as Gold or IMF Special Drawing Rights, -1 is returned.
    /// </para>
    /// <para>
    /// The Malagasy ariary and the Mauritanian ouguiya are technically divided into five subunits (the iraimbilanja and
    /// khoum respectively), rather than by a power of ten. To represent this in decimal: 5 is 10 to the power of log(5) = 0.69897...
    /// </para>
    /// </remarks>
    public double DecimalDigits { get; }

    /// <summary>The first day the currency is valid, if it has one.</summary>
    public DateTime? ValidFrom { get; }

    /// <summary>The last day the currency is valid, if it has one.</summary>
    public DateTime? ValidTo { get; }

    /// <summary>The major currency unit, always 1.</summary>
    public decimal MajorUnit => 1;

    /// <summary>The smallest unit of the currency, e.g. 0.01 for the euro.</summary>
    public decimal MinorUnit
        => DecimalDigits == DefaultCurrencies.NotApplicable ? MajorUnit : new decimal(1 / Math.Pow(10, DecimalDigits));

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
        Registry.TryGet(code, @namespace, out var currency)
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
    public static IEnumerable<Currency> GetAllCurrencies() => Registry.GetAllCurrencies();

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
}
