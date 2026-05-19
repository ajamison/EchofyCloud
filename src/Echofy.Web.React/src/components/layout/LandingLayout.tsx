import { Link } from 'react-router-dom'
import { Outlet } from 'react-router-dom'

export function LandingLayout() {
  return (
    <div className="min-h-screen bg-white text-slate-800">
      <header className="sticky top-0 z-50 border-b border-slate-100 bg-white/90 backdrop-blur">
        <div className="mx-auto flex max-w-6xl items-center justify-between px-6 py-4">
          <Link to="/" className="text-2xl font-bold text-primary">
            Echofy
          </Link>
          <nav className="hidden items-center gap-8 text-sm font-medium text-slate-600 md:flex">
            <a href="#features" className="hover:text-primary transition-colors">Features</a>
            <a href="#how-it-works" className="hover:text-primary transition-colors">How It Works</a>
            <a href="#pricing" className="hover:text-primary transition-colors">Pricing</a>
          </nav>
          <div className="flex items-center gap-3">
            <Link
              to="/login"
              className="text-sm font-medium text-slate-600 hover:text-primary transition-colors"
            >
              Sign In
            </Link>
            <Link
              to="/register"
              className="rounded-lg bg-primary px-4 py-2 text-sm font-semibold text-white hover:bg-primary-600 transition-colors"
            >
              Get Started
            </Link>
          </div>
        </div>
      </header>
      <main>
        <Outlet />
      </main>
      <footer className="border-t border-slate-100 bg-slate-50 py-12">
        <div className="mx-auto max-w-6xl px-6">
          <div className="flex flex-col items-center justify-between gap-4 md:flex-row">
            <span className="text-xl font-bold text-primary">Echofy</span>
            <p className="text-sm text-slate-500">
              &copy; {new Date().getFullYear()} Echofy. All rights reserved.
            </p>
            <div className="flex gap-6 text-sm text-slate-500">
              <a href="#" className="hover:text-primary transition-colors">Privacy</a>
              <a href="#" className="hover:text-primary transition-colors">Terms</a>
              <a href="#" className="hover:text-primary transition-colors">Contact</a>
            </div>
          </div>
        </div>
      </footer>
    </div>
  )
}
