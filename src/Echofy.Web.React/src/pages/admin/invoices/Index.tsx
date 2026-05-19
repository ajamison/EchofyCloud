import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { Plus, FileText } from 'lucide-react'
import { api } from '../../../lib/api'
import { useAuth } from '../../../lib/auth'
import { PageHeader, Table, Th, Td, Badge, Button, PageSpinner } from '../../../components/ui'
import type { InvoiceListItemDto } from '../../../types'

const statusColor: Record<string, 'slate' | 'blue' | 'green' | 'yellow' | 'red' | 'purple'> = {
  Draft:     'slate',
  Sent:      'blue',
  Viewed:    'purple',
  Paid:      'green',
  Overdue:   'red',
  Cancelled: 'red',
}

export default function AdminInvoices() {
  const { user } = useAuth()
  const isAdmin = user?.role === 'Admin'
  const { data: invoices = [], isLoading } = useQuery<InvoiceListItemDto[]>({
    queryKey: ['admin-invoices'],
    queryFn: () => api.get('/admin/invoices').then(r => r.data),
  })

  if (isLoading) return <PageSpinner />

  const totalPaid    = invoices.filter(i => i.status === 'Paid').reduce((s, i) => s + i.total, 0)
  const totalPending = invoices.filter(i => i.status === 'Sent').reduce((s, i) => s + i.total, 0)

  return (
    <div className="space-y-6">
      <PageHeader
        title="Invoices"
        subtitle={`${invoices.length} total · $${totalPaid.toFixed(2)} paid · $${totalPending.toFixed(2)} pending`}
        actions={
          <Link to="/admin/invoices/new">
            <Button><Plus size={15} /> New Invoice</Button>
          </Link>
        }
      />

      <Table>
        <thead>
          <tr>
            <Th>Invoice #</Th>
            <Th>Customer</Th>
            {isAdmin && <Th>Company</Th>}
            <Th>Issued</Th>
            <Th>Due</Th>
            <Th>Total</Th>
            <Th>Status</Th>
            <Th></Th>
          </tr>
        </thead>
        <tbody>
          {invoices.length === 0 && (
            <tr>
              <td colSpan={isAdmin ? 8 : 7} className="py-12 text-center text-sm text-slate-400">
                <FileText size={32} className="mx-auto mb-2 text-slate-300" />
                No invoices yet. Create your first one.
              </td>
            </tr>
          )}
          {invoices.map(inv => (
            <tr key={inv.id} className="hover:bg-slate-50 dark:hover:bg-slate-700/30">
              <Td className="font-medium text-primary">{inv.invoiceNumber}</Td>
              <Td>
                <div className="font-medium text-slate-800 dark:text-slate-200">{inv.customerName}</div>
                <div className="text-xs text-slate-400">{inv.customerEmail}</div>
              </Td>
              {isAdmin && (
                <Td>
                  {inv.companyName
                    ? <Badge label={inv.companyName} color="blue" />
                    : <span className="text-xs text-slate-400">—</span>}
                </Td>
              )}
              <Td className="text-slate-500">{new Date(inv.issuedDate).toLocaleDateString()}</Td>
              <Td className="text-slate-500">{new Date(inv.dueDate).toLocaleDateString()}</Td>
              <Td className="font-semibold text-slate-800 dark:text-slate-200">${inv.total.toFixed(2)}</Td>
              <Td><Badge label={inv.status} color={statusColor[inv.status] ?? 'slate'} /></Td>
              <Td>
                <Link
                  to={`/admin/invoices/${inv.id}`}
                  className="text-xs font-medium text-primary hover:underline"
                >
                  View
                </Link>
              </Td>
            </tr>
          ))}
        </tbody>
      </Table>
    </div>
  )
}
