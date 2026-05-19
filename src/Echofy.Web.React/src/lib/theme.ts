import { useEffect, useState } from 'react'

const KEY = 'echofy_dark'

export function useTheme() {
  const [dark, setDark] = useState(() => {
    try { return localStorage.getItem(KEY) === 'true' } catch { return false }
  })

  useEffect(() => {
    document.documentElement.classList.toggle('dark', dark)
    try { localStorage.setItem(KEY, String(dark)) } catch { /* */ }
  }, [dark])

  return { dark, toggle: () => setDark(d => !d) }
}
