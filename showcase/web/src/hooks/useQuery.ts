import { useEffect, useRef, useState } from 'react'

export type Query<T> =
  | { status: 'idle'; data?: undefined; error?: undefined }
  | { status: 'loading'; data?: T; error?: undefined }
  | { status: 'success'; data: T; error?: undefined }
  | { status: 'error'; data?: T; error: unknown }

/**
 * Loads data for `key`, and again whenever `key` changes; a null key loads nothing.
 * A newer key aborts the older request, so a slow answer can never overwrite a newer one.
 * While loading, the previous data stays available, so the page doesn't flicker.
 */
export function useQuery<T>(key: string | null, load: (signal: AbortSignal) => Promise<T>): Query<T> {
  const [query, setQuery] = useState<Query<T>>({ status: 'idle' })
  const latestLoad = useRef(load)

  useEffect(() => {
    latestLoad.current = load
  })

  useEffect(() => {
    if (key === null) {
      setQuery({ status: 'idle' })
      return
    }

    const controller = new AbortController()
    setQuery((previous) => ({ status: 'loading', data: previous.data }))
    latestLoad.current(controller.signal).then(
      (data) => setQuery({ status: 'success', data }),
      (error: unknown) => {
        if (!controller.signal.aborted) setQuery((previous) => ({ status: 'error', error, data: previous.data }))
      },
    )
    return () => controller.abort()
  }, [key])

  return query
}
