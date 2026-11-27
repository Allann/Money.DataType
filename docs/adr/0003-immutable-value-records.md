# 3. Money types are immutable value records, serialised with System.Text.Json only

**Status:** Accepted

## Context

`Amount`, `Currency` and `ExchangeRate` were mutable structs (private and internal setters, an XML
`ReadXml` that assigned `this`), with hand-written `Equals`, `GetHashCode` and `==`. `Currency` also
implemented `IXmlSerializable` and `ISerializable` for `BinaryFormatter`, which .NET has made obsolete
and unsafe.

## Decision

All three are `readonly record struct`s with get-only properties; the compiler generates equality.
System.Text.Json, through `[JsonConverter]` attributes, is the only supported serialiser.

## Consequences

- Values can't change after construction, so they're safe to share, cache and use as dictionary keys.
- Equality can't drift out of sync with the fields when one is added.
- Callers using `XmlSerializer` or `BinaryFormatter` with these types must move to JSON, or write their own
  surrogate. That's a breaking change, accepted because `BinaryFormatter` is a security risk.
