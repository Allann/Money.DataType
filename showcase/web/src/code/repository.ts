/** Where the solution lives, for "view on GitHub" links. Kept apart from sources.ts so linking doesn't bundle the source. */
export const repositoryUrl = 'https://github.com/Allann/Money.DataType'

export const sourceUrl = (path: string, from?: number, to?: number) =>
  `${repositoryUrl}/blob/main/${path}${from ? `#L${from}${to && to !== from ? `-L${to}` : ''}` : ''}`
