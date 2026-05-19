import { Outlet } from 'react-router-dom'
import { useTheme } from '../../lib/theme'
import { Sun, Moon } from 'lucide-react'

export function AuthLayout() {
  const { dark, toggle } = useTheme()

  return (
    <div className="relative flex min-h-screen items-center justify-center bg-slate-100 dark:bg-slate-900">
      <button
        onClick={toggle}
        className="absolute right-4 top-4 rounded-lg p-2 text-slate-500 hover:bg-slate-200 hover:text-slate-700 dark:text-slate-400 dark:hover:bg-slate-800 dark:hover:text-slate-200"
        title={dark ? 'Switch to light mode' : 'Switch to dark mode'}
      >
        {dark ? <Sun size={18} /> : <Moon size={18} />}
      </button>
      <div className="w-full max-w-md">
        <div className="mb-8 text-center">
          <h1 className="text-3xl font-bold text-slate-800 dark:text-slate-100">Echofy</h1>
          <p className="mt-1 text-sm text-slate-500 dark:text-slate-400">Business management platform</p>
        </div>
        <div className="rounded-2xl bg-white p-8 shadow-sm ring-1 ring-slate-200 dark:bg-slate-800 dark:ring-slate-700">
          <Outlet />
        </div>
      </div>
    </div>
  )
}
