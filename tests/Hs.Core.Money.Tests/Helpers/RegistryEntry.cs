namespace Hs.Core.Money.Tests.Helpers;

/// <summary>
/// Puts a currency registry entry back as it was when disposed, so a test that registers or unregisters
/// a currency doesn't leak into the tests running alongside it.
/// </summary>
internal sealed class RegistryEntry(string code, string @namespace = "ISO-4217") : IDisposable
{
    private readonly Currency? _original = CurrencyRegistry.TryGet(code, @namespace, out var currency) ? currency : null;

    public void Dispose()
    {
        CurrencyRegistry.TryRemove(code, @namespace, out _);
        if (_original is { } original)
        {
            CurrencyRegistry.TryAdd(code, @namespace, original);
        }
    }
}
