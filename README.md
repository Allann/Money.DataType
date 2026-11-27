# Money.DataType

[![CI](https://github.com/Allann/Money.DataType/actions/workflows/ci.yml/badge.svg)](https://github.com/Allann/Money.DataType/actions/workflows/ci.yml)
[![CodeQL](https://github.com/Allann/Money.DataType/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/Allann/Money.DataType/actions/workflows/codeql-analysis.yml)

Two small .NET 10 libraries:

- **Hs.Core.Money** treats money as a value in its own right: an amount in a currency that can't be accidentally
  added to another currency, rounded to that currency's minor unit, divided without losing a cent, formatted,
  parsed, converted at an exchange rate and written out in words.
- **Hs.Core** writes any `decimal` out in words, in the language of a culture (English and Dutch so far).

It began life as a job-interview exercise in 2020 and has since been rebuilt to show how I'd write it today:
immutable value types, explicit domain modelling, property-based tests and measured performance.

## Using it

```csharp
var enUS = CultureInfo.GetCultureInfo("en-US");
var nlNL = CultureInfo.GetCultureInfo("nl-NL");
var price = new Amount(1234.5m, "EUR");

price * 1.21m;                           // 1493.74 EUR: 1493.745 rounds to even (banker's rounding)
price + new Amount(10m, "USD");          // throws InvalidCurrencyException

new Amount(0.05m, "EUR").SafeDivide(2);  // 0.02, 0.03 EUR: nothing lost to rounding
new Amount(1m, "EUR").SafeDivide([2, 3, 3]); // 0.25, 0.38, 0.37 EUR

price.ToString("C", enUS);               // "€1,234.50"
price.ToString("I", enUS);               // "EUR 1,234.50"
price.ToString("F", enUS);               // "1,234.50 Euro"
price.ToString("W", enUS);               // "one thousand two hundred and thirty-four euros and fifty cents"
price.ToString("W", nlNL);               // "duizend tweehonderdvierendertig euro en vijftig cent"

var rate = ExchangeRate.Parse("EUR/USD 1.2591");
rate.Convert(new Amount(100m, "EUR"));   // 125.91 USD

21m.ToWords(enUS);                       // "twenty-one"
21m.ToWords(nlNL);                       // "eenentwintig"
```

Every example above is a test ([ReadmeExamplesSpec](tests/Hs.Core.Money.Tests/ReadmeExamplesSpec.cs)), so it can't
drift from the code.

## Design

The decisions that shape the libraries, and what they cost, are recorded as [ADRs](docs/adr/README.md):

1. [Banker's rounding by default](docs/adr/0001-bankers-rounding-by-default.md)
2. [Model a currency's minor unit explicitly](docs/adr/0002-explicit-minor-units.md): hundredths, thousandths,
   none, or fifths (the Malagasy ariary), with one rounding rule for all of them
3. [Immutable value records, serialised with System.Text.Json only](docs/adr/0003-immutable-value-records.md)
4. [A process-wide, thread-safe currency registry](docs/adr/0004-a-process-wide-currency-registry.md)
5. [Number words: each language owns its grammar](docs/adr/0005-language-grammar-behind-ilanguage.md)

The ISO 4217 currency table is checked against a snapshot of the official list, so a currency that's
missing, withdrawn or has the wrong number or minor units fails the build.

## Project layout

```text
src/Hs.Core                 number to words (English, Dutch)
src/Hs.Core.Money           Amount, Currency, ExchangeRate, formatting, parsing, JSON, words
tests/Hs.Core.Tests         unit and property-based tests
tests/Hs.Core.Money.Tests   unit, property-based and ISO 4217 conformance tests
benchmarks/                 BenchmarkDotNet benchmarks
docs/adr/                   architecture decision records
```

## Building and testing

Requires the .NET 10 SDK.

```bash
dotnet build Hs.Core.slnx                 # warnings are errors, with the recommended .NET analyzers
dotnet test --solution Hs.Core.slnx       # xunit v3 on Microsoft.Testing.Platform
dotnet test --solution Hs.Core.slnx --coverlet --coverlet-output-format cobertura   # with coverage
```

The test suite has about 630 tests, including [CsCheck](https://github.com/AnthonyLloyd/CsCheck) properties: for any
generated amount, dividing never loses money, arithmetic obeys its laws, and formatting, JSON and type conversion
round-trip. Line coverage is 100% for Hs.Core and 97% across Hs.Core.Money's suite. CI builds and tests on Linux
and Windows, publishes a coverage summary and the NuGet packages, and runs CodeQL.

## Performance

```bash
dotnet run -c Release --project benchmarks/Hs.Core.Money.Benchmarks -- --filter '*'
```

On .NET 10 (BenchmarkDotNet short run, x64):

| Operation | Time | Allocated |
| --- | ---: | ---: |
| `Currency.FromCode("EUR")` | 14 ns | 0 B |
| `a + b` | 17 ns | 0 B |
| `ToString("C")` | 80 ns | 408 B |
| `Amount.Parse` | 98 ns | 336 B |
| `ToJson()` / `FromJson()` | 93 / 363 ns | 120 / 240 B |
| `SafeDivide(3)` | 298 ns | 1,856 B |
| `ToString("W")` (in words) | 408 ns | 1,320 B |

The benchmarks paid for themselves: they showed the JSON helpers rebuilding their serialiser options on every
call (serialising was 5× slower) and a closure allocation on every currency lookup; both are fixed.
