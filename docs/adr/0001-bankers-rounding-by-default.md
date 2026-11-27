# 1. Round amounts with banker's rounding by default

**Status:** Accepted

## Context

An `Amount` is stored to its currency's minor unit, so `new Amount(12.345m, USD)` has to round. The choice
of rounding mode decides who gets the half cent, millions of times over: rounding half away from zero
(what most people learn at school) drifts upwards on average; rounding half to even does not.

## Decision

Constructors round with `MidpointRounding.ToEven` (banker's rounding, IEEE 754 §4) unless the caller passes
another mode. `SafeDivide` uses the same default for its shares.

## Consequences

- Sums of many rounded amounts don't drift: 12.345 → 12.34 and 12.355 → 12.36.
- It surprises people who expect 12.345 → 12.35. The constructor documents the mode, and every constructor
  and `SafeDivide` overload accepts a `MidpointRounding` for callers whose rules differ (tax authorities
  often specify away-from-zero).
- `ToString("C")` formats an already-rounded value, so what's displayed always matches what's stored.
