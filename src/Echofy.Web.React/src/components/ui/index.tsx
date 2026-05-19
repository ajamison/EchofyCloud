import { type ButtonHTMLAttributes, type InputHTMLAttributes, type SelectHTMLAttributes, type TextareaHTMLAttributes, forwardRef } from 'react'
import clsx from 'clsx'
import { Loader2 } from 'lucide-react'

// ── Button ─────────────────────────────────────────────────────────────────────
interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'primary' | 'secondary' | 'danger' | 'ghost'
  size?: 'sm' | 'md' | 'lg'
  loading?: boolean
}

export const Button = forwardRef<HTMLButtonElement, ButtonProps>(
  ({ variant = 'primary', size = 'md', loading, children, className, disabled, ...props }, ref) => (
    <button
      ref={ref}
      disabled={disabled || loading}
      className={clsx(
        'inline-flex items-center justify-center gap-2 rounded-lg font-medium transition-colors focus:outline-none focus:ring-2 focus:ring-offset-1 disabled:opacity-50',
        {
          primary:   'bg-primary text-white hover:bg-primary-600 focus:ring-primary/50',
          secondary: 'border border-slate-300 bg-white text-slate-700 hover:bg-slate-50 focus:ring-slate-300 dark:border-slate-600 dark:bg-slate-700 dark:text-slate-200 dark:hover:bg-slate-600',
          danger:    'bg-red-600 text-white hover:bg-red-700 focus:ring-red-500',
          ghost:     'text-slate-600 hover:bg-slate-100 focus:ring-slate-200 dark:text-slate-300 dark:hover:bg-slate-700',
        }[variant],
        { sm: 'px-3 py-1.5 text-xs', md: 'px-4 py-2 text-sm', lg: 'px-5 py-2.5 text-base' }[size],
        className
      )}
      {...props}
    >
      {loading && <Loader2 size={14} className="animate-spin" />}
      {children}
    </button>
  )
)
Button.displayName = 'Button'

// ── Badge ─────────────────────────────────────────────────────────────────────
interface BadgeProps { label: string; color?: 'blue' | 'green' | 'red' | 'yellow' | 'slate' | 'purple' }
export function Badge({ label, color = 'blue' }: BadgeProps) {
  return (
    <span className={clsx(
      'inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium',
      {
        blue:   'bg-blue-100 text-blue-800 dark:bg-blue-900/40 dark:text-blue-300',
        green:  'bg-green-100 text-green-800 dark:bg-green-900/40 dark:text-green-300',
        red:    'bg-red-100 text-red-800 dark:bg-red-900/40 dark:text-red-300',
        yellow: 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900/40 dark:text-yellow-300',
        slate:  'bg-slate-100 text-slate-700 dark:bg-slate-700 dark:text-slate-300',
        purple: 'bg-purple-100 text-purple-800 dark:bg-purple-900/40 dark:text-purple-300',
      }[color]
    )}>
      {label}
    </span>
  )
}

// ── Card ──────────────────────────────────────────────────────────────────────
export function Card({ children, className }: { children: React.ReactNode; className?: string }) {
  return (
    <div className={clsx('rounded-xl bg-white p-6 shadow-sm ring-1 ring-slate-200 dark:bg-slate-800 dark:ring-slate-700', className)}>
      {children}
    </div>
  )
}

// ── Spinner ───────────────────────────────────────────────────────────────────
export function Spinner({ className }: { className?: string }) {
  return <Loader2 className={clsx('animate-spin text-primary', className)} />
}

export function PageSpinner() {
  return (
    <div className="flex h-64 items-center justify-center">
      <Spinner className="h-8 w-8" />
    </div>
  )
}

// ── PageHeader ────────────────────────────────────────────────────────────────
interface PageHeaderProps {
  title: string
  subtitle?: string
  actions?: React.ReactNode
}
export function PageHeader({ title, subtitle, actions }: PageHeaderProps) {
  return (
    <div className="mb-6 flex items-start justify-between">
      <div>
        <h1 className="text-2xl font-bold text-slate-800 dark:text-slate-100">{title}</h1>
        {subtitle && <p className="mt-1 text-sm text-slate-500 dark:text-slate-400">{subtitle}</p>}
      </div>
      {actions && <div className="flex items-center gap-2">{actions}</div>}
    </div>
  )
}

