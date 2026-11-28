# 5. Number words: each language owns its grammar

**Status:** Accepted

## Context

`Hs.Core` writes numbers out in words. The obvious design is one algorithm plus a word list per language,
but languages differ in structure, not just vocabulary. English says "twenty-one" and "one hundred and five"
and uses the short scale (billion = 10⁹). Dutch says "eenentwintig" (one-and-twenty, as one word),
"honderdvijf", writes numbers up to a thousand as one word with a space after "duizend", and uses the long
scale (miljard = 10⁹). Japanese groups digits in fours rather than threes (一万 = 10⁴, 一億 = 10⁸), leaves "one"
off 十, 百 and 千 but not 万, and doesn't space its words or join amounts with "and": "千二百三十四ドル五十六セント".

## Decision

Only the genuinely language-independent work is shared: splitting off the sign and the fraction, and a
`DigitGroup` helper that splits a whole number into groups of any size. Each language implements a small internal
`ILanguage` that words the whole number (choosing its own group size), the fraction digits and the sign, so it
also controls spacing. The language comes from the `CultureInfo`, falling back to English.
Money wording (unit names, plurals, "and"/"en"/nothing, spacing) lives in `Hs.Core.Money`, so `Hs.Core` knows nothing about money.

## Consequences

- Adding a language is one class and one dictionary entry, and can't break the others.
- Some logic looks duplicated between languages (English and Dutch both have a `BelowHundred`); that's deliberate,
  because the rules really are different.
- The first version shared three-digit grouping across languages. Adding Japanese showed that was a language
  rule in disguise, and moved it behind `ILanguage`: the cost of finding the seam one language late.
- Unsupported cultures get English rather than an error, the same way .NET resources fall back to the neutral language.
