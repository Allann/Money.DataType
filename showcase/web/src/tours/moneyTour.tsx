import { Link } from 'react-router'
import { ArchitectureMap, DivideDemo, RoundingDemo } from './demos/MoneyDemos'
import type { TourDefinition } from './Tour'

const money = (file: string) => `src/Hs.Core.Money/${file}`

export const moneyTour: TourDefinition = {
  title: 'Hs.Core.Money: the architecture',
  kicker: 'Architecture tour · 10 steps',
  next: { to: '/cheque', label: 'Finish: write a cheque' },
  steps: [
    {
      id: 'overview',
      title: 'The shape of it',
      lede: 'Three layers, each depending only on the one below; money logic lives in exactly one of them.',
      body: (
        <>
          <p>
            <strong>Hs.Core</strong> writes numbers in words and knows nothing about money.{' '}
            <strong>Hs.Core.Money</strong> builds on it: an <code>Amount</code> is a value in a{' '}
            <code>Currency</code>, and everything else (rounding, arithmetic, division, formatting, parsing, JSON,
            exchange rates and words) is a pure operation on those two types.
          </p>
          <p>
            This showcase sits on top: a minimal ASP.NET Core API that checks its input and returns JSON, and the
            React app you're reading. Neither knows how to round a yen. They ask the library.
          </p>
          <p>Each box opens the step that explains it.</p>
        </>
      ),
      demo: <ArchitectureMap />,
    },
    {
      id: 'amount',
      title: 'An amount is a value',
      lede: 'Amount is an immutable record struct of a decimal and a Currency, rounded when it’s created.',
      body: (
        <>
          <p>
            Money is never a bare <code>decimal</code> here. Ten is meaningless; ten euros isn't. Pairing the two in a{' '}
            <code>readonly record struct</code> means the compiler generates equality, values can't change after
            construction, and an amount is safe to share, cache or use as a dictionary key.
          </p>
          <p>
            Every constructor funnels into one, which rounds to the currency's minor unit. There's no way to hold
            12.345 US dollars, so nothing downstream has to wonder whether a value is rounded yet.
          </p>
          <p>
            The default rounding is banker's rounding (half to even), so sums of many amounts don't drift upwards.
            Callers whose rules differ pass a <code>MidpointRounding</code>.
          </p>
        </>
      ),
      adr: { title: 'ADR 0003: Immutable value records, serialised with System.Text.Json only', file: '0003-immutable-value-records.md' },
      code: [
        { spec: { file: money('Amount.cs'), from: 'public Amount(decimal value, Currency currency, MidpointRounding rounding)', highlight: ['currency.Round(value, rounding)'] } },
        { spec: { file: money('Amount.cs'), from: 'public decimal Value { get; }', to: 'public Currency Currency { get; }' } },
      ],
    },
    {
      id: 'minor-units',
      title: 'Currencies know their smallest coin',
      lede: 'A currency has a minor unit (0.01, 1, 0.001 or even 0.2) and one rounding rule that honours it.',
      body: (
        <>
          <p>
            Most currencies divide into hundredths, but not all: the yen has no minor unit, the Bahraini dinar has
            thousandths, and the Malagasy ariary divides into <em>fifths</em>. This was once a <code>double</code>{' '}
            with magic values (−1 and log₁₀5) that every caller had to special-case, and two didn't.
          </p>
          <p>
            Now <code>DecimalDigits</code> is just what to display, and <code>MinorUnit</code> is an explicit{' '}
            <code>decimal</code>. <code>Currency.Round</code> is the single rule: a power of ten uses{' '}
            <code>Math.Round</code> to that many places, and anything else rounds to a whole number of minor units.
          </p>
          <p>
            Try 0.3 in the demo: the ariary rounds it to 0.4, two fifths. Then 0.125 and 2.5, which show banker's
            rounding sending each midpoint to the even neighbour.
          </p>
        </>
      ),
      adr: { title: 'ADR 0002: Model a currency’s minor unit explicitly', file: '0002-explicit-minor-units.md' },
      demo: <RoundingDemo />,
      code: [
        { spec: { file: money('Currency.cs'), from: 'public int DecimalDigits { get; }', withoutDocs: true } },
        { spec: { file: money('Currency.cs'), from: 'public decimal MinorUnit { get; }' } },
        { spec: { file: money('Currency.cs'), from: 'internal decimal Round(decimal amount', highlight: ['Math.Round(amount / MinorUnit, rounding) * MinorUnit'] } },
      ],
    },
    {
      id: 'safety',
      title: 'Currencies can’t be mixed',
      lede: 'Adding euros to dollars throws, because there’s no right answer without an exchange rate.',
      body: (
        <>
          <p>
            The operators make amounts feel like numbers (<code>price * 1.21m</code>, <code>a + b</code>,{' '}
            <code>-a</code>), but combining two amounts checks that their currencies match first. A mismatch throws{' '}
            <code>InvalidCurrencyException</code>, naming both.
          </p>
          <p>
            Multiplying or dividing by a plain <code>decimal</code> is always allowed, and the result is rounded
            again. Dividing one amount by another returns a <code>decimal</code> ratio, because the currency
            cancels out.
          </p>
          <p>
            Ordinary division can lose a cent to rounding, so the <code>/</code> operator's documentation points to{' '}
            <code>SafeDivide</code>.
          </p>
        </>
      ),
      code: [
        { spec: { file: money('Amount.BinaryOperators.cs'), from: 'public static Amount Add(Amount money1, Amount money2)', withoutDocs: true } },
        { spec: { file: money('Amount.cs'), from: 'private static void AssertIsSameCurrency' } },
      ],
    },
    {
      id: 'registry',
      title: 'One registry, safe across threads',
      lede: 'Currencies are reference data, like cultures: one process-wide, lock-free registry holds them.',
      body: (
        <>
          <p>
            <code>Currency.FromCode("EUR")</code> looks the currency up in a <code>ConcurrentDictionary</code> keyed
            by (namespace, code), seeded with the ISO 4217 currencies, current and historic. Applications can register
            their own, a loyalty-points currency say, in a namespace of their own.
          </p>
          <p>
            It used to be a plain <code>Dictionary</code>, and under the parallel test run a lookup during a
            registration threw “Collection was modified”. The fix kept the simple static API rather than injecting a
            registry everywhere an amount is created. A lookup takes about 14 ns and allocates nothing.
          </p>
          <p>
            A lookup by code alone prefers current ISO, then historic ISO, then custom, so a withdrawn code still
            resolves.
          </p>
        </>
      ),
      adr: { title: 'ADR 0004: A process-wide, thread-safe currency registry', file: '0004-a-process-wide-currency-registry.md' },
      code: [
        { spec: { file: money('CurrencyRegistry.cs'), from: 'internal static class CurrencyRegistry', to: 'new(DefaultCurrencies.All' } },
        { spec: { file: money('CurrencyRegistry.cs'), from: 'public static bool TryGet(string code, out Currency currency)', highlight: ['TryGetCustom'] } },
      ],
    },
    {
      id: 'divide',
      title: 'Dividing without losing a cent',
      lede: 'SafeDivide rounds every share but the last, and gives the last one whatever is left.',
      body: (
        <>
          <p>
            Split €100 three ways and naive rounding gives 33.33 × 3 = 99.99, so a cent vanishes. Over a payroll or a
            ledger, those cents matter.
          </p>
          <p>
            <code>Allocate</code> rounds each share (by equal parts or by ratios) to the currency's minor unit, then
            gives the last share the remainder, so the shares always add up to exactly the amount. Because it
            rounds with <code>Currency.Round</code>, it works for yen and fifths too.
          </p>
          <p>
            A property test checks this for thousands of generated amounts, currencies and share counts. The demo
            adds up the shares again in your browser, exactly, rather than trusting the server.
          </p>
        </>
      ),
      demo: <DivideDemo />,
      code: [
        { spec: { file: money('Extensions/AmountExtensions.SafeDivide.cs'), from: 'private static Amount[] Allocate', highlight: ['var remainder = money.Value - shares.Sum();'] } },
        { spec: { file: 'tests/Hs.Core.Money.Tests/AmountProperties.cs', from: 'public void DividingIntoSharesNeverLosesOrCreatesMoney', withoutDocs: true } },
      ],
    },
    {
      id: 'formatting',
      title: 'Formatting and parsing',
      lede: 'Amount implements IFormattable with four formats of its own; parsing finds the currency in the text.',
      body: (
        <>
          <p>
            The culture decides the <em>shape</em> (separators, where the symbol goes, how negatives look) and the
            currency decides the <em>content</em> (its symbol and number of digits). So €1,234.50 in en-US becomes €
            1.234,50 in nl-NL.
          </p>
          <ul>
            <li>
              <code>C</code>: with the symbol, “€1,234.50”
            </li>
            <li>
              <code>I</code>: with the ISO code, “EUR 1,234.50”
            </li>
            <li>
              <code>F</code>: with the English name, “1,234.50 Euro”
            </li>
            <li>
              <code>W</code>: in words, in the culture's language
            </li>
          </ul>
          <p>
            Parsing works backwards: it gathers the non-numeric characters, and takes the currency whose symbol or
            code they are. If none matches, or several share the symbol, it says so instead of guessing.
          </p>
        </>
      ),
      code: [
        { spec: { file: money('Amount.Formattable.cs'), from: 'public string ToString(string? format, IFormatProvider? formatProvider)', withoutDocs: true, highlight: ['"W" => ToWords'] } },
        { spec: { file: money('Amount.Parsable.cs'), from: 'private static (Currency? Currency, string? Error) CurrencyIn' } },
      ],
    },
    {
      id: 'serialisation',
      title: 'JSON and type conversion',
      lede: 'One supported serialiser, System.Text.Json, wired in by attributes, so amounts serialise with no setup.',
      body: (
        <>
          <p>
            An amount is written as <code>{'{"value":1234.5,"currency":"EUR"}'}</code>. The currency goes as its
            code, never as the whole currency, so the JSON stays small and the registry stays the source of truth.
            Reading is lenient (a numeric string or a full currency object also works) and goes through the
            constructor, so it rounds like everything else.
          </p>
          <p>
            Legacy <code>BinaryFormatter</code> and XML support was removed: <code>BinaryFormatter</code> is a
            security risk and obsolete in .NET. A <code>TypeConverter</code> remains for configuration binding and
            designers.
          </p>
          <p>
            Benchmarks found the JSON helpers building new serialiser options on every call, which made serialising
            five times slower. They're shared now.
          </p>
        </>
      ),
      adr: { title: 'ADR 0003: Immutable value records, serialised with System.Text.Json only', file: '0003-immutable-value-records.md' },
      code: [
        { spec: { file: money('Serialization/JsonConverters.cs'), from: 'public override void Write(Utf8JsonWriter writer, Amount value' } },
        { spec: { file: money('Serialization/JsonConverters.cs'), from: 'private static decimal ReadAmountValue' } },
      ],
    },
    {
      id: 'words',
      title: 'Amounts in words',
      lede: 'Hs.Core.Money splits an amount into major and minor units, names them, and asks Hs.Core for the numbers.',
      body: (
        <>
          <p>
            This is where the two libraries meet. <code>Split</code> turns 1234.56 USD into 1234 dollars and 56 cents
            using the currency's minor unit, so yen (no minor unit) and fifths need no special cases.{' '}
            <code>Count</code> then calls Hs.Core's <code>ToWords</code> for each number.
          </p>
          <p>
            What money adds is a <code>MoneyLanguage</code>: unit names and plurals, the word between them, and
            spacing. Japanese has no “and” and no spaces, so 千二百三十四ドル五十六セント is just concatenation. Dutch doesn't
            pluralise units, so it's “vijf dollar”.
          </p>
          <p>
            Keeping money words out of Hs.Core is the dependency rule from step 1 in practice. The{' '}
            <Link to="/cheque">cheque</Link> is this code running as you type.
          </p>
        </>
      ),
      code: [
        { spec: { file: money('Wording/AmountWords.cs'), from: 'public static Words Write(Amount amount, CultureInfo culture)', to: 'private readonly record struct Parts', highlight: ['count.ToWords(culture)'] } },
        { spec: { file: money('Wording/MoneyLanguage.cs'), from: '// Japanese doesn', to: '}));', withoutDocs: true } },
      ],
    },
    {
      id: 'exchange',
      title: 'Exchange rates, and how it’s all proven',
      lede: 'An ExchangeRate converts in either direction; tests check every claim the tour has made.',
      body: (
        <>
          <p>
            <code>ExchangeRate.Parse("EUR/USD 1.2591")</code> gives a rate that converts euros to dollars and back.
            Converting money in a third currency throws rather than guessing a cross rate.
          </p>
          <p>The claims in this tour are checked by tests:</p>
          <ul>
            <li>
              <strong>Properties</strong> (CsCheck) for any amount: it's a whole number of minor units, dividing
              never loses money, arithmetic obeys its laws, and formatting, JSON and type conversion round-trip.
            </li>
            <li>
              <strong>ISO 4217 conformance</strong>: the currency table is compared with a snapshot of the official
              list, so a missing, withdrawn or mis-numbered currency fails the build.
            </li>
            <li>
              <strong>README examples</strong> are tests too, so the documentation can't drift from the code.
            </li>
            <li>
              <strong>Benchmarks</strong> (BenchmarkDotNet): a lookup takes 14 ns, adding two amounts 17 ns, and
              neither allocates.
            </li>
          </ul>
        </>
      ),
      code: [
        { spec: { file: money('ExchangeRate.cs'), from: 'public Amount Convert(Amount money)' } },
        { spec: { file: 'tests/Hs.Core.Money.Tests/CurrencySpec.cs', from: 'public void WhenACurrencyIsInTheIsoList_ThenItIsDefined', withoutDocs: true } },
      ],
    },
  ],
}
