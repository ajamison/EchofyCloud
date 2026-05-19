import { NavLink } from 'react-router-dom'
import { useAuth } from '../../lib/auth'
import {
  LayoutDashboard, Package, Users, UserCircle,
  Settings, ChevronDown, ChevronRight, QrCode, Share2, Gift, UserRound, FileText, Building2, Trophy, KeyRound,
} from 'lucide-react'
import { useState } from 'react'
import clsx from 'clsx'

function EchofyLogo({ collapsed }: { collapsed?: boolean }) {
  return (
    <div className={clsx('flex items-center gap-2.5', collapsed && 'justify-center')}>
      <svg
        width={collapsed ? 30 : 36}
        height={collapsed ? 27 : 32}
        viewBox="0 0 570 510"
        fill="none"
        xmlns="http://www.w3.org/2000/svg"
        className="shrink-0"
      >
        {/* Top-left QR finder */}
        <rect x="18" y="18" width="138" height="138" rx="18" fill="white"/>
        <rect x="40" y="40" width="94" height="94" rx="7" fill="#1e40af"/>
        <rect x="60" y="60" width="54" height="54" rx="7" fill="white"/>

        {/* Bottom-left QR finder */}
        <rect x="18" y="294" width="138" height="138" rx="18" fill="white"/>
        <rect x="40" y="316" width="94" height="94" rx="7" fill="#1e40af"/>
        <rect x="60" y="336" width="54" height="54" rx="7" fill="white"/>

        {/* QR data modules */}
        <rect x="173" y="18"  width="34" height="34" rx="6" fill="white"/>
        <rect x="213" y="18"  width="34" height="34" rx="6" fill="white"/>
        <rect x="173" y="76"  width="34" height="34" rx="6" fill="white"/>
        <rect x="173" y="130" width="34" height="34" rx="6" fill="white"/>
        <rect x="213" y="106" width="34" height="34" rx="6" fill="white"/>
        <rect x="18"  y="172" width="70" height="34" rx="6" fill="white"/>
        <rect x="107" y="172" width="34" height="34" rx="6" fill="white"/>
        <rect x="173" y="202" width="34" height="34" rx="6" fill="white"/>
        <rect x="173" y="248" width="34" height="34" rx="6" fill="white"/>
        <rect x="213" y="226" width="34" height="34" rx="6" fill="white"/>
        <rect x="18"  y="258" width="34" height="34" rx="6" fill="white"/>
        <rect x="173" y="294" width="34" height="34" rx="6" fill="white"/>
        <rect x="213" y="320" width="34" height="34" rx="6" fill="white"/>
        <rect x="173" y="368" width="34" height="34" rx="6" fill="white"/>

        {/* Price tag body */}
        <path
          d="M250,44 L438,44 Q462,44 462,68 L460,312 Q460,340 440,356 L316,428 Q298,442 280,430 L216,370 Q202,354 216,336 L250,44 Z"
          fill="white"
        />
        {/* String hole */}
        <circle cx="404" cy="110" r="30" fill="#1e3a8a"/>

        {/* Radio waves */}
        <path d="M494,192 C516,246 516,274 494,328" stroke="white" strokeWidth="32" strokeLinecap="round"/>
        <path d="M530,152 C561,248 561,272 530,366" stroke="white" strokeWidth="31" strokeLinecap="round"/>
      </svg>
      {!collapsed && (
        <span className="text-lg font-bold tracking-tight text-white">Echofy</span>
      )}
    </div>
  )
}

interface NavItem {
  label: string
  to?: string
  icon: React.ReactNode
  children?: NavItem[]
  module?: string
  roles?: string[]
}

