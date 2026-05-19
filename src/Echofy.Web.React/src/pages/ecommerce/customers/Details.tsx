import { useParams, Link } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { ArrowLeft, Edit } from 'lucide-react'
import { api } from '../../../lib/api'
import { useAuth } from '../../../lib/auth'
import { Card, PageHeader, PageSpinner, Button } from '../../../components/ui'
import type { Customer } from '../../../types'

export default function CustomerDetails() {
  const { id } = useParams<{ id: string }>()
  const { user } = useAuth()
  const canEdit = user?.role === 'Admin' || user?.role === 'Manager'
  const { data: customer, isLoading } = useQuery<Customer>({
    queryKey: ['customer', id],
    queryFn: () => api.get(`/customers/${id}`).then((r) => r.data),
  })

  if (isLoading) return <PageSpinner />
  if (!customer) return <p className="text-slate-500">Customer not found.</p>

  return (
    <div>
      <div className="mb-4">
        <Link to="/customers" className="flex items-center gap-1 text-sm text-slate-500 hover:text-slate-700">
          <ArrowLeft size={14} /> Back to Customers
        </Link>
      </div>
      <PageHeader
        title={customer.fullName}
        subtitle={customer.clientName ? `${customer.email} · ${customer.clientName}` : customer.email}
        actions={canEdit && (
          <Link to={`/customers/${id}/edit`}>
            <Button variant="secondary" size="sm"><Edit size={14} /> Edit</Button>
          </Link>
        )}
      />
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        <Card>
          <h3 className="mb-4 font-semibold text-slate-700">Contact</h3>
          <dl className="space-y-2 text-sm">
            <div><dt className="text-slate-400">Phone</dt><dd>{customer.phone}</dd></div>
            <div><dt className="text-slate-400">Address</dt><dd>{[customer.street, customer.city, customer.province, customer.country].filter(Boolean).join(', ')}</dd></div>
            {customer.notes && <div><dt className="text-slate-400">Notes</dt><dd>{customer.notes}</dd></div>}
          </dl>
        </Card>
        <Card>
          <h3 className="mb-4 font-semibold text-slate-700">Details</h3>
          <dl className="space-y-2 text-sm">
            <div className="flex justify-between"><dt className="text-slate-400">Joined</dt><dd>{new Date(customer.joinedDate).toLocaleDateString()}</dd></div>
          </dl>
        </Card>
      </div>
    </div>
  )
}
