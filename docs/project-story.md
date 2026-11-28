# From number words to a money showcase

## It started with a number

The story began with a feature requested during a technical interview: take a number and write it out in words. The
original exercise dates back to 1999. Years later, I returned to it with a different goal—not just to make it work,
but to rebuild it in a way that demonstrates how I approach software today.

That became **Hs.Core**, a small C# library that turns `decimal` values into words. The interview exercise did not
ask for multiple languages; I added them because I wanted to demonstrate that language can change the rules for
building words, not just the vocabulary. Dutch puts units before tens, while Japanese groups digits by
ten-thousands. If those rules were mixed into one large conversion algorithm, each new language would add branches
through code that should be common to every language.

Instead, the implementation follows a functional style: conversion is made from explicit inputs, has no mutable
state or side effects, and returns a value. The shared code handles what is common, such as the sign and fractional
digits, then delegates the language-dependent work through the `ILanguage` contract. The culture selects a language
implementation; that implementation supplies the grammar that shapes the result. In this way, the language can be
changed to change the outcome without changing most of the number-to-words algorithm.

The public API stays simple:

```csharp
21m.ToWords(enUS); // "twenty-one"
21m.ToWords(nlNL); // "eenentwintig"
21m.ToWords(jaJP); // "二十一"
```

The result is a `Words` value rather than an ordinary string. That reflects an early design choice: keep the input
as simple data, put the conversion in a focused operation, and give the result a type that says what it represents.
The `ToWords` extension keeps the operation natural to call without making a number responsible for languages or
wording rules. A `Words` value, in turn, cannot be casually confused with any other text.

This separation gives the data a stable, clear meaning while allowing the rules that transform it to evolve. It is
not a blanket rule that behaviour should always live elsewhere: behaviour that defines a value's validity belongs
close to that value. But writing a number in words is a transformation, and here a stateless operation with an
explicit culture is a useful boundary. That balance—keeping concepts explicit while placing change where it belongs—
became a theme in the libraries that followed.

## Giving the words a real domain

Next, I created **Hs.Core.Money** to show how Hs.Core could be used as a formatter in a richer domain. Money can be
written out too, but it brings its own rules: an amount has a currency, currencies have different smallest units,
and each language has its own names and grammar for those units.

Hs.Core.Money composes those responsibilities. It splits an amount into major and minor units, supplies the currency
names, and asks Hs.Core to write the numbers. Hs.Core remains independent of money; the money library depends on
the number-to-words feature, not the other way around. The same words feature is also available through the
`"W"` amount format.

```csharp
var price = new Amount(1234.5m, "EUR");
price.ToString("W", enUS); // "one thousand two hundred and thirty-four euros and fifty cents"
price.ToString("W", nlNL); // "duizend tweehonderdvierendertig euro en vijftig cent"
```

Once money became the example, it opened the door to a broader set of design techniques—especially ways to avoid
primitive obsession, where important domain concepts are represented by unstructured numbers or strings.

## Keeping one domain type organized by feature

Hs.Core.Money also gave me a way to keep data and its closely related behaviour together without putting every
operation in one large source file. `Amount` is one immutable `readonly partial record struct`: its main file
defines the value and construction rules, while companion files group the behaviour by feature. Arithmetic and
operators, comparison, conversion, formatting, parsing, and writing the amount in words each have a clear home.

This is a practical application of the single responsibility principle (SRP) at the source-file level. The files
have focused reasons to change, but they still describe the same domain value; the public API remains one `Amount`,
and its operations can share the type's invariants and implementation details. `partial` is an organizational
boundary, not a separate runtime object or an abstraction by itself, so the feature boundaries still need to be
chosen deliberately. Operations that are better kept outside the core value—such as the `SafeDivide` convenience
and generic JSON helpers—remain focused extensions.

## Making invalid combinations harder

An `Amount` is an immutable value containing both a decimal value and a `Currency`. It is no longer possible to
silently treat ten euros as interchangeable with ten dollars: adding amounts checks that their currencies match.
The currency itself is a domain value, with a code, name, symbol, validity period and explicit minor unit.

That explicit minor unit handles more than the familiar hundredths. Yen have no minor unit; some currencies use
thousandths; the Malagasy ariary is divided into fifths. Recording that rule in the currency means rounding can be
centralised instead of scattered through callers as special cases. Amounts are rounded when constructed, using
banker's rounding by default.

The same modelling approach carries through the rest of the library:

- Arithmetic works on amounts while enforcing currency compatibility.
- `SafeDivide` allocates shares so rounding does not lose or create money.
- `ExchangeRate` makes the direction and currencies of a conversion explicit.
- Formatting and parsing account for culture and currency, rather than passing around ambiguous strings.
- JSON and type conversion have deliberate representations, while immutable record structs make values safe to
  compare and pass around.
- A process-wide, thread-safe currency registry keeps reference data in one place and supports custom currencies.

These are not features added just to make the library bigger. Each one explores a practical question: where should a
rule live, how can the type make misuse visible, and how can the API make correct behaviour the natural path?

## Showing the work, not just describing it

The final step was to build a React UI that makes the libraries easier to explore. The showcase has two code tours:
one follows a number from `ToWords` through the language grammars; the other walks through the money model and its
design decisions. The tours display excerpts from the actual C# source, so they explain the implementation rather
than a separate, easily outdated version of it.

There is also a cheque that writes its amount in words as you type. Choose a language and currency, enter an amount,
and the UI sends it to a small ASP.NET Core API. The API validates the request and delegates the money behaviour to
Hs.Core.Money; it does not duplicate the rounding or wording rules in the web app. A short typing delay avoids
sending a request for every keystroke.

The result brings the original interview exercise full circle. What began as “write this number in words” became a
functional C# library, then a demonstration of domain modelling and other software techniques, and finally an
interactive way to see how the pieces fit together. The number-to-words feature is still at the centre—but now it
also shows how a small, well-defined capability can become the foundation for a larger design.
