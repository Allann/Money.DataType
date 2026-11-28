import { describe, expect, it } from 'vitest'
import { anatomy, decimalMax, decimalSeparator, readAmount, sameDecimal, splitDigitGroups, sumDecimals } from './numbers'

describe('splitDigitGroups, mirroring DigitGroup.Split', () => {
  it('groups in thousands for English and Dutch', () => {
    expect(splitDigitGroups(1_234_567n, 1000)).toEqual([
      { value: 1, scale: 2 },
      { value: 234, scale: 1 },
      { value: 567, scale: 0 },
    ])
  })

  it('groups in ten thousands for Japanese', () => {
    expect(splitDigitGroups(123_456_789n, 10000)).toEqual([
      { value: 1, scale: 2 },
      { value: 2345, scale: 1 },
      { value: 6789, scale: 0 },
    ])
  })

  it('leaves out empty groups, and has none for zero', () => {
    expect(splitDigitGroups(2_000_300_000n, 1000)).toEqual([
      { value: 2, scale: 3 },
      { value: 300, scale: 1 },
    ])
    expect(splitDigitGroups(0n, 1000)).toEqual([])
  })

  it('covers decimal.MaxValue without losing precision', () => {
    expect(splitDigitGroups(BigInt(decimalMax), 1000)[0]).toEqual({ value: 79, scale: 9 })
  })
})

describe('anatomy', () => {
  it('separates sign, whole and fraction digits', () => {
    expect(anatomy('-12.05')).toEqual({ negative: true, whole: 12n, fractionDigits: [0, 5] })
  })

  it('drops trailing zeros, as decimal does', () => {
    expect(anatomy('1.50')).toEqual({ negative: false, whole: 1n, fractionDigits: [5] })
  })

  it('treats minus zero as zero', () => {
    expect(anatomy('-0.0')?.negative).toBe(false)
  })

  it.each(['', 'abc', '1e5', '1.2.3', '79228162514264337593543950336'])('rejects "%s"', (text) => {
    expect(anatomy(text)).toBeNull()
  })
})

describe('decimalSeparator', () => {
  it.each([
    ['en-AU', '.'],
    ['ja-JP', '.'],
    ['nl-NL', ','],
  ])('is right for %s', (culture, separator) => {
    expect(decimalSeparator(culture)).toBe(separator)
  })
})

describe('readAmount', () => {
  it('keeps digits and one separator', () => {
    expect(readAmount('$1,234.56', '.', 2)).toEqual({ display: '1234.56', invariant: '1234.56' })
  })

  it('reads the culture separator, and ignores the other as a thousands separator', () => {
    expect(readAmount('1.234,5', ',', 2)).toEqual({ display: '1234,5', invariant: '1234.5' })
  })

  it('stops at the currency minor unit', () => {
    expect(readAmount('9.999', '.', 2).display).toBe('9.99')
  })

  it('allows no fraction for a currency without a minor unit, like the yen', () => {
    expect(readAmount('1234.56', '.', 0)).toEqual({ display: '1234', invariant: '1234' })
  })

  it('keeps a separator just typed, so typing can carry on', () => {
    expect(readAmount('12.', '.', 2)).toEqual({ display: '12.', invariant: '12' })
  })

  it('writes a leading zero before a bare separator, and drops other leading zeros', () => {
    expect(readAmount('.5', '.', 2).display).toBe('0.5')
    expect(readAmount('007', '.', 2).display).toBe('7')
  })

  it('caps the whole digits', () => {
    expect(readAmount('1234567', '.', 2, 4).display).toBe('1234')
  })

  it('is empty when nothing usable was typed', () => {
    expect(readAmount('abc', '.', 2)).toEqual({ display: '', invariant: '' })
  })
})

describe('sumDecimals', () => {
  it('adds exactly, where floating point would not', () => {
    expect(sumDecimals(['0.1', '0.2'])).toBe('0.3')
    expect(sumDecimals(['33.33', '33.33', '33.34'])).toBe('100.00')
  })

  it('handles whole numbers, negatives and different places', () => {
    expect(sumDecimals(['333', '333', '334'])).toBe('1000')
    expect(sumDecimals(['-1.5', '0.25'])).toBe('-1.25')
  })

  it('compares decimals by value', () => {
    expect(sameDecimal('100.00', '100')).toBe(true)
    expect(sameDecimal('0.05', '0.050')).toBe(true)
    expect(sameDecimal('0.05', '0.04')).toBe(false)
    expect(sameDecimal('-2', '2')).toBe(false)
  })
})
