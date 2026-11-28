import { describe, expect, it } from 'vitest'
import { excerpt, sources } from '../code/sources'
import { coreTour } from './coreTour'
import { moneyTour } from './moneyTour'

const tours = [coreTour, moneyTour]

describe.each(tours.map((tour) => [tour.title, tour] as const))('%s', (_, tour) => {
  it('has a unique id for every step', () => {
    const ids = tour.steps.map((step) => step.id)
    expect(new Set(ids).size).toBe(ids.length)
  })

  // The tours quote the real source by what it says. If code is renamed or moved, this fails, not the tour.
  const excerpts = tour.steps.flatMap((step) => (step.code ?? []).map(({ spec }) => [step.id, spec.file, spec.from, spec] as const))

  it.each(excerpts)('%s: quotes %s from "%s"', (_step, _file, _from, spec) => {
    const cut = excerpt(spec)
    expect(cut.code).toContain(spec.from)
    if (spec.to) expect(cut.code).toContain(spec.to)
    expect(cut.endLine - cut.startLine).toBeLessThan(60)
  })
})

describe('sources', () => {
  it('bundles the libraries and tests but no build output', () => {
    const paths = [...sources.keys()]
    expect(paths).toContain('src/Hs.Core/NumberToWords.cs')
    expect(paths).toContain('tests/Hs.Core.Money.Tests/AmountProperties.cs')
    expect(paths.some((path) => /\/(bin|obj)\//.test(path))).toBe(false)
  })
})
