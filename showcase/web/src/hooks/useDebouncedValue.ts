import { useEffect, useState } from 'react'

/**
 * The value, once it has stopped changing for `delayMs`: typing "1234" makes one request, not four.
 * A delay of 0 passes every change straight through.
 */
export function useDebouncedValue<T>(value: T, delayMs: number): T {
  const [settled, setSettled] = useState(value)

  useEffect(() => {
    if (delayMs <= 0) {
      setSettled(value)
      return
    }
    const timer = setTimeout(() => setSettled(value), delayMs)
    return () => clearTimeout(timer)
  }, [value, delayMs])

  return delayMs <= 0 ? value : settled
}
