import { createContext, useContext, useState, useEffect, type ReactNode } from 'react'
import { api } from './api'
import type { AuthUser, LoginResponse } from '../types'

interface AuthContextType {
  user: AuthUser | null
  login: (email: string, password: string) => Promise<void>
  logout: () => void
  hasModule: (module: string) => boolean
  isAdmin: () => boolean
  isSuperAdmin: () => boolean
  isSuperUser: () => boolean
  canWrite: () => boolean
  hasAdminAccess: () => boolean
}

const AuthContext = createContext<AuthContextType | null>(null)

function parseJwtPayload(token: string): { sub: string } | null {
  try {
    const payload = token.split('.')[1]
    return JSON.parse(atob(payload))
  } catch {
    return null
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(() => {
    const stored = localStorage.getItem('echofy_user')
    return stored ? JSON.parse(stored) : null
  })

  useEffect(() => {
    // Validate token expiry on mount
    const token = localStorage.getItem('echofy_token')
    if (token) {
      const payload = parseJwtPayload(token)
      if (!payload) logout()
    }
  }, [])

  const login = async (email: string, password: string) => {
    const { data } = await api.post<LoginResponse>('/auth/login', { email, password })
    const payload = parseJwtPayload(data.token)
    const authUser: AuthUser = {
      id: payload?.sub ?? '',
      email: data.email,
      fullName: data.fullName,
      role: data.role,
      modules: data.modules,
      clientId: data.clientId,
      clientName: data.clientName,
    }
    localStorage.setItem('echofy_token', data.token)
    localStorage.setItem('echofy_user', JSON.stringify(authUser))
    setUser(authUser)
  }

  const logout = () => {
    localStorage.removeItem('echofy_token')
    localStorage.removeItem('echofy_user')
    setUser(null)
  }

  const hasModule = (module: string) => user?.modules.includes(module) ?? false
  const isAdmin = () => user?.role === 'Admin'
  const isSuperAdmin = () => user?.role === 'SuperAdmin'
  const isSuperUser = () => user?.role === 'SuperUser'
  // SuperUser is read-only; everyone else with a write-capable role can mutate
  const canWrite = () => user?.role !== 'SuperUser'
  // True for Admin, SuperAdmin, SuperUser — any role with cross-tenant visibility
  const hasAdminAccess = () => ['Admin', 'SuperAdmin', 'SuperUser'].includes(user?.role ?? '')

  return (
    <AuthContext.Provider value={{ user, login, logout, hasModule, isAdmin, isSuperAdmin, isSuperUser, canWrite, hasAdminAccess }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}
