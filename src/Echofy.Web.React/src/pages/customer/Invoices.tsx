import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { FileText } from 'lucide-react'
import { api } from '../../lib/api'
import { PageHeader, Card, Badge, PageSpinner } from '../../components/ui'
import type { InvoiceListItemDto } from '../../types'

const statusColor: Record<string, 'slate' | 'blue' | 'green' | 'yellow' | 'red' | 'purple'> = {
  Draft: 'slate', Sent: 'blue', Viewed: 'purple', Paid: 'green', Overdue: 'red', Cancelled: 'red',
}

export default function CustomerInvoices() {
  const { data: invoices = [], isLoading } = useQuery<InvoiceListItemDto[]>({
    queryKey: ['me', 'invoices'],
    queryFn: () => api.get('/me/invoices').then(r => r.data),
  })

  if (isLoading) return <PageSpinner />

  return (
    <div className="space-y-6">
      <PageHeader title="My Invoices" subtitle="Invoices from your service provider." />

      {invoices.length === 0 ? (
        <Card>
          <div className="py-12 text-center">
            <FileText size={36} className="mx-auto mb-3 text-slate-300" />
            <h3 className="font-medium text-slate-500">No invoices yet</h3>
            <p className="mt-1 text-sm text-slate-400">Your invoices will appear here once issued.</p>
          </div>
        </Card>
      ) : (
        <div className="space-y-3">
          {invoices.map(inv => (
            <Link key={inv.id} to={`/customer/invoices/${inv.id}`} className="block">
              <Card className="transition-shadow hover:shadow-md">
                <div className="flex flex-wrap items-center justify-between gap-3">
                  <div>
                    <p className="font-semibold text-primary">{inv.invoiceNumber}</p>
                    <p className="text-sm text-slate-500">
                      Due {new Date(inv.dueDate).toLocaleDateString()}
                      {inv.paidAt && ` · Paid ${new Date(inv.paidAt).toLocaleDateString()}`}
                    </p>
                  </div>
                  <div className="flex items-center gap-3">
                    <span className="text-lg font-bold text-slate-800 dark:text-slate-100">
                      ${inv.total.toFixed(2)}
                    </span>
                    <Badge label={inv.status} color={statusColor[inv.status] ?? 'slate'} />
                  </div>
                </div>
              </Card>
            </Link>
          ))}
        </div>
      )}
    </div>
  )
}
