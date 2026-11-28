import { useId, useState } from 'react'
import { useSearchParams } from 'react-router'
import { api } from '../../api'
import { useDebouncedValue } from '../../hooks/useDebouncedValue'
import { useQuery } from '../../hooks/useQuery'
import { sameDecimal, sumDecimals } from '../../numbers'
import { NumberField, validNumber } from './NumberField'
import { problemOf } from './useWords'

type Box = { id: string; step: string; title: string; detail: string }

const layers: readonly { name: string; note: string; boxes: readonly Box[] }[] = [
  {
    name: 'Showcase',
    note: 'React and a minimal API: input checking and JSON, no money logic',
    boxes: [
      { id: 'react', step: 'words', title: 'React app', detail: 'Tours, cheque' },
      { id: 'api', step: 'words', title: 'Hs.Showcase.Api', detail: 'Minimal API' },
    ],
  },
  {
    name: 'Hs.Core.Money',
    note: 'Money as a value: an amount in a currency',
    boxes: [
      { id: 'amount', step: 'amount', title: 'Amount', detail: 'Value + Currency' },
      { id: 'currency', step: 'minor-units', title: 'Currency', detail: 'Minor unit, Round' },
      { id: 'registry', step: 'registry', title: 'CurrencyRegistry', detail: 'ISO 4217, thread-safe' },
      { id: 'divide', step: 'divide', title: 'SafeDivide', detail: 'Lossless allocation' },
      { id: 'format', step: 'formatting', title: 'Format & parse', detail: 'C, I, F, W' },
      { id: 'json', step: 'serialisation', title: 'JSON & converters', detail: 'System.Text.Json' },
      { id: 'rate', step: 'exchange', title: 'ExchangeRate', detail: 'EUR/USD 1.2591' },
      { id: 'words', step: 'words', title: 'AmountWords', detail: 'MoneyLanguage' },
    ],
  },
  {
    name: 'Hs.Core',
    note: 'Numbers in words; knows nothing about money',
    boxes: [{ id: 'core', step: 'words', title: 'NumberToWords', detail: 'English, Dutch, Japanese' }],
  },
]

/** Step 1: the solution's layers, each depending only on the one below. Each box opens its step. */
export function ArchitectureMap() {
  const [, setParams] = useSearchParams()

  return (
    <div className="architecture" role="list" aria-label="Architecture, top layer first">
      {layers.map((layer, position) => (
        <section key={layer.name} className={`layer layer-${position}`} role="listitem" aria-label={layer.name}>
          <header>
            <h4>{layer.name}</h4>
            <p>{layer.note}</p>
          </header>
          <div className="layer-boxes">
            {layer.boxes.map((box) => (
              <button key={box.id} type="button" className="box" onClick={() => setParams({ step: box.step })}>
                <strong>{box.title}</strong>
                <span>{box.detail}</span>
              </button>
            ))}
          </div>
          {position < layers.length - 1 && (
            <div className="depends" aria-hidden="true">
              depends on ↓
            </div>
          )}
        </section>
      ))}
    </div>
  )
}

const roundingCurrencies = [
  { code: 'EUR', unit: 'hundredths' },
  { code: 'JPY', unit: 'whole yen' },
  { code: 'BHD', unit: 'thousandths' },
  { code: 'MGA', unit: 'fifths' },
] as const

function RoundingRow({ value, code, unit }: { value: string | null; code: string; unit: string }) {
  const query = useQuery(value === null ? null : `${value}|${code}`, (signal) => api.amount(value!, code, 'en-AU', signal))
  const problem = problemOf(query.error)

  return (
    <tr className={query.status === 'success' ? undefined : 'stale'}>
      <th scope="row">
        {code}
        <span className="muted">{unit}</span>
      </th>
      <td className="numeric">{problem ? '—' : (query.data?.value ?? '…')}</td>
      <td className="numeric">{problem ? <span className="problem">{problem}</span> : (query.data?.figures ?? '')}</td>
    </tr>
  )
}

/** Step 3: one value, rounded by four currencies with different minor units. */
export function RoundingDemo() {
  const [text, setText] = useState('1234.5678')
  const value = useDebouncedValue(validNumber(text), 250)

  return (
    <>
      <NumberField label="A value" value={text} onChange={setText} presets={['1234.5678', '0.125', '2.5', '0.3']} />
      <table className="rounding">
        <thead>
          <tr>
            <th scope="col">Currency</th>
            <th scope="col">new Amount(…).Value</th>
            <th scope="col">Formatted</th>
          </tr>
        </thead>
        <tbody>
          {roundingCurrencies.map((currency) => (
            <RoundingRow key={currency.code} value={value} code={currency.code} unit={currency.unit} />
          ))}
        </tbody>
      </table>
    </>
  )
}

/** Step 6: divide an amount and see that the shares add back up exactly. */
export function DivideDemo() {
  const id = useId()
  const [text, setText] = useState('100')
  const [currency, setCurrency] = useState('EUR')
  const [shares, setShares] = useState(3)
  const value = useDebouncedValue(validNumber(text), 250)

  const division = useQuery(value === null ? null : `${value}|${currency}|${shares}`, (signal) => api.divide(value!, currency, shares, signal))
  const problem = problemOf(division.error)
  const parts = division.data?.shares ?? []
  const largest = Math.max(...parts.map(Number), 0)
  // Added up here, exactly, rather than trusting the server's word for it.
  const sum = sumDecimals(parts)

  return (
    <>
      <NumberField label="Amount" value={text} onChange={setText} presets={['100', '0.05', '1', '10']} />
      <div className="row">
        <label className="field">
          <span>Currency</span>
          <select value={currency} onChange={(event) => setCurrency(event.target.value)}>
            <option value="EUR">EUR (cents)</option>
            <option value="JPY">JPY (whole yen)</option>
            <option value="BHD">BHD (fils, thousandths)</option>
          </select>
        </label>
        <label className="field" htmlFor={`${id}-shares`}>
          <span>
            Shares: <strong>{shares}</strong>
          </span>
          <input id={`${id}-shares`} type="range" min={2} max={12} value={shares} onChange={(event) => setShares(Number(event.target.value))} />
        </label>
      </div>
      {problem ? (
        <p className="problem">{problem}</p>
      ) : (
        <div className={division.status === 'success' ? 'shares' : 'shares stale'}>
          <ol>
            {parts.map((share, index) => (
              <li key={index}>
                <span className="bar" style={{ width: largest ? `${(Number(share) / largest) * 100}%` : '0' }} />
                <span className="numeric">{share}</span>
              </li>
            ))}
          </ol>
          {division.data && (
            <p className="sum">
              {parts.join(' + ')} = <strong className="numeric">{sum}</strong> {division.data.currency}
              {sameDecimal(sum, division.data.total) ? ': exactly the amount divided.' : `, but the amount was ${division.data.total}!`}
            </p>
          )}
        </div>
      )}
    </>
  )
}
