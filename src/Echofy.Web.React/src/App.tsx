import React from 'react'
import { Navigate, Route, Routes } from 'react-router-dom'
import { useAuth } from './lib/auth'

// Layouts
import { Layout } from './components/layout/Layout'
import { AuthLayout } from './components/layout/AuthLayout'
import { PublicLayout } from './components/layout/PublicLayout'
import { LandingLayout } from './components/layout/LandingLayout'

// Auth
import Login from './pages/auth/Login'
import Register from './pages/auth/Register'

// Dashboard
import Dashboard from './pages/dashboard/Index'

// E-Commerce
import Products from './pages/ecommerce/products/Index'
import ProductDetails from './pages/ecommerce/products/Details'
import ProductForm from './pages/ecommerce/products/Form'
import ManufacturerProducts from './pages/ecommerce/manufacturer-products/Index'
import ManufacturerProductDetails from './pages/ecommerce/manufacturer-products/Details'
import ManufacturerProductForm from './pages/ecommerce/manufacturer-products/Form'
import Customers from './pages/ecommerce/customers/Index'
import CustomerDetails from './pages/ecommerce/customers/Details'
import CustomerForm from './pages/ecommerce/customers/Form'

// CRM
import CrmDashboard from './pages/crm/Dashboard'
import Leads from './pages/crm/leads/Index'
import CreateLead from './pages/crm/leads/Create'
import Deals from './pages/crm/deals/Index'
import Contacts from './pages/crm/contacts/Index'

// Admin
import AdminUsers from './pages/admin/Users'
import AdminClients from './pages/admin/Clients'
import AdminCompanies from './pages/admin/Companies'
import AdminCategories from './pages/admin/Categories'
import AdminManufacturers from './pages/admin/Manufacturers'
import AdminUnits from './pages/admin/Units'
import AdminReviews from './pages/admin/Reviews'
import AdminAuditLogs from './pages/admin/AuditLogs'
import AdminShortIds from './pages/admin/ShortIds'
import AdminReferrals from './pages/admin/Referrals'
import AdminRewardPrograms from './pages/admin/RewardPrograms'
import AdminNfcSettings from './pages/admin/NfcSettings'
import AdminInvoices from './pages/admin/invoices/Index'
import AdminInvoiceForm from './pages/admin/invoices/Form'
import AdminInvoiceDetail from './pages/admin/invoices/Detail'

// Customer portal
import CustomerDashboard from './pages/customer/Dashboard'
import Favorites from './pages/customer/Favorites'
import CustomerReferrals from './pages/customer/Referrals'
import CustomerInvoices from './pages/customer/Invoices'
import CustomerInvoiceDetail from './pages/customer/InvoiceDetail'

// Public
import LandingPage from './pages/public/LandingPage'
import ProductPage from './pages/public/ProductPage'
import Placeholder from './pages/Placeholder'

// ── Guards ─────────────────────────────────────────────────────────────────────
function RequireAuth({ children }: { children: React.ReactElement }) {
  const { user } = useAuth()
  return user ? children : <Navigate to="/login" replace />
}

// Admin, SuperAdmin and SuperUser can all access admin-section pages
function RequireAdmin({ children }: { children: React.ReactElement }) {
  const { user, hasAdminAccess } = useAuth()
  if (!user) return <Navigate to="/login" replace />
  if (!hasAdminAccess()) return <Navigate to="/dashboard" replace />
  return children
}

// SuperAdmin and SuperUser only — excludes regular Admin
function RequireSuper({ children }: { children: React.ReactElement }) {
  const { user } = useAuth()
  if (!user) return <Navigate to="/login" replace />
  if (user.role !== 'SuperAdmin' && user.role !== 'SuperUser') return <Navigate to="/dashboard" replace />
  return children
}

// Blocks SuperUser from write-only routes (create/edit forms)
function RequireWrite({ children }: { children: React.ReactElement }) {
  const { user, canWrite } = useAuth()
  if (!user) return <Navigate to="/login" replace />
  if (!canWrite()) return <Navigate to="/dashboard" replace />
  return children
}

function RequireModule({ module, children }: { module: string; children: React.ReactElement }) {
  const { user, hasModule } = useAuth()
  if (!user) return <Navigate to="/login" replace />
  if (!hasModule(module)) return <Navigate to="/dashboard" replace />
  return children
}

