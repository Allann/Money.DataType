import { Link, useNavigate } from 'react-router'
import { repositoryUrl } from '../code/repository'
import { Cheque } from '../components/Cheque'
import { Modal } from '../components/Modal'

// Each is a test in tests/Hs.Core.Money.Tests/ReadmeExamplesSpec.cs, so the page can't claim what the code doesn't do.
const samples = [
  { code: 'new Amount(1234.5m, "EUR").ToString("W", enUS)', words: 'one thousand two hundred and thirty-four euros and fifty cents', lang: 'en' },
  { code: 'new Amount(1234.5m, "EUR").ToString("W", nlNL)', words: 'duizend tweehonderdvierendertig euro en vijftig cent', lang: 'nl' },
  { code: 'new Amount(1234.56m, "JPY").ToString("W", jaJP)', words: '千二百三十五円', lang: 'ja' },
]

function TourIcon() {
  return (
    <svg viewBox="0 0 48 48" aria-hidden="true">
      <path d="M17 15 8 24l9 9M31 15l9 9-9 9M27 11l-6 26" />
    </svg>
  )
}

function ChequeIcon() {
  return (
    <svg viewBox="0 0 48 48" aria-hidden="true">
      <rect x="5" y="12" width="38" height="24" rx="3" />
      <path d="M11 21h14M11 28h20M33 21h4" />
    </svg>
  )
}

function LayersIcon() {
  return (
    <svg viewBox="0 0 48 48" aria-hidden="true">
      <path d="m24 8 17 8-17 8-17-8 17-8ZM7 24l17 8 17-8M7 32l17 8 17-8" />
    </svg>
  )
}

/** The front page: what the solution is, and the three ways to explore it. The cheque opens over it at /cheque. */
export function Landing({ chequeOpen = false }: { chequeOpen?: boolean }) {
  const navigate = useNavigate()

  return (
    <div className="landing">
      <header className="hero">
        <p className="eyebrow">Hs.Core · Hs.Core.Money · .NET 10</p>
        <h1>
          Money that knows its currency, and numbers that <em>say themselves</em>.
        </h1>
        <p className="hero-lede">
          Two small C# libraries, rebuilt in a functional style: immutable values, pure functions, explicit domain
          types. Read how they work, or watch them write a cheque.
        </p>
        <ul className="samples" aria-label="Examples">
          {samples.map((sample) => (
            <li key={sample.lang}>
              <code>{sample.code}</code>
              <span lang={sample.lang}>{sample.words}</span>
            </li>
          ))}
        </ul>
      </header>

      <main>
        <h2 className="visually-hidden">Explore</h2>
        <div className="features">
          <Link to="/tour/core" className="feature">
            <span className="feature-icon">
              <TourIcon />
            </span>
            <span className="feature-kicker">Code tour · 9 steps</span>
            <strong>How Hs.Core writes numbers in words</strong>
            <span>
              From one extension method to three grammars: English “and”, Dutch compound words, Japanese groups of
              four. Real source, with live examples.
            </span>
            <span className="feature-go">Start the tour →</span>
          </Link>

          <Link to="/cheque" className="feature feature-accent">
            <span className="feature-icon">
              <ChequeIcon />
            </span>
            <span className="feature-kicker">Try it</span>
            <strong>Write a cheque</strong>
            <span>
              Type an amount and see it written out in words as you go, in English, Dutch or Japanese, with a
              keystroke delay so it waits until you pause.
            </span>
            <span className="feature-go">Open the cheque →</span>
          </Link>

          <Link to="/tour/money" className="feature">
            <span className="feature-icon">
              <LayersIcon />
            </span>
            <span className="feature-kicker">Architecture tour · 10 steps</span>
            <strong>Inside Hs.Core.Money</strong>
            <span>
              Amounts as values, minor units from yen to fifths, a thread-safe registry, lossless division,
              formatting, JSON, and the decisions behind them.
            </span>
            <span className="feature-go">Start the tour →</span>
          </Link>
        </div>

        <dl className="facts">
          <div>
            <dt>Languages</dt>
            <dd>3</dd>
          </div>
          <div>
            <dt>ISO 4217 currencies</dt>
            <dd>checked against the official list</dd>
          </div>
          <div>
            <dt>Hs.Core line coverage</dt>
            <dd>100%</dd>
          </div>
          <div>
            <dt>Property-based tests</dt>
            <dd>CsCheck</dd>
          </div>
        </dl>
      </main>

      <footer className="site-footer">
        <span>React and an ASP.NET Core minimal API over the libraries. Started in 1999, rebuilt in 2026.</span>
        <a href={repositoryUrl} target="_blank" rel="noreferrer">
          Source on GitHub ↗
        </a>
      </footer>

      {chequeOpen && (
        <Modal labelledBy="cheque-title" onClose={() => navigate('/')}>
          <div className="cheque-dialog">
            <h2 id="cheque-title">Write a cheque</h2>
            <p className="cheque-intro">
              Type in the amount box. The browser cleans each keystroke; when you pause, Hs.Core.Money rounds the
              amount to the currency's minor unit and writes it out.
            </p>
            <Cheque />
          </div>
        </Modal>
      )}
    </div>
  )
}
