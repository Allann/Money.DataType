import { act, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { Cheque, defaultDelayMs } from './Cheque'

const currencies = [
  { code: 'AUD', name: 'Australian Dollar', symbol: '$', decimalDigits: 2, minorUnit: 0.01 },
  { code: 'JPY', name: 'Yen', symbol: '¥', decimalDigits: 0, minorUnit: 1 },
]

/** A stand-in for the API that records the amounts asked for. */
function fakeApi() {
  const asked: string[] = []
  const fetch = vi.fn(async (input: string | URL | Request) => {
    const url = new URL(String(input), 'http://localhost')
    if (url.pathname === '/api/currencies') return Response.json(currencies)

    const value = url.searchParams.get('value')!
    asked.push(`${value} ${url.searchParams.get('currency')} ${url.searchParams.get('culture')}`)
    return Response.json({ value, currency: url.searchParams.get('currency'), figures: `$${value}`, words: `words for ${value}` })
  })
  vi.stubGlobal('fetch', fetch)
  return asked
}

async function settle(ms = 0) {
  await act(async () => {
    await vi.advanceTimersByTimeAsync(ms)
  })
}

describe('Cheque', () => {
  beforeEach(() => vi.useFakeTimers({ shouldAdvanceTime: true }))
  afterEach(() => {
    vi.useRealTimers()
    vi.unstubAllGlobals()
  })

  const setup = () => userEvent.setup({ advanceTimers: vi.advanceTimersByTime })

  it('waits for a pause in typing, then asks for the words once', async () => {
    const asked = fakeApi()
    const user = setup()
    render(<Cheque />)
    await settle()

    await user.type(screen.getByLabelText(/amount in figures/i), '1234.5')
    expect(asked).toEqual([])
    expect(screen.getByText(/waiting for you to pause/i)).toBeInTheDocument()

    await settle(defaultDelayMs)
    expect(asked).toEqual(['1234.5 AUD en-AU'])
    expect(screen.getByText(/written by hs.core.money/i)).toBeInTheDocument()
    expect(screen.getByRole('status')).toHaveTextContent('words for 1234.5')
  })

  it('asks on every keystroke when the delay is off', async () => {
    const asked = fakeApi()
    const user = setup()
    render(<Cheque />)
    await settle()

    await user.click(screen.getByRole('checkbox', { name: /keystroke delay/i }))
    await user.type(screen.getByLabelText(/amount in figures/i), '123')
    await settle()

    expect(asked).toEqual(['1 AUD en-AU', '12 AUD en-AU', '123 AUD en-AU'])
  })

  it('cleans what is typed, and allows no fraction of a yen', async () => {
    fakeApi()
    const user = setup()
    render(<Cheque />)
    await settle()

    const amount = screen.getByLabelText(/amount in figures/i)
    await user.type(amount, 'a1b2.345')
    expect(amount).toHaveValue('12.34')

    await user.selectOptions(screen.getByRole('combobox', { name: /currency/i }), 'JPY')
    expect(amount).toHaveValue('12')
  })

  it('rewrites the amount with the new language’s decimal separator, and asks in that language', async () => {
    const asked = fakeApi()
    const user = setup()
    render(<Cheque />)
    await settle()

    const amount = screen.getByLabelText(/amount in figures/i)
    await user.type(amount, '7.5')
    await user.click(screen.getByRole('radio', { name: 'Nederlands' }))
    expect(amount).toHaveValue('7,5')

    await settle(defaultDelayMs)
    expect(asked).toEqual(['7.5 AUD nl-NL'])
  })
})
