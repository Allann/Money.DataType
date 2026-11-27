using System.Collections.Concurrent;

namespace Hs.Core.Money;

/// <summary>
/// The currencies known to the process: the ISO 4217 defaults plus any registered with <see cref="CurrencyBuilder"/>.
/// Thread-safe, so currencies can be registered while others are being looked up.
/// </summary>
internal static class CurrencyRegistry
{
    internal const string HistoricNamespace = "ISO-4217-HISTORIC";

    private static readonly ConcurrentDictionary<(string Namespace, string Code), Currency> Currencies =
        new(DefaultCurrencies.All.Select(c => KeyValuePair.Create((c.Namespace, c.Code), c)));

    /// <summary>Finds a currency by code alone: a current ISO 4217 currency first, then a historic one, then a custom one.</summary>
    /// <exception cref="ArgumentNullException"><paramref name="code"/> is null or whitespace.</exception>
    public static bool TryGet(string code, out Currency currency)
    {
        ThrowIfNullOrWhiteSpace(code);
        return Currencies.TryGetValue((Currency.IsoNamespace, code), out currency)
            || Currencies.TryGetValue((HistoricNamespace, code), out currency)
            || TryGetCustom(code, out currency);
    }

    /// <summary>Finds a currency by code within a namespace.</summary>
    /// <exception cref="ArgumentNullException"><paramref name="code"/> or <paramref name="namespace"/> is null or whitespace.</exception>
    public static bool TryGet(string code, string @namespace, out Currency currency)
    {
        ThrowIfNullOrWhiteSpace(code);
        ThrowIfNullOrWhiteSpace(@namespace);
        return Currencies.TryGetValue((@namespace, code), out currency);
    }

    /// <summary>Registers a currency; false if one with the same code and namespace already is.</summary>
    /// <exception cref="ArgumentNullException"><paramref name="code"/> or <paramref name="namespace"/> is null or whitespace.</exception>
    public static bool TryAdd(string code, string @namespace, Currency currency)
    {
        ThrowIfNullOrWhiteSpace(code);
        ThrowIfNullOrWhiteSpace(@namespace);
        return Currencies.TryAdd((@namespace, code), currency);
    }

    /// <summary>Unregisters a currency; false if it wasn't registered.</summary>
    /// <exception cref="ArgumentNullException"><paramref name="code"/> or <paramref name="namespace"/> is null or whitespace.</exception>
    public static bool TryRemove(string code, string @namespace, out Currency currency)
    {
        ThrowIfNullOrWhiteSpace(code);
        ThrowIfNullOrWhiteSpace(@namespace);
        return Currencies.TryRemove((@namespace, code), out currency);
    }

    /// <summary>All registered currencies, ordered by namespace (ISO 4217 first) and then code.</summary>
    public static IEnumerable<Currency> GetAllCurrencies() =>
        Currencies.Values
            .OrderBy(c => c.Namespace switch { Currency.IsoNamespace => 0, HistoricNamespace => 1, _ => 2 })
            .ThenBy(c => c.Namespace, StringComparer.Ordinal)
            .ThenBy(c => c.Code, StringComparer.Ordinal)
            .ToArray();

    // Separate from TryGet so the closure over `code` is only allocated on this rare path, not on every lookup.
    private static bool TryGetCustom(string code, out Currency currency)
    {
        currency = Currencies.Values.Where(c => c.Code == code).OrderBy(c => c.Namespace, StringComparer.Ordinal).FirstOrDefault();
        return currency != default;
    }

    private static void ThrowIfNullOrWhiteSpace(string? value, [System.Runtime.CompilerServices.CallerArgumentExpression(nameof(value))] string? name = null)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentNullException(name);
        }
    }
}