function NavGroup({ item, collapsed }: { item: NavItem; collapsed?: boolean }) {
  const [open, setOpen] = useState(true)
  const { user } = useAuth()

  const visible = item.children?.filter((child) => {
    if (child.roles && !child.roles.includes(user?.role ?? '')) return false
    if (child.module && !user?.modules.includes(child.module)) return false
    return true
  })

  if (!visible?.length) return null

  if (collapsed) {
    return (
      <div className="space-y-0.5">
        {visible.map((child) => (
          <NavLink
            key={child.to}
            to={child.to!}
            title={child.label}
            className={({ isActive }) =>
              clsx(
                'flex items-center justify-center rounded-md p-2 transition-colors',
                isActive
                  ? 'bg-primary text-white'
                  : 'text-slate-300 hover:bg-slate-700 hover:text-white'
              )
            }
          >
            {child.icon}
          </NavLink>
        ))}
      </div>
    )
  }

  return (
    <div>
      <button
        onClick={() => setOpen((o) => !o)}
        className="flex w-full items-center gap-3 px-3 py-2 text-xs font-semibold uppercase tracking-wider text-slate-400 hover:text-slate-200"
      >
        {item.icon}
        <span className="flex-1 text-left">{item.label}</span>
        {open ? <ChevronDown size={12} /> : <ChevronRight size={12} />}
      </button>
      {open && (
        <div className="ml-4 space-y-0.5">
          {visible.map((child) => (
            <NavLink
              key={child.to}
              to={child.to!}
              className={({ isActive }) =>
                clsx(
                  'flex items-center gap-2.5 rounded-md px-3 py-2 text-sm transition-colors',
                  isActive
                    ? 'bg-primary text-white'
                    : 'text-slate-300 hover:bg-slate-700 hover:text-white'
                )
              }
            >
              {child.icon}
              {child.label}
            </NavLink>
          ))}
        </div>
      )}
    </div>
  )
}

interface SidebarProps {
  onClose?: () => void
  collapsed?: boolean
}

