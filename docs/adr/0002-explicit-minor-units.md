# 2. Model a currency's minor unit explicitly

**Status:** Accepted

## Context

Most currencies divide into hundredths, but not all: the yen has no minor unit, the Bahraini dinar has
thousandths, gold has none at all ("N.A." in ISO 4217), and the Malagasy ariary and Mauritanian ouguiya
divide into *fifths*. `Currency.DecimalDigits` used to be a `double` that encoded the last two cases as magic
values (-1 and log₁₀5 ≈ 0.699), and every caller had to special-case them. Two didn't: formatting gold
threw, and `SafeDivide` threw for gold and rounded the ariary to whole units.

## Decision

`DecimalDigits` is an `int` — the digits to display — and `MinorUnit` is an explicit `decimal`: 0.01, 1,
0.001 or 0.2. `Currency.Round` is the single rounding rule; `Amount`, `SafeDivide` and formatting all use it.
Currencies with no minor unit have 0 digits and a minor unit of 1.

## Consequences

- No caller needs to know about the special cases, and the two bugs are gone (with regression tests).
- ISO 4217 says the ariary and ouguiya have 2 minor units; we deliberately differ, and the ISO conformance
  test lists them as known exceptions.
- `DecimalDigits` changed type, which is a breaking change for anyone who read it as a `double`.
