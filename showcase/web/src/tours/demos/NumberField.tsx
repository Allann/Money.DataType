import { useId } from 'react'
import { anatomy } from '../../numbers'

type Props = {
  label: string
  value: string
  onChange: (value: string) => void
  presets?: readonly string[]
}

/** A number to try, with presets. Flags anything that isn't a decimal Hs.Core can word. */
export function NumberField({ label, value, onChange, presets = [] }: Props) {
  const id = useId()
  const valid = anatomy(value) !== null

  return (
    <div className="number-field">
      <label htmlFor={id}>{label}</label>
      <input
        id={id}
        value={value}
        inputMode="decimal"
        spellCheck={false}
        autoComplete="off"
        aria-invalid={!valid}
        aria-describedby={valid ? undefined : `${id}-problem`}
        onChange={(event) => onChange(event.target.value)}
      />
      {!valid && (
        <p id={`${id}-problem`} className="field-problem">
          A decimal, such as 1234.56 or -7, up to 79228162514264337593543950335.
        </p>
      )}
      {presets.length > 0 && (
        <div className="presets" role="group" aria-label="Examples">
          {presets.map((preset) => (
            <button key={preset} type="button" className={preset === value ? 'chip selected' : 'chip'} onClick={() => onChange(preset)}>
              {preset.length > 12 ? `${preset.slice(0, 6)}…` : preset}
            </button>
          ))}
        </div>
      )}
    </div>
  )
}

/** The text, if it's a number the API will accept; otherwise null, so nothing is requested. */
export const validNumber = (text: string) => (anatomy(text) === null ? null : text.trim())