export function Sidebar({ onClose, collapsed }: SidebarProps) {
  const { user } = useAuth()

  const navItems: NavItem[] = [
    {
      label: 'Main', icon: <LayoutDashboard size={14} />,
      children: [
        { label: 'Dashboard', to: '/dashboard', icon: <LayoutDashboard size={16} /> },
        { label: 'My Account', to: '/customer', icon: <UserRound size={16} /> },
        { label: 'My Referrals', to: '/customer/referrals', icon: <Share2 size={16} /> },
      ],
    },
    {
      label: 'E-Commerce', icon: <Package size={14} />, module: 'ecommerce',
      children: [
        { label: 'Products', to: '/products', icon: <Package size={16} />, module: 'ecommerce' },
        { label: 'Mfr Products', to: '/manufacturer-products', icon: <Package size={16} />, module: 'ecommerce' },
        { label: 'Customers', to: '/customers', icon: <Users size={16} />, module: 'ecommerce' },
        { label: 'Invoices', to: '/admin/invoices', icon: <FileText size={16} />, module: 'ecommerce' },
      ],
    },
    {
      label: 'Admin', icon: <Settings size={14} />,
      children: [
        { label: 'Users', to: '/admin/users', icon: <Users size={16} />, roles: ['Admin', 'SuperAdmin', 'SuperUser'] },
        { label: 'Clients', to: '/admin/clients', icon: <UserCircle size={16} />, roles: ['Admin', 'SuperAdmin', 'SuperUser'] },
        { label: 'Companies', to: '/admin/companies', icon: <Building2 size={16} />, roles: ['Admin', 'SuperAdmin', 'SuperUser'] },
        { label: 'Categories', to: '/admin/categories', icon: <Package size={16} />, roles: ['Admin', 'SuperAdmin', 'SuperUser'] },
        { label: 'Manufacturers', to: '/admin/manufacturers', icon: <Package size={16} />, roles: ['Admin', 'SuperAdmin', 'SuperUser'] },
        { label: 'Units', to: '/admin/units', icon: <Package size={16} />, roles: ['Admin', 'SuperAdmin', 'SuperUser'] },
        { label: 'Reviews', to: '/admin/reviews', icon: <Settings size={16} />, roles: ['Admin', 'SuperAdmin', 'SuperUser'] },
        { label: 'QR Labels', to: '/admin/short-ids', icon: <QrCode size={16} />, roles: ['Admin', 'SuperAdmin', 'SuperUser'] },
        { label: 'Referrals', to: '/admin/referrals', icon: <Gift size={16} />, roles: ['Admin', 'SuperAdmin', 'SuperUser'] },
        { label: 'Reward Programs', to: '/admin/reward-programs', icon: <Trophy size={16} />, roles: ['Admin', 'SuperAdmin', 'SuperUser'] },
        { label: 'NFC Settings', to: '/admin/nfc-settings', icon: <KeyRound size={16} />, roles: ['SuperAdmin', 'SuperUser'] },
        { label: 'Audit Logs', to: '/admin/audit-logs', icon: <Settings size={16} />, roles: ['Admin', 'SuperAdmin', 'SuperUser'] },
      ],
    },
  ]

  // Customer-facing role gets simplified nav
  if (user?.role === 'Customer') {
    return (
      <aside className={clsx('flex h-full flex-col bg-slate-800 transition-all duration-300', collapsed ? 'w-16' : 'w-64')}>
        <div className={clsx('flex h-16 items-center', collapsed ? 'justify-center px-2' : 'px-5')}>
          <EchofyLogo collapsed={collapsed} />
        </div>
        <nav className="flex-1 space-y-0.5 px-3 py-4">
          <NavLink to="/customer" title="My Account" className={({ isActive }) => clsx('flex items-center rounded-md px-3 py-2 text-sm', collapsed ? 'justify-center' : 'gap-2.5', isActive ? 'bg-primary text-white' : 'text-slate-300 hover:bg-slate-700 hover:text-white')}>
            <LayoutDashboard size={16} /> {!collapsed && 'My Account'}
          </NavLink>
          <NavLink to="/customer/favorites" title="My Favorites" className={({ isActive }) => clsx('flex items-center rounded-md px-3 py-2 text-sm', collapsed ? 'justify-center' : 'gap-2.5', isActive ? 'bg-primary text-white' : 'text-slate-300 hover:bg-slate-700 hover:text-white')}>
            <Package size={16} /> {!collapsed && 'My Favorites'}
          </NavLink>
          <NavLink to="/customer/invoices" title="My Invoices" className={({ isActive }) => clsx('flex items-center rounded-md px-3 py-2 text-sm', collapsed ? 'justify-center' : 'gap-2.5', isActive ? 'bg-primary text-white' : 'text-slate-300 hover:bg-slate-700 hover:text-white')}>
            <FileText size={16} /> {!collapsed && 'My Invoices'}
          </NavLink>
          <NavLink to="/customer/referrals" title="My Referrals" className={({ isActive }) => clsx('flex items-center rounded-md px-3 py-2 text-sm', collapsed ? 'justify-center' : 'gap-2.5', isActive ? 'bg-primary text-white' : 'text-slate-300 hover:bg-slate-700 hover:text-white')}>
            <Share2 size={16} /> {!collapsed && 'My Referrals'}
          </NavLink>
        </nav>
      </aside>
    )
  }

  return (
    <aside
      className={clsx('flex h-full flex-col bg-slate-800 transition-all duration-300', collapsed ? 'w-16' : 'w-64')}
      onClick={onClose}
    >
      <div className={clsx('flex h-16 items-center', collapsed ? 'justify-center px-2' : 'px-5')}>
        <EchofyLogo collapsed={collapsed} />
      </div>
      <nav className={clsx('flex-1 space-y-1 overflow-y-auto py-4', collapsed ? 'px-2' : 'px-3')}>
        {navItems.map((item) => (
          <NavGroup key={item.label} item={item} collapsed={collapsed} />
        ))}
      </nav>
      {!collapsed && (
        <div className="border-t border-slate-700 p-4">
          <div className="text-xs text-slate-400">
            <div className="font-medium text-slate-200">{user?.fullName}</div>
            <div>{user?.role} {user?.clientName ? `· ${user.clientName}` : ''}</div>
          </div>
        </div>
      )}
    </aside>
  )
}
