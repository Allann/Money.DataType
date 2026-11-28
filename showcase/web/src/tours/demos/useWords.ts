import { api, ApiError } from '../../api'
import { useDebouncedValue } from '../../hooks/useDebouncedValue'
import { useQuery } from '../../hooks/useQuery'

const demoDelayMs = 250

/** The words for an invariant number in a culture, from Hs.Core through the API, once typing pauses. */
export function useWords(number: string | null, culture: string) {
  const key = useDebouncedValue(number === null ? null : `${number}|${culture}`, demoDelayMs)
  const query = useQuery(key, (signal) => {
    const [value = '', language = ''] = key!.split('|')
    return api.words(value, language, signal)
  })
  return { ...query, words: query.data?.words, problem: problemOf(query.error) }
}

export function problemOf(error: unknown): string | undefined {
  if (error === undefined) return undefined
  return error instanceof ApiError ? error.reason : 'The API isn’t answering: start it with dotnet run --project showcase/Hs.Showcase.Api'
}
