/**
 * Small, pure helpers for the page: reading what a person types, and illustrating how Hs.Core takes a number apart.
 * The words themselves always come from the C# code, through the API.
 */

/** decimal.MaxValue: the largest number Hs.Core words. */
export const decimalMax = '79228162514264337593543950335'

/** A digit group, as Hs.Core's DigitGroup: 1,234,567 in thousands is 1 (scale 2) · 234 (scale 1) · 567 (scale 0). */
export type DigitGroup = { value: number; scale: number }

/** Mirrors DigitGroup.Split: the non-zero groups of `size` in a whole number, most significant first. */
export function splitDigitGroups(whole: bigint, size: 1000 | 10000): DigitGroup[] {
  const groups: DigitGroup[] = []
  for (let rest = whole, scale = 0; rest > 0n; rest /= BigInt(size), scale++) {
    const value = Number(rest % BigInt(size))
    if (value > 0) groups.unshift({ value, scale })
  }
  return groups
}

/** How Hs.Core sees a number before any words: its sign, whole part and fraction digits. */
export type Anatomy = { negative: boolean; whole: bigint; fractionDigits: number[] }

const decimalPattern = /^(-)?(\d{1,29})(?:\.(\d{1,28}))?$/

/** Reads an invariant number such as "-12.05"; null when it isn't one, or is beyond decimal's range. */
export function anatomy(text: string): Anatomy | null {
  const match = decimalPattern.exec(text.trim())
  if (!match) return null
  const [, minus, whole = '0', fraction = ''] = match
  if (BigInt(whole) > BigInt(decimalMax)) return null

  const digits = fraction.replace(/0+$/, '')
  return { negative: minus === '-' && (BigInt(whole) > 0n || digits !== ''), whole: BigInt(whole), fractionDigits: [...digits].map(Number) }
}

/** The character a culture writes between whole and fraction: "." in en-AU and ja-JP, "," in nl-NL. */
export function decimalSeparator(culture: string): string {
  return new Intl.NumberFormat(culture).formatToParts(1.5).find((part) => part.type === 'decimal')?.value ?? '.'
}

export type AmountInput = {
  /** What the amount field shows, in the culture's style: "1234,5". */
  display: string
  /** The same amount, invariant, for the API: "1234.5"; empty when nothing's been typed. */
  invariant: string
}

/**
 * Cleans what's typed into a cheque's amount box: digits, and one decimal separator if the currency has a minor unit,
 * with no more decimals than it has and at most `maxWholeDigits` before the separator. Anything else is ignored.
 */
export function readAmount(typed: string, separator: string, decimalDigits: number, maxWholeDigits = 15): AmountInput {
  const at = typed.indexOf(separator)
  const keepDigits = (text: string) => text.replace(/\D/g, '')

  const whole = keepDigits(at < 0 ? typed : typed.slice(0, at)).replace(/^0+(?=\d)/, '').slice(0, maxWholeDigits)
  const hasSeparator = at >= 0 && decimalDigits > 0
  const fraction = hasSeparator ? keepDigits(typed.slice(at + separator.length)).slice(0, decimalDigits) : ''

  const display = hasSeparator ? `${whole || '0'}${separator}${fraction}` : whole
  const invariant = whole === '' && !hasSeparator ? '' : `${whole || '0'}${fraction ? `.${fraction}` : ''}`
  return { display, invariant }
}

/** Adds invariant decimals exactly (no floating point), e.g. ["33.33", "33.33", "33.34"] is "100.00". */
export function sumDecimals(values: readonly string[]): string {
  const places = Math.max(0, ...values.map((value) => value.split('.')[1]?.length ?? 0))
  const scaled = (value: string) => {
    const [whole = '0', fraction = ''] = value.replace('-', '').split('.')
    const units = BigInt(whole + fraction.padEnd(places, '0'))
    return value.startsWith('-') ? -units : units
  }
  const total = values.reduce((sum, value) => sum + scaled(value), 0n)
  const digits = (total < 0n ? -total : total).toString().padStart(places + 1, '0')
  const sign = total < 0n ? '-' : ''
  return places === 0 ? sign + digits : `${sign}${digits.slice(0, -places)}.${digits.slice(-places)}`
}

/** Whether two invariant decimals are the same number: "100.00" and "100" are. */
export function sameDecimal(a: string, b: string): boolean {
  const negated = b.startsWith('-') ? b.slice(1) : `-${b}`
  return /^-?[0.]+$/.test(sumDecimals([a, negated]))
}
