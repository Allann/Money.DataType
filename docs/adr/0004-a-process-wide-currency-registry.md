# 4. Keep a process-wide, thread-safe currency registry

**Status:** Accepted

## Context

`Currency.FromCode("EUR")` needs somewhere to look currencies up, and `CurrencyBuilder.Register` lets
applications add their own (a loyalty-points currency, say). The registry was a static `Dictionary`
mutated without synchronisation; under the parallel test run, a lookup during a registration threw
"Collection was modified". The alternative to a static registry is injecting one wherever an amount is
created or parsed.

## Decision

Keep a single process-wide registry — currencies are reference data, like `CultureInfo` — but make it
a `ConcurrentDictionary` keyed by (namespace, code). Lookup by code alone prefers current ISO 4217, then
historic ISO 4217, then custom namespaces.

## Consequences

- `FromCode` stays a one-liner for callers, and is lock-free and allocation-free (about 14 ns).
- Registration is global state. Tests that register or unregister currencies must restore the registry
  afterwards (`RegistryEntry` in the tests does this), or they leak into tests running alongside them.
- An application that needs *different* currency sets in different contexts can't have them; if that ever
  matters, the registry would become an injected service with the static one as its default.
