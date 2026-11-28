/**
 * Cuts excerpts out of source files by what the code says, not by line number, so an excerpt
 * follows its code when lines are added above it, and fails loudly (in a test) when the code is renamed.
 */

export type ExcerptSpec = {
  /** The file, relative to the repository root, e.g. "src/Hs.Core/NumberToWords.cs". */
  file: string
  /** Text on the first line of the excerpt, e.g. "public static Words ToWords". */
  from: string
  /** Text on the last line. Without it, the excerpt ends where the declaration on the first line does. */
  to?: string
  /** Text on lines to highlight. */
  highlight?: string[]
  /** Leave out the XML doc comment and attributes above the first line. */
  withoutDocs?: boolean
}

export type Excerpt = {
  file: string
  code: string
  /** The 1-based line in the file where the excerpt starts. */
  startLine: number
  endLine: number
  /** 1-based lines within the excerpt to highlight. */
  highlighted: ReadonlySet<number>
}

export class ExcerptError extends Error {
  constructor(spec: ExcerptSpec, problem: string) {
    super(`${spec.file}: ${problem}`)
    this.name = 'ExcerptError'
  }
}

const isDocOrAttribute = (line: string) => /^\s*(\/\/\/|\[)/.test(line)

const count = (text: string, character: string) => text.split(character).length - 1

/** The index of the line that ends the declaration starting at `start`: its closing brace, or its semicolon. */
function endOfDeclaration(lines: readonly string[], start: number): number {
  let depth = 0
  let opened = false
  for (let index = start; index < lines.length; index++) {
    const line = (lines[index] ?? '').replace(/\/\/.*$/, '')
    depth += count(line, '{') - count(line, '}')
    opened ||= line.includes('{')
    if (opened ? depth <= 0 : line.trimEnd().endsWith(';')) return index
  }
  return lines.length - 1
}

function dedent(lines: readonly string[]): string[] {
  const indents = lines.filter((line) => line.trim() !== '').map((line) => line.length - line.trimStart().length)
  const common = Math.min(...indents)
  return lines.map((line) => line.slice(common))
}

export function cutExcerpt(source: string, spec: ExcerptSpec): Excerpt {
  const lines = source.replace(/\r\n/g, '\n').split('\n')
  const first = lines.findIndex((line) => line.includes(spec.from))
  if (first < 0) throw new ExcerptError(spec, `no line contains "${spec.from}"`)

  const last = spec.to === undefined ? endOfDeclaration(lines, first) : lines.findIndex((line, index) => index >= first && line.includes(spec.to!))
  if (last < 0) throw new ExcerptError(spec, `no line after "${spec.from}" contains "${spec.to}"`)

  let start = first
  while (!spec.withoutDocs && start > 0 && isDocOrAttribute(lines[start - 1] ?? '')) start--

  const excerpt = dedent(lines.slice(start, last + 1))
  const highlighted = new Set<number>()
  for (const text of spec.highlight ?? []) {
    const line = excerpt.findIndex((candidate) => candidate.includes(text))
    if (line < 0) throw new ExcerptError(spec, `no line in the excerpt contains "${text}" to highlight`)
    highlighted.add(line + 1)
  }

  return { file: spec.file, code: excerpt.join('\n'), startLine: start + 1, endLine: last + 1, highlighted }
}
