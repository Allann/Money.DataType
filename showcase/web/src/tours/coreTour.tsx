import { AnatomyDemo, CompareDemo, GroupsDemo } from './demos/CoreDemos'
import type { TourDefinition } from './Tour'

const core = (file: string) => `src/Hs.Core/${file}`

export const coreTour: TourDefinition = {
  title: 'Hs.Core: numbers in words',
  kicker: 'Code tour · 9 steps',
  next: { to: '/tour/money', label: 'Next: tour the money architecture' },
  steps: [
    {
      id: 'entry',
      title: 'One extension method',
      lede: 'The whole public API is ToWords on decimal, in the current culture or one you choose.',
      body: (
        <>
          <p>
            Callers write <code>21m.ToWords(culture)</code>. There's no converter to construct, no options object and no
            interface to inject: turning a number into words has no state and no side effects, so it's a pure static
            function.
          </p>
          <p>
            The culture only picks a <em>language</em>, by its two-letter ISO name, so <code>nl-NL</code> and{' '}
            <code>nl-BE</code> both get Dutch. Any culture without a language of its own falls back to English instead
            of throwing. That's a deliberate choice: words are for reading, and English words beat an exception.
          </p>
          <p>
            <code>decimal</code> sets the range: up to 79,228,162,514,264,337,593,543,950,335 and 28 decimal places,
            with no rounding surprises from binary floating point.
          </p>
        </>
      ),
      code: [{ spec: { file: core('NumberToWords.cs'), from: 'public static class NumberToWords', to: 'Languages.GetValueOrDefault', highlight: ['["ja"]', 'GetValueOrDefault'] } }],
    },
    {
      id: 'words-type',
      title: 'Words, not strings',
      lede: 'The result is a Words value, so a number’s words can’t be confused with any other string.',
      body: (
        <>
          <p>
            <code>ToWords</code> could have returned a <code>string</code>. It returns a one-field{' '}
            <code>readonly record struct</code> instead, which costs nothing at run time (a struct with one reference is
            the size of that reference) and gives the compiler something to check: a method that wants words won't
            accept a customer name.
          </p>
          <p>
            As a record, it also gets value equality for free, which the tests rely on:{' '}
            <code>21m.ToWords(dutch).Should().Be(new Words("eenentwintig"))</code>.
          </p>
        </>
      ),
      code: [{ spec: { file: core('Words.cs'), from: 'public readonly record struct Words' } }],
    },
    {
      id: 'sign-fraction',
      title: 'Sign and fraction first',
      lede: 'Before any language is involved, the number is taken apart: sign, whole part, fraction digits.',
      body: (
        <>
          <p>
            Every language deals with the same three things, so <code>NumberToWords</code> handles them once:
          </p>
          <ul>
            <li>
              <strong>Signed</strong> words the magnitude and lets the language say “minus”, “min” or
              “マイナス”.
            </li>
            <li>
              <strong>Unsigned</strong> splits off the fraction with <code>decimal.Truncate</code>; a whole number
              never mentions a point.
            </li>
            <li>
              <strong>FractionDigits</strong> reads the fraction one digit at a time, recursively: shift left by
              multiplying by ten, take the digit, recurse on the rest. Fractions are read digit by digit
              (“three point one four”), not as a number (“three point fourteen”).
            </li>
          </ul>
          <p>
            Because <code>decimal</code> keeps its digits exactly, the recursion ends when the fraction reaches zero.
            Even 0.0000000000000000000000000001 comes out as twenty-seven zeros and a one.
          </p>
        </>
      ),
      demo: <AnatomyDemo />,
      code: [{ spec: { file: core('NumberToWords.cs'), from: 'private static string Signed', to: 'return [(int)digit', highlight: ['language.Negative', 'FractionDigits(shifted - digit)'] } }],
    },
    {
      id: 'grammar',
      title: 'Each language owns its grammar',
      lede: 'ILanguage has four methods, and each language implements all of its own rules behind them.',
      body: (
        <>
          <p>
            The obvious design is one algorithm with a word list per language. But languages differ in structure, not
            just vocabulary: Dutch writes numbers up to a thousand as one word with units before tens
            (“eenentwintig”, one-and-twenty), and Japanese groups digits in fours and uses no spaces.
          </p>
          <p>
            So the line is drawn at grammar, not vocabulary. A language gets a whole number and returns its words,
            however it likes. The shared code only decides <em>when</em> to call the language (sign, whole, fraction),
            never <em>how</em> it words things.
          </p>
          <p>
            The first version did share three-digit grouping. Adding Japanese showed that grouping was a language
            rule in disguise, so it moved behind <code>ILanguage</code> too. English and Dutch each having a{' '}
            <code>BelowHundred</code> looks like duplication but isn't: the rules really are different.
          </p>
        </>
      ),
      adr: { title: 'ADR 0005: Number words: each language owns its grammar', file: '0005-language-grammar-behind-ilanguage.md' },
      code: [{ spec: { file: core('Languages/ILanguage.cs'), from: 'internal interface ILanguage' } }],
    },
    {
      id: 'groups',
      title: 'Splitting into digit groups',
      lede: 'Every language reads big numbers a group at a time; DigitGroup.Split finds the groups.',
      body: (
        <>
          <p>
            English and Dutch read 1,234,567 as three groups of a thousand: “one million, two hundred and
            thirty-four thousand, five hundred and sixty-seven”. Japanese reads it as groups of ten thousand: 123 万
            4567.
          </p>
          <p>
            <code>Split</code> takes the group size as a parameter and is a pure recursive function: the remainder
            is this group, the quotient is the rest. It drops empty groups, which is how “two billion three hundred
            thousand” skips the millions.
          </p>
          <p>
            Each group is a <code>record struct</code> carrying its <code>Scale</code>, the power of the group size,
            which a language maps to its own word: “million” or 億.
          </p>
        </>
      ),
      demo: <GroupsDemo />,
      code: [{ spec: { file: core('DigitGroup.cs'), from: 'internal readonly record struct DigitGroup', highlight: ['whole < size', 'Groups(decimal.Truncate(whole / size)'] } }],
    },
    {
      id: 'english',
      title: 'English: hundreds, and “and”',
      lede: 'British English puts “and” before the last two digits, and pattern matching says exactly when.',
      body: (
        <>
          <p>
            Most of English is three lookup tables. The one subtle rule is “and”: 105 is “one hundred and five”, and
            1,005 is “one thousand and five”, but 1,100 is “one thousand one hundred”.
          </p>
          <p>
            A list pattern states that rule directly: when there's more than one group and the last group is the
            units with no hundreds, join it with “and”. <code>InWords</code> covers the same rule inside a group, and{' '}
            <code>BelowHundred</code> hyphenates “forty-two”.
          </p>
          <p>Each case is a single expression, with no mutable string builders and no flags.</p>
        </>
      ),
      code: [
        { spec: { file: core('Languages/English.cs'), from: 'private static string Whole(DigitGroup[] groups)', highlight: ['{ Scale: 0, Hundreds: 0 } last'] } },
        { spec: { file: core('Languages/English.cs'), from: 'private static string InWords(DigitGroup group)', to: '_ => $"{Tens[number / 10]}-{Units[number % 10]}"' } },
      ],
    },
    {
      id: 'dutch',
      title: 'Dutch: one word to a thousand',
      lede: 'Dutch puts units before tens, joined with “en”, and writes everything below a thousand as one word.',
      body: (
        <>
          <p>
            Following the Taalunie spelling rules, 999 is <em>negenhonderdnegenennegentig</em>: nine-hundred-nine-and-ninety, as
            one word. “Duizend” joins on too (tweeduizend), but there's a space after it and after miljoen, miljard
            and the rest.
          </p>
          <p>
            The diaeresis is part of the grammar: 22 is <em>tweeëntwintig</em>, because “twee” ends in a vowel and
            “een” starts with one. One conditional on <code>EndsWith('e')</code> covers it.
          </p>
          <p>
            Dutch uses the long scale, so 10⁹ is a <em>miljard</em> and 10¹² a <em>biljoen</em>. That's why it can't
            share English's scale words, even though both group in thousands.
          </p>
        </>
      ),
      code: [{ spec: { file: core('Languages/Dutch.cs'), from: 'private static string Group(DigitGroup group)', to: '(Units[number % 10].EndsWith', highlight: ['{ Scale: 1, Value: 1 } => "duizend"', 'EndsWith'] } }],
    },
    {
      id: 'japanese',
      title: 'Japanese: groups of four',
      lede: 'Japanese counts in ten thousands (万, 億, 兆 …), drops “one” before 十, 百 and 千, and uses no spaces.',
      body: (
        <>
          <p>
            123,456,789 is 一億二千三百四十五万六千七百八十九: 1 億, 2345 万, 6789. Within a group, each digit takes
            its place word (千, 百, 十), and zeros say nothing.
          </p>
          <p>
            One quirk: 1,000 is 千, not 一千, but 10,000 is 一万. <code>Place</code> captures it in a four-arm switch
            on (digit, place). And there's no fraction grammar to learn beyond 点, the point.
          </p>
          <p>
            Japanese was added to show off the money side. The yen has no minor unit, so a Japanese cheque is all
            whole numbers.
          </p>
        </>
      ),
      code: [{ spec: { file: core('Languages/Japanese.cs'), from: 'public string Whole(decimal whole)', to: 'private static int Pow10', highlight: ['(1, _) => Places[place]', 'GroupSize'] } }],
    },
    {
      id: 'proof',
      title: 'Try it, and how it’s proven',
      lede: 'Example tests pin the tricky cases; property tests check laws over thousands of generated numbers.',
      body: (
        <>
          <p>
            The example tests cover the boundaries each grammar cares about (105, 1,005, 1,100, 22, 10,000) and the
            full range, from <code>decimal.MaxValue</code> down to its smallest fraction, in every language.
          </p>
          <p>
            The <a href="https://github.com/AnthonyLloyd/CsCheck">CsCheck</a> properties ask more general questions
            about any decimal: does every number have words? Is a negative exactly “minus” and its magnitude? Do two
            different whole numbers ever get the same words? A failure shrinks to the smallest counterexample.
          </p>
          <p>Line coverage of Hs.Core is 100%.</p>
        </>
      ),
      demo: <CompareDemo />,
      code: [{ spec: { file: 'tests/Hs.Core.Tests/NumberToWordsProperties.cs', from: 'public void ANegativeNumberIsMinusItsMagnitude', to: '((decimal)a).ToWords(culture) != ((decimal)b).ToWords(culture));', withoutDocs: true } }],
    },
  ],
}
