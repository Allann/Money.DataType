# 5. Number words: each language owns its grammar

**Status:** Accepted

## Context

`Hs.Core` writes numbers out in words. The obvious design is one algorithm plus a word list per language,
but languages differ in structure, not just vocabulary. English says "twenty-one" and "one hundred and five"
and uses the short scale (billion = 10⁹). Dutch says "eenentwintig" (one-and-twenty, as one word),
"honderdvijf", writes numbers up to a thousand as one word with a space after "duizend", and uses the long
scale (miljard = 10⁹).

## Decision

The language-independent work is shared: the sign, splitting off the fraction, and splitting the whole number
into three-digit groups (`Triplet`). Each language implements a small internal `ILanguage` that turns those
groups into words with its own rules. The language comes from the `CultureInfo`, falling back to English.
Money wording (unit names, plurals, "and"/"en") lives in `Hs.Core.Money`, so `Hs.Core` knows nothing about money.

## Consequences

- Adding a language is one class and one dictionary entry, and can't break the others.
- Some logic looks duplicated between languages (both have a `BelowHundred`); that's deliberate, because the
  rules really are different.
- Unsupported cultures get English rather than an error, the same way .NET resources fall back to the neutral language.
