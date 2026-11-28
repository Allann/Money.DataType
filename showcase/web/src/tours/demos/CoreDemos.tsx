import { useState } from 'react'
import { languages } from '../../api'
import { anatomy, decimalMax, splitDigitGroups } from '../../numbers'
import { NumberField, validNumber } from './NumberField'
import { useWords } from './useWords'

const english = ['zero', 'one', 'two', 'three', 'four', 'five', 'six', 'seven', 'eight', 'nine']
const thousands = ['', 'thousand', 'million', 'billion', 'trillion', 'quadrillion', 'quintillion', 'sextillion', 'septillion', 'octillion']
const tenThousands = ['', '万', '億', '兆', '京', '垓', '秭', '穣']

function Words({ number, culture }: { number: string | null; culture: string }) {
  const { words, problem, status } = useWords(number, culture)
  if (problem) return <p className="words problem">{problem}</p>
  return (
    <p className={status === 'success' ? 'words' : 'words stale'} lang={culture}>
      {words ?? '…'}
    </p>
  )
}

/** Step 3: what Signed, Unsigned and FractionDigits do to a number before any language sees it. */
export function AnatomyDemo() {
  const [text, setText] = useState('-12.05')
  const parts = anatomy(text)

  return (
    <>
      <NumberField label="A number" value={text} onChange={setText} presets={['-12.05', '3.14159', '0.5', '1000']} />
      {parts && (
        <ol className="pipeline">
          <li>
            <span className="pipeline-label">Signed</span>
            {parts.negative ? <code>language.Negative(…)</code> : <span className="muted">not negative: nothing to add</span>}
          </li>
          <li>
            <span className="pipeline-label">Whole</span>
            <code>language.Whole({parts.whole.toString()})</code>
          </li>
          <li>
            <span className="pipeline-label">FractionDigits</span>
            {parts.fractionDigits.length === 0 ? (
              <span className="muted">no fraction</span>
            ) : (
              <span className="digits">
                {parts.fractionDigits.map((digit, index) => (
                  <code key={index} title={english[digit]}>
                    {digit}
                  </code>
                ))}
              </span>
            )}
          </li>
        </ol>
      )}
      <Words number={validNumber(text)} culture="en-AU" />
    </>
  )
}

function Groups({ whole, size, names }: { whole: bigint; size: 1000 | 10000; names: readonly string[] }) {
  const groups = splitDigitGroups(whole, size)
  if (groups.length === 0) return <p className="muted">No groups: zero has its own word.</p>
  return (
    <ol className="groups">
      {groups.map((group) => (
        <li key={group.scale}>
          <span className="group-value">{group.value}</span>
          <span className="group-scale">{names[group.scale] || 'units'}</span>
        </li>
      ))}
    </ol>
  )
}

/** Step 5: the same number in groups of a thousand and of ten thousand, and the words each produces. */
export function GroupsDemo() {
  const [text, setText] = useState('123456789')
  const parts = anatomy(text)
  const number = validNumber(text)

  return (
    <>
      <NumberField label="A whole number" value={text} onChange={setText} presets={['123456789', '2000300000', '10000', decimalMax]} />
      {parts && (
        <>
          <h4>
            In thousands <span className="muted">(English, Dutch)</span>
          </h4>
          <Groups whole={parts.whole} size={1000} names={thousands} />
          <Words number={number} culture="en-AU" />
          <Words number={number} culture="nl-NL" />
          <h4>
            In ten thousands <span className="muted">(Japanese)</span>
          </h4>
          <Groups whole={parts.whole} size={10000} names={tenThousands} />
          <Words number={number} culture="ja-JP" />
        </>
      )}
    </>
  )
}

/** Any number, in every language at once. */
export function CompareDemo() {
  const [text, setText] = useState('1005')
  const number = validNumber(text)

  return (
    <div className="compare">
      <NumberField label="A number" value={text} onChange={setText} presets={['21', '105', '1005', '10000', '22.5', '-3.14', decimalMax]} />
      <dl className="compare-list">
        {languages.map((language) => (
          <div key={language.culture}>
            <dt>
              {language.name} <span className="muted">{language.culture}</span>
            </dt>
            <dd>
              <Words number={number} culture={language.culture} />
            </dd>
          </div>
        ))}
      </dl>
    </div>
  )
}