export default function App() {
  return (
    <Routes>
      {/* Landing page */}
      <Route element={<LandingLayout />}>
        <Route path="/" element={<LandingPage />} />
      </Route>

      {/* Public product page */}
      <Route element={<PublicLayout />}>
        <Route path="/p/:shortId" element={<ProductPage />} />
      </Route>

      {/* Auth pages */}
      <Route element={<AuthLayout />}>
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
      </Route>

      {/* Authenticated app */}
      <Route element={<RequireAuth><Layout /></RequireAuth>}>
        <Route path="dashboard" element={<Dashboard />} />

        {/* E-Commerce */}
        <Route path="products" element={<RequireModule module="ecommerce"><Products /></RequireModule>} />
        <Route path="products/new" element={<RequireWrite><RequireModule module="ecommerce"><ProductForm /></RequireModule></RequireWrite>} />
        <Route path="products/:id" element={<RequireModule module="ecommerce"><ProductDetails /></RequireModule>} />
        <Route path="products/:id/edit" element={<RequireWrite><RequireModule module="ecommerce"><ProductForm /></RequireModule></RequireWrite>} />
        <Route path="manufacturer-products" element={<RequireModule module="ecommerce"><ManufacturerProducts /></RequireModule>} />
        <Route path="manufacturer-products/new" element={<RequireWrite><RequireModule module="ecommerce"><ManufacturerProductForm /></RequireModule></RequireWrite>} />
        <Route path="manufacturer-products/:id" element={<RequireModule module="ecommerce"><ManufacturerProductDetails /></RequireModule>} />
        <Route path="manufacturer-products/:id/edit" element={<RequireWrite><RequireModule module="ecommerce"><ManufacturerProductForm /></RequireModule></RequireWrite>} />
        <Route path="customers" element={<RequireModule module="ecommerce"><Customers /></RequireModule>} />
        <Route path="customers/new" element={<RequireWrite><RequireModule module="ecommerce"><CustomerForm /></RequireModule></RequireWrite>} />
        <Route path="customers/:id" element={<RequireModule module="ecommerce"><CustomerDetails /></RequireModule>} />
        <Route path="customers/:id/edit" element={<RequireWrite><RequireModule module="ecommerce"><CustomerForm /></RequireModule></RequireWrite>} />

        {/* CRM */}
        <Route path="crm" element={<RequireModule module="crm"><CrmDashboard /></RequireModule>} />
        <Route path="crm/leads" element={<RequireModule module="crm"><Leads /></RequireModule>} />
        <Route path="crm/leads/new" element={<RequireWrite><RequireModule module="crm"><CreateLead /></RequireModule></RequireWrite>} />
        <Route path="crm/deals" element={<RequireModule module="crm"><Deals /></RequireModule>} />
        <Route path="crm/contacts" element={<RequireModule module="crm"><Contacts /></RequireModule>} />

        {/* Productivity */}
        <Route path="kanban" element={<RequireModule module="kanban"><Placeholder title="Kanban" /></RequireModule>} />
        <Route path="calendar" element={<RequireModule module="calendar"><Placeholder title="Calendar" /></RequireModule>} />
        <Route path="chat" element={<RequireModule module="chat"><Placeholder title="Chat" /></RequireModule>} />

        {/* Admin */}
        <Route path="admin/users" element={<RequireAdmin><AdminUsers /></RequireAdmin>} />
        <Route path="admin/clients" element={<RequireAdmin><AdminClients /></RequireAdmin>} />
        <Route path="admin/companies" element={<RequireAdmin><AdminCompanies /></RequireAdmin>} />
        <Route path="admin/categories" element={<RequireAdmin><AdminCategories /></RequireAdmin>} />
        <Route path="admin/manufacturers" element={<RequireAdmin><AdminManufacturers /></RequireAdmin>} />
        <Route path="admin/units" element={<RequireAdmin><AdminUnits /></RequireAdmin>} />
        <Route path="admin/reviews" element={<RequireAdmin><AdminReviews /></RequireAdmin>} />
        <Route path="admin/audit-logs" element={<RequireAdmin><AdminAuditLogs /></RequireAdmin>} />
        <Route path="admin/short-ids" element={<RequireAdmin><AdminShortIds /></RequireAdmin>} />
        <Route path="admin/referrals" element={<RequireAdmin><AdminReferrals /></RequireAdmin>} />
        <Route path="admin/reward-programs" element={<RequireAdmin><AdminRewardPrograms /></RequireAdmin>} />
        <Route path="admin/nfc-settings" element={<RequireSuper><AdminNfcSettings /></RequireSuper>} />
        <Route path="admin/invoices" element={<RequireModule module="ecommerce"><AdminInvoices /></RequireModule>} />
        <Route path="admin/invoices/new" element={<RequireWrite><RequireModule module="ecommerce"><AdminInvoiceForm /></RequireModule></RequireWrite>} />
        <Route path="admin/invoices/:id" element={<RequireModule module="ecommerce"><AdminInvoiceDetail /></RequireModule>} />
        <Route path="admin/invoices/:id/edit" element={<RequireWrite><RequireModule module="ecommerce"><AdminInvoiceForm /></RequireModule></RequireWrite>} />

        {/* Customer portal */}
        <Route path="customer" element={<CustomerDashboard />} />
        <Route path="customer/favorites" element={<Favorites />} />
        <Route path="customer/referrals" element={<CustomerReferrals />} />
        <Route path="customer/invoices" element={<CustomerInvoices />} />
        <Route path="customer/invoices/:id" element={<CustomerInvoiceDetail />} />
      </Route>

      {/* Catch-all */}
      <Route path="*" element={<Navigate to="/dashboard" replace />} />
    </Routes>
  )
}
