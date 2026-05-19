import { useState } from 'react'
import { Outlet } from 'react-router-dom'
import { Menu, X, Bell, Search, LogOut, PanelLeftClose, PanelLeftOpen, Sun, Moon } from 'lucide-react'
import { Sidebar } from './Sidebar'
import { useAuth } from '../../lib/auth'
import { useTheme } from '../../lib/theme'

export function Layout() {
  const [sidebarOpen, setSidebarOpen] = useState(false)
  const [collapsed, setCollapsed] = useState(false)
  const { user, logout } = useAuth()
  const { dark, toggle } = useTheme()

  return (
    <div className="flex h-screen overflow-hidden bg-slate-100 dark:bg-slate-900">
      {/* Desktop sidebar */}
      <div className="hidden lg:flex lg:flex-shrink-0">
        <Sidebar collapsed={collapsed} />
      </div>

      {/* Mobile sidebar overlay */}
      {sidebarOpen && (
        <div className="fixed inset-0 z-40 lg:hidden">
          <div className="absolute inset-0 bg-slate-900/60" onClick={() => setSidebarOpen(false)} />
          <div className="relative flex h-full w-64">
            <Sidebar onClose={() => setSidebarOpen(false)} />
            <button
              className="absolute right-2 top-2 text-slate-400 hover:text-white"
              onClick={() => setSidebarOpen(false)}
            >
              <X size={20} />
            </button>
          </div>
        </div>
      )}

      {/* Main content */}
      <div className="flex flex-1 flex-col overflow-hidden">
        {/* Top bar */}
        <header className="flex h-16 items-center gap-4 border-b border-slate-200 bg-white px-6 dark:border-slate-700 dark:bg-slate-800">
          <button
            className="lg:hidden text-slate-500 hover:text-slate-700 dark:text-slate-400 dark:hover:text-slate-200"
            onClick={() => setSidebarOpen(true)}
          >
            <Menu size={20} />
          </button>
          <button
            className="hidden lg:block text-slate-500 hover:text-slate-700 dark:text-slate-400 dark:hover:text-slate-200"
            onClick={() => setCollapsed((c) => !c)}
            title={collapsed ? 'Expand sidebar' : 'Collapse sidebar'}
          >
            {collapsed ? <PanelLeftOpen size={20} /> : <PanelLeftClose size={20} />}
          </button>
          <div className="flex flex-1 items-center gap-3">
            <div className="relative hidden sm:block">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" size={15} />
              <input
                type="text"
                placeholder="Search…"
                className="w-64 rounded-lg border border-slate-200 bg-slate-50 py-1.5 pl-9 pr-3 text-sm focus:outline-none focus:ring-2 focus:ring-primary/50 dark:border-slate-600 dark:bg-slate-700 dark:text-slate-200 dark:placeholder-slate-500"
              />
            </div>
          </div>
          <div className="flex items-center gap-3">
            {/* Dark mode toggle */}
            <button
              onClick={toggle}
              className="rounded-lg p-1.5 text-slate-500 hover:bg-slate-100 hover:text-slate-700 dark:text-slate-400 dark:hover:bg-slate-700 dark:hover:text-slate-200"
              title={dark ? 'Switch to light mode' : 'Switch to dark mode'}
            >
              {dark ? <Sun size={18} /> : <Moon size={18} />}
            </button>
            <button className="relative text-slate-500 hover:text-slate-700 dark:text-slate-400 dark:hover:text-slate-200">
              <Bell size={18} />
            </button>
            <div className="flex items-center gap-2">
              <div className="flex h-8 w-8 items-center justify-center rounded-full bg-primary text-xs font-semibold text-white">
                {user?.fullName?.[0]?.toUpperCase() ?? '?'}
              </div>
              <span className="hidden text-sm font-medium text-slate-700 sm:block dark:text-slate-200">{user?.fullName}</span>
            </div>
            <button
              onClick={logout}
              className="text-slate-500 hover:text-red-500 dark:text-slate-400 dark:hover:text-red-400"
              title="Sign out"
            >
              <LogOut size={18} />
            </button>
          </div>
        </header>

        {/* Page content */}
        <main className="flex-1 overflow-y-auto p-6">
          <Outlet />
        </main>
      </div>
    </div>
  )
}
