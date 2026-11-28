# Money.DataType

[![CI](https://github.com/Allann/Money.DataType/actions/workflows/ci.yml/badge.svg)](https://github.com/Allann/Money.DataType/actions/workflows/ci.yml)
[![CodeQL](https://github.com/Allann/Money.DataType/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/Allann/Money.DataType/actions/workflows/codeql-analysis.yml)

Two small .NET 10 libraries:

- **Hs.Core.Money** treats money as a value in its own right: an amount in a currency that can't be accidentally
  added to another currency, rounded to that currency's minor unit, divided without losing a cent, formatted,
  parsed, converted at an exchange rate and written out in words.
- **Hs.Core** writes any `decimal` out in words, in the language of a culture (English, Dutch and Japanese so far).

It began life as a job-interview exercise in 1999 and has since been rebuilt to show how I'd write it today:
immutable value types, explicit domain modelling, property-based tests and measured performance.

## Using it

```csharp
var enUS = CultureInfo.GetCultureInfo("en-US");
var nlNL = CultureInfo.GetCultureInfo("nl-NL");
var jaJP = CultureInfo.GetCultureInfo("ja-JP");
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

// The yen has no minor unit, so an amount is whole yen in figures and in words.
var yen = new Amount(1234.56m, "JPY");   // 1235 JPY
yen.ToString("C", jaJP);                 // "¥1,235"
yen.ToString("W", jaJP);                 // "千二百三十五円"
yen.ToString("W", enUS);                 // "one thousand two hundred and thirty-five yen"
new Amount(1234.56m, "USD").ToString("W", jaJP); // "千二百三十四ドル五十六セント"

var rate = ExchangeRate.Parse("EUR/USD 1.2591");
rate.Convert(new Amount(100m, "EUR"));   // 125.91 USD

21m.ToWords(enUS);                       // "twenty-one"
21m.ToWords(nlNL);                       // "eenentwintig"
21m.ToWords(jaJP);                       // "二十一"
123_456_789m.ToWords(jaJP);              // "一億二千三百四十五万六千七百八十九": digits group in fours
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

## Showcase

A React front end over the libraries, in [showcase/](showcase/):

- **A code tour of Hs.Core**: how a number becomes words, from the extension method to three grammars.
- **A cheque that writes itself**: type an amount and it's written out in words in English, Dutch or Japanese,
  with a keystroke delay so it asks only when you pause.
- **An architecture tour of Hs.Core.Money**: amounts, minor units, the registry, lossless division,
  formatting, JSON and the decisions behind them.

The tours quote the real source files at build time, and a test fails if an excerpt no longer matches the code.
The front end calls [Hs.Showcase.Api](showcase/Hs.Showcase.Api/), a minimal ASP.NET Core API that checks its
input and returns JSON; it holds no money logic of its own.

```bash
dotnet run --project showcase/Hs.Showcase.Api   # the API, on http://localhost:5080
cd showcase/web && npm install && npm run dev    # the app, on http://localhost:5173
```

In VS Code, choose **Showcase: Full stack** from Run and Debug to start the API and React app together.
Choose **Showcase: API** to debug the API and open Scalar at `/scalar`, or **Showcase: React app** to run
the front end on its own. Scalar lets you inspect and try the API operations. The React app proxies API
requests to `http://localhost:5080`, so start the API as well when you want to exercise its endpoints
through the UI. Starting the API stops an existing
Showcase API process on port 5080 before rebuilding, avoiding a file-lock or port-conflict failure.

Or `npm run build` in `showcase/web`, which builds the app into the API's `wwwroot`, so the API serves both on
port 5080. Requires Node 24.

## Project layout

```text
src/Hs.Core                 number to words (English, Dutch, Japanese)
src/Hs.Core.Money           Amount, Currency, ExchangeRate, formatting, parsing, JSON, words
tests/Hs.Core.Tests         unit and property-based tests
tests/Hs.Core.Money.Tests   unit, property-based and ISO 4217 conformance tests
showcase/Hs.Showcase.Api    minimal API over the libraries, serving the web app
showcase/web                React front end: code tours and the cheque
tests/Hs.Showcase.Api.Tests API tests through ASP.NET Core's pipeline
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

The .NET suite has about 680 tests, including [CsCheck](https://github.com/AnthonyLloyd/CsCheck) properties: for any
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
