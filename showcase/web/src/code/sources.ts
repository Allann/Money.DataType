import { cutExcerpt, type Excerpt, type ExcerptSpec } from './excerpt'

/**
 * The solution's C# source, bundled at build time. The tours quote these files, so what they show is always
 * the code that's actually in the repository.
 */
const files = import.meta.glob(
  [
    '../../../../src/**/*.cs',
    '!**/obj/**',
    '!**/bin/**',
    // Big data tables that no step quotes.
    '!**/DefaultCurrencies.cs',
    '!**/Amount.FiveMostUsedCurrencies.cs',
    // Only the tests the tours quote.
    '../../../../tests/Hs.Core.Tests/NumberToWordsProperties.cs',
    '../../../../tests/Hs.Core.Money.Tests/AmountProperties.cs',
    '../../../../tests/Hs.Core.Money.Tests/CurrencySpec.cs',
  ],
  {
    query: '?raw',
    import: 'default',
    eager: true,
  },
) as Record<string, string>

/** Source files by their path from the repository root, e.g. "src/Hs.Core/Words.cs". */
export const sources: ReadonlyMap<string, string> = new Map(
  Object.entries(files).map(([path, text]) => [path.replace(/^(\.\.\/)+/, ''), text]),
)

export function excerpt(spec: ExcerptSpec): Excerpt {
  const source = sources.get(spec.file)
  if (source === undefined) throw new Error(`${spec.file} isn't bundled: add it to the glob in sources.ts.`)
  return cutExcerpt(source, spec)
}
