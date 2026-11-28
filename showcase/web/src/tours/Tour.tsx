import { useEffect, useRef, type ReactNode } from 'react'
import { Link, useSearchParams } from 'react-router'
import type { ExcerptSpec } from '../code/excerpt'
import { sourceUrl } from '../code/repository'
import { CodeBlock } from '../components/CodeBlock'

export type TourStep = {
  /** Used in the URL: /tour/core?step=groups. */
  id: string
  title: string
  /** One sentence: the point of the step. */
  lede: string
  body: ReactNode
  /** Something to try, beside the explanation. */
  demo?: ReactNode
  code?: readonly { spec: ExcerptSpec; caption?: string }[]
  /** The architecture decision record behind the step, in docs/adr. */
  adr?: { title: string; file: string }
}

export type TourDefinition = {
  title: string
  kicker: string
  steps: readonly TourStep[]
  /** Where to go after the last step. */
  next: { to: string; label: string }
}

const isTyping = (target: EventTarget | null) =>
  target instanceof HTMLElement && (target.isContentEditable || ['INPUT', 'SELECT', 'TEXTAREA'].includes(target.tagName))

/** A guided walk through real code: one idea per step, with the source it lives in and something to try. */
export function Tour({ tour }: { tour: TourDefinition }) {
  const [params, setParams] = useSearchParams()
  const index = Math.max(0, tour.steps.findIndex((step) => step.id === params.get('step')))
  const step = tour.steps[index]!
  const heading = useRef<HTMLHeadingElement>(null)
  const firstRender = useRef(true)

  const goTo = (next: number) => {
    const target = tour.steps[next]
    if (target) setParams({ step: target.id })
  }

  useEffect(() => {
    if (firstRender.current) {
      firstRender.current = false
      return
    }
    heading.current?.focus()
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }, [index])

  useEffect(() => {
    function onKeyDown(event: KeyboardEvent) {
      if (isTyping(event.target) || event.altKey || event.ctrlKey || event.metaKey) return
      if (event.key === 'ArrowRight') goTo(index + 1)
      if (event.key === 'ArrowLeft') goTo(index - 1)
    }
    document.addEventListener('keydown', onKeyDown)
    return () => document.removeEventListener('keydown', onKeyDown)
  })

  return (
    <div className="tour">
      <nav className="tour-nav" aria-label={`${tour.title} steps`}>
        <Link to="/" className="back">
          ← All features
        </Link>
        <p className="tour-kicker">{tour.kicker}</p>
        <h1 className="tour-title">{tour.title}</h1>
        <ol className="tour-steps">
          {tour.steps.map((candidate, position) => (
            <li key={candidate.id}>
              <button type="button" aria-current={position === index ? 'step' : undefined} onClick={() => goTo(position)}>
                <span className="tour-step-number">{position + 1}</span>
                {candidate.title}
              </button>
            </li>
          ))}
        </ol>
        <p className="tour-hint">
          Tip: <kbd>←</kbd> <kbd>→</kbd> move between steps.
        </p>
      </nav>

      <main className="tour-main">
        <div className="tour-progress" aria-hidden="true">
          <div style={{ width: `${((index + 1) / tour.steps.length) * 100}%` }} />
        </div>
        <article className="tour-step" aria-labelledby="tour-step-title">
          <p className="tour-step-count">
            Step {index + 1} of {tour.steps.length}
          </p>
          <h2 id="tour-step-title" ref={heading} tabIndex={-1}>
            {step.title}
          </h2>
          <p className="lede">{step.lede}</p>

          <div className={step.demo ? 'tour-content with-demo' : 'tour-content'}>
            <div className="prose">
              {step.body}
              {step.adr && (
                <p className="adr">
                  <span>Decision record</span>
                  <a href={sourceUrl(`docs/adr/${step.adr.file}`)} target="_blank" rel="noreferrer">
                    {step.adr.title} ↗
                  </a>
                </p>
              )}
            </div>
            {step.demo && <aside className="demo">{step.demo}</aside>}
          </div>

          {step.code?.map(({ spec, caption }) => <CodeBlock key={`${spec.file}:${spec.from}`} spec={spec} caption={caption} />)}
        </article>

        <footer className="tour-pager">
          <button type="button" className="button secondary" onClick={() => goTo(index - 1)} disabled={index === 0}>
            ← Previous
          </button>
          {index < tour.steps.length - 1 ? (
            <button type="button" className="button" onClick={() => goTo(index + 1)}>
              Next: {tour.steps[index + 1]!.title} →
            </button>
          ) : (
            <Link to={tour.next.to} className="button">
              {tour.next.label} →
            </Link>
          )}
        </footer>
      </main>
    </div>
  )
}
