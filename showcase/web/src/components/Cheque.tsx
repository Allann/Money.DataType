import { useId, useMemo, useRef, useState } from 'react'
import { api, ApiError, languages, type CurrencyOption } from '../api'
import { useDebouncedValue } from '../hooks/useDebouncedValue'
import { useQuery } from '../hooks/useQuery'
import { decimalSeparator, readAmount } from '../numbers'

/** The cheque's printed wording, in each language the showcase writes. */
const printed: Record<string, { pay: string; sum: string; date: string; signature: string; specimen: string; payee: string }> = {
  'en-AU': { pay: 'Pay', sum: 'The sum of', date: 'Date', signature: 'Signature', specimen: 'Specimen · not negotiable', payee: 'A. Future Employer' },
  'nl-NL': { pay: 'Betaal aan', sum: 'De som van', date: 'Datum', signature: 'Handtekening', specimen: 'Specimen · niet verhandelbaar', payee: 'Een toekomstige werkgever' },
  'ja-JP': { pay: '支払先', sum: '金額', date: '振出日', signature: '署名', specimen: '見本 · SPECIMEN', payee: '未来の雇用主 様' },
}

const defaultCurrency: CurrencyOption = { code: 'AUD', name: 'Australian Dollar', symbol: '$', decimalDigits: 2, minorUnit: 0.01 }

export const defaultDelayMs = 500

function describeError(error: unknown): string {
  if (error instanceof ApiError) return error.reason
  return 'The API isn’t answering. Start it with: dotnet run --project showcase/Hs.Showcase.Api'
}

/**
 * A cheque that writes its amount in words as you type. Each keystroke is cleaned in the browser; once typing
 * pauses for the keystroke delay, the amount goes to Hs.Core.Money, which rounds it to the currency's minor unit
 * and writes it out. A newer request cancels an older one, so the words never lag behind the figures.
 */
