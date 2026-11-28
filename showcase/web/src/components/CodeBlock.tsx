import { useEffect, useMemo, useState, type CSSProperties } from 'react'
import type { ExcerptSpec } from '../code/excerpt'
import { loadHighlighter, toHtml } from '../code/highlighter'
import { sourceUrl } from '../code/repository'
import { excerpt } from '../code/sources'

type Props = { spec: ExcerptSpec; caption?: string }

/** An excerpt of the solution's real source, with its path, line numbers and a link to it on GitHub. */
export function CodeBlock({ spec, caption }: Props) {
  const cut = useMemo(() => excerpt(spec), [spec])
  const [html, setHtml] = useState<string | null>(null)

  useEffect(() => {
    let current = true
    setHtml(null)
    loadHighlighter().then((shiki) => {
      if (current) setHtml(toHtml(shiki, cut.code, cut.highlighted))
    })
    return () => {
      current = false
    }
  }, [cut])

  const style = { '--line-offset': cut.startLine - 1 } as CSSProperties

  return (
    <figure className="code" style={style}>
      <figcaption className="code-header">
        <span className="code-path">{cut.file}</span>
        <a href={sourceUrl(cut.file, cut.startLine, cut.endLine)} target="_blank" rel="noreferrer">
          lines {cut.startLine}–{cut.endLine}
          <span aria-hidden="true"> ↗</span>
        </a>
      </figcaption>
      {caption && <p className="code-caption">{caption}</p>}
      {html ? (
        <div className="code-body" dangerouslySetInnerHTML={{ __html: html }} />
      ) : (
        <div className="code-body">
          <pre className="shiki">
            <code>
              {cut.code.split('\n').map((line, index) => (
                <span key={index} className={cut.highlighted.has(index + 1) ? 'line highlighted' : 'line'}>
                  {line}
                  {'\n'}
                </span>
              ))}
            </code>
          </pre>
        </div>
      )}
    </figure>
  )
}