// ── FormField ─────────────────────────────────────────────────────────────────
interface FormFieldProps { label: string; error?: string; required?: boolean; children: React.ReactNode }
export function FormField({ label, error, required, children }: FormFieldProps) {
  return (
    <div>
      <label className="mb-1 block text-sm font-medium text-slate-700 dark:text-slate-300">
        {label}{required && <span className="ml-0.5 text-red-500">*</span>}
      </label>
      {children}
      {error && <p className="mt-1 text-xs text-red-500">{error}</p>}
    </div>
  )
}

// ── Input ─────────────────────────────────────────────────────────────────────
export const Input = forwardRef<HTMLInputElement, InputHTMLAttributes<HTMLInputElement>>(
  ({ className, ...props }, ref) => (
    <input
      ref={ref}
      className={clsx(
        'w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm placeholder-slate-400 transition-colors focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/30 disabled:bg-slate-50',
        'dark:border-slate-600 dark:bg-slate-700 dark:text-slate-200 dark:placeholder-slate-500 dark:focus:border-primary dark:disabled:bg-slate-800',
        className
      )}
      {...props}
    />
  )
)
Input.displayName = 'Input'

// ── Select ────────────────────────────────────────────────────────────────────
export const Select = forwardRef<HTMLSelectElement, SelectHTMLAttributes<HTMLSelectElement>>(
  ({ className, children, ...props }, ref) => (
    <select
      ref={ref}
      className={clsx(
        'w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/30 disabled:bg-slate-50',
        'dark:border-slate-600 dark:bg-slate-700 dark:text-slate-200 dark:disabled:bg-slate-800',
        className
      )}
      {...props}
    >
      {children}
    </select>
  )
)
Select.displayName = 'Select'

// ── Textarea ──────────────────────────────────────────────────────────────────
export const Textarea = forwardRef<HTMLTextAreaElement, TextareaHTMLAttributes<HTMLTextAreaElement>>(
  ({ className, ...props }, ref) => (
    <textarea
      ref={ref}
      className={clsx(
        'w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm placeholder-slate-400 focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/30',
        'dark:border-slate-600 dark:bg-slate-700 dark:text-slate-200 dark:placeholder-slate-500',
        className
      )}
      rows={3}
      {...props}
    />
  )
)
Textarea.displayName = 'Textarea'

// ── Table ─────────────────────────────────────────────────────────────────────
export function Table({ children }: { children: React.ReactNode }) {
  return (
    <div className="overflow-x-auto rounded-xl ring-1 ring-slate-200 dark:ring-slate-700">
      <table className="w-full text-sm">{children}</table>
    </div>
  )
}
export function Th({ children, className }: { children: React.ReactNode; className?: string }) {
  return (
    <th className={clsx('bg-slate-50 px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500 dark:bg-slate-800 dark:text-slate-400', className)}>
      {children}
    </th>
  )
}
export function Td({ children, className }: { children: React.ReactNode; className?: string }) {
  return <td className={clsx('border-t border-slate-100 px-4 py-3 text-slate-700 dark:border-slate-700 dark:text-slate-300', className)}>{children}</td>
}

// ── Modal ─────────────────────────────────────────────────────────────────────
interface ModalProps { open: boolean; onClose: () => void; title: string; children: React.ReactNode }
export function Modal({ open, onClose, title, children }: ModalProps) {
  if (!open) return null
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div className="absolute inset-0 bg-slate-900/50" onClick={onClose} />
      <div className="relative w-full max-w-lg rounded-2xl bg-white p-6 shadow-xl dark:bg-slate-800">
        <h2 className="mb-4 text-lg font-semibold text-slate-800 dark:text-slate-100">{title}</h2>
        {children}
      </div>
    </div>
  )
}

// ── Empty State ───────────────────────────────────────────────────────────────
export function EmptyState({ message }: { message: string }) {
  return (
    <div className="flex h-40 items-center justify-center text-sm text-slate-400 dark:text-slate-500">{message}</div>
  )
}

// ── Stats Card ────────────────────────────────────────────────────────────────
interface StatCardProps { label: string; value: string | number; icon: React.ReactNode; color?: string }
export function StatCard({ label, value, icon, color = 'bg-primary' }: StatCardProps) {
  return (
    <Card className="flex items-center gap-4">
      <div className={clsx('flex h-12 w-12 items-center justify-center rounded-xl text-white', color)}>
        {icon}
      </div>
      <div>
        <p className="text-sm text-slate-500 dark:text-slate-400">{label}</p>
        <p className="text-2xl font-bold text-slate-800 dark:text-slate-100">{value}</p>
      </div>
    </Card>
  )
}
