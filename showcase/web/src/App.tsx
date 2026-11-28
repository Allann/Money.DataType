import { lazy, Suspense } from 'react'
import { BrowserRouter, Link, Route, Routes } from 'react-router'
import { Landing } from './pages/Landing'

// The tours bundle the C# source and a highlighter, so they load only when visited.
const CoreTour = lazy(() => import('./pages/TourPages').then((module) => ({ default: module.CoreTourPage })))
const MoneyTour = lazy(() => import('./pages/TourPages').then((module) => ({ default: module.MoneyTourPage })))

function NotFound() {
  return (
    <main className="not-found">
      <h1>Nothing here</h1>
      <p>
        <Link to="/">Back to the start</Link>
      </p>
    </main>
  )
}

export function AppRoutes() {
  return (
    <Suspense fallback={<p className="loading">Loading…</p>}>
      <Routes>
        <Route path="/" element={<Landing />} />
        <Route path="/cheque" element={<Landing chequeOpen />} />
        <Route path="/tour/core" element={<CoreTour />} />
        <Route path="/tour/money" element={<MoneyTour />} />
        <Route path="*" element={<NotFound />} />
      </Routes>
    </Suspense>
  )
}

export function App() {
  return (
    <BrowserRouter>
      <AppRoutes />
    </BrowserRouter>
  )
}
