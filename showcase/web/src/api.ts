/** The showcase API (showcase/Hs.Showcase.Api): a thin HTTP layer over Hs.Core and Hs.Core.Money. */

export type Language = { culture: string; name: string; nativeName: string }

export type CurrencyOption = {
  code: string
  name: string
  symbol: string
  decimalDigits: number
  minorUnit: number
}

export type NumberInWords = { number: string; words: string }

export type AmountInWords = { value: string; currency: string; figures: string; words: string }

export type Division = { total: string; currency: string; shares: string[] }

/** A request the API rejected, with its reasons by parameter name. */
export class ApiError extends Error {
  readonly errors: Readonly<Record<string, readonly string[]>>

  constructor(message: string, errors: Record<string, string[]> = {}) {
    super(message)
    this.name = 'ApiError'
    this.errors = errors
  }

  /** The first reason given, or the message: what to show a person. */
  get reason(): string {
    return Object.values(this.errors)[0]?.[0] ?? this.message
  }
}

async function get<T>(path: string, query: Record<string, string>, signal?: AbortSignal): Promise<T> {
  const search = new URLSearchParams(query).toString()
  const response = await fetch(`/api/${path}${search ? `?${search}` : ''}`, { signal })
  if (!response.ok) {
    const problem = (await response.json().catch(() => null)) as { title?: string; errors?: Record<string, string[]> } | null
    throw new ApiError(problem?.title ?? `The API answered ${response.status}.`, problem?.errors)
  }
  return (await response.json()) as T
}

export const api = {
  languages: (signal?: AbortSignal) => get<Language[]>('languages', {}, signal),
  currencies: (signal?: AbortSignal) => get<CurrencyOption[]>('currencies', {}, signal),
  words: (number: string, culture: string, signal?: AbortSignal) =>
    get<NumberInWords>('words', { number, culture }, signal),
  amount: (value: string, currency: string, culture: string, signal?: AbortSignal) =>
    get<AmountInWords>('amount', { value, currency, culture }, signal),
  divide: (value: string, currency: string, shares: number, signal?: AbortSignal) =>
    get<Division>('divide', { value, currency, shares: String(shares) }, signal),
}

/** The cultures the showcase offers, so pages can render before /api/languages answers. */
export const languages: readonly Language[] = [
  { culture: 'en-AU', name: 'English', nativeName: 'English' },
  { culture: 'nl-NL', name: 'Dutch', nativeName: 'Nederlands' },
  { culture: 'ja-JP', name: 'Japanese', nativeName: '日本語' },
]