export function Cheque() {
  const id = useId()
  const [culture, setCulture] = useState('en-AU')
  const [currencyCode, setCurrencyCode] = useState(defaultCurrency.code)
  const [typed, setTyped] = useState('')
  const [payee, setPayee] = useState('')
  const [delayOn, setDelayOn] = useState(true)
  const [delayMs, setDelayMs] = useState(defaultDelayMs)
  const [keystrokes, setKeystrokes] = useState(0)
  const requests = useRef(0)

  const currencies = useQuery('currencies', api.currencies)
  const currency = currencies.data?.find((option) => option.code === currencyCode) ?? defaultCurrency
  const separator = decimalSeparator(culture)
  const amount = readAmount(typed, separator, currency.decimalDigits)

  const wanted = amount.invariant === '' ? null : `${amount.invariant}|${currency.code}|${culture}`
  const settled = useDebouncedValue(wanted, delayOn ? delayMs : 0)
  const words = useQuery(settled, (signal) => {
    requests.current++
    const [value = '', code = '', language = ''] = settled!.split('|')
    return api.amount(value, code, language, signal)
  })

  const text = printed[culture] ?? printed['en-AU']!
  const date = useMemo(() => new Intl.DateTimeFormat(culture, { dateStyle: 'medium' }).format(new Date()), [culture])
  const waiting = wanted !== settled
  const current = wanted !== null && !waiting && words.status === 'success'

  function changeCurrency(code: string) {
    const next = currencies.data?.find((option) => option.code === code) ?? defaultCurrency
    setCurrencyCode(code)
    setTyped((previous) => readAmount(previous, separator, next.decimalDigits).display)
  }

  function changeCulture(next: string) {
    // Keep the amount, but rewrite it with the new culture's decimal separator.
    setTyped((previous) => readAmount(previous, separator, currency.decimalDigits).display.replace(separator, decimalSeparator(next)))
    setCulture(next)
  }

  const status =
    wanted === null
      ? 'Type an amount in the box.'
      : waiting
        ? 'Waiting for you to pause…'
        : words.status === 'loading'
          ? 'Writing…'
          : words.status === 'error'
            ? describeError(words.error)
            : 'Written by Hs.Core.Money.'

  return (
    <div className="cheque-layout">
      <div className="cheque-controls" role="group" aria-label="Cheque options">
        <fieldset className="segmented">
          <legend>Language</legend>
          {languages.map((language) => (
            <label key={language.culture} className={language.culture === culture ? 'selected' : undefined}>
              <input type="radio" name={`${id}-language`} value={language.culture} checked={language.culture === culture} onChange={() => changeCulture(language.culture)} />
              <span lang={language.culture}>{language.nativeName}</span>
            </label>
          ))}
        </fieldset>

        <label className="field">
          <span>Currency</span>
          <select value={currency.code} onChange={(event) => changeCurrency(event.target.value)}>
            {(currencies.data ?? [defaultCurrency]).map((option) => (
              <option key={option.code} value={option.code}>
                {option.code} · {option.name}
              </option>
            ))}
          </select>
        </label>
      </div>

      <article className="cheque" lang={culture} aria-label="Cheque">
        <div className="cheque-watermark" aria-hidden="true">
          {text.specimen}
        </div>
        <header className="cheque-top">
          <div className="cheque-bank">
            <span className="cheque-bank-mark" aria-hidden="true">
              ¤
            </span>
            <span>
              Showcase Savings Bank
              <small>Branch 0001 · Hs.Core.Money</small>
            </span>
          </div>
          <div className="cheque-date">
            <span>{text.date}</span>
            <strong>{date}</strong>
          </div>
        </header>

        <div className="cheque-pay">
          <label htmlFor={`${id}-payee`}>{text.pay}</label>
          <input id={`${id}-payee`} className="cheque-line" value={payee} placeholder={text.payee} onChange={(event) => setPayee(event.target.value)} autoComplete="off" />
        </div>

        <div className="cheque-amount">
          <label htmlFor={`${id}-amount`} className="visually-hidden">
            Amount in figures ({currency.code})
          </label>
          <span className="cheque-symbol" aria-hidden="true">
            {currency.symbol}
          </span>
          <input
            id={`${id}-amount`}
            data-autofocus
            inputMode={currency.decimalDigits > 0 ? 'decimal' : 'numeric'}
            autoComplete="off"
            value={amount.display}
            placeholder={currency.decimalDigits > 0 ? `0${separator}${'0'.repeat(currency.decimalDigits)}` : '0'}
            onChange={(event) => {
              setKeystrokes((count) => count + 1)
              setTyped(event.target.value)
            }}
          />
        </div>

        <div className="cheque-sum">
          <span className="cheque-sum-label">{text.sum}</span>
          <output className={`cheque-words${current ? '' : ' stale'}`} htmlFor={`${id}-amount`} aria-live="polite">
            {wanted === null ? '' : (words.data?.words ?? '')}
          </output>
        </div>

        <footer className="cheque-bottom">
          <span className="cheque-micr" aria-hidden="true">
            ⑈000123⑈ ⑆062000⑆ 1999 2026⑈
          </span>
          <span className="cheque-signature">
            <span className="cheque-signed" aria-hidden="true">
              {payee ? 'Hs' : ''}
            </span>
            {text.signature}
          </span>
        </footer>
      </article>

      <div className="cheque-status">
        <p className={words.status === 'error' && !waiting ? 'status error' : 'status'}>
          <span className={`dot ${waiting ? 'waiting' : words.status}`} aria-hidden="true" />
          {status}
          {current && words.data && <span className="figures"> {words.data.figures}</span>}
        </p>

        <div className="delay">
          <label className="switch">
            <input type="checkbox" checked={delayOn} onChange={(event) => setDelayOn(event.target.checked)} />
            <span>Keystroke delay</span>
          </label>
          <label className="delay-range">
            <span className="visually-hidden">Delay in milliseconds</span>
            <input type="range" min={100} max={1500} step={50} value={delayMs} disabled={!delayOn} onChange={(event) => setDelayMs(Number(event.target.value))} />
            <span className="delay-value">{delayOn ? `${delayMs} ms` : 'off'}</span>
          </label>
          <p className="delay-count">
            <strong>{keystrokes}</strong> keystrokes, <strong>{requests.current}</strong> requests
          </p>
        </div>
      </div>
    </div>
  )
}
