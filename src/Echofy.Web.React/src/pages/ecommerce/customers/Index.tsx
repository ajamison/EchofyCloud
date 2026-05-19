import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { Plus, Eye, Search } from 'lucide-react'
import { api } from '../../../lib/api'
import { useAuth } from '../../../lib/auth'
import { Button, PageHeader, Table, Th, Td, PageSpinner, Input, Badge } from '../../../components/ui'
import type { CustomerListItem } from '../../../types'

export default function Customers() {
  const [search, setSearch] = useState('')
  const { hasAdminAccess, canWrite } = useAuth()
  const showClientColumn = hasAdminAccess()

  const { data: customers = [], isLoading } = useQuery<CustomerListItem[]>({
    queryKey: ['customers', search],
    queryFn: () => api.get('/customers', { params: { search: search || undefined } }).then((r) => r.data),
  })

  if (isLoading) return <PageSpinner />

  return (
    <div>
      <PageHeader
        title="Customers"
        subtitle={`${customers.length} customers`}
        actions={canWrite() && <Link to="/customers/new"><Button><Plus size={15} /> Add Customer</Button></Link>}
      />
      <div className="mb-4 flex items-center gap-3">
        <div className="relative flex-1 max-w-xs">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" size={14} />
          <Input className="pl-8" placeholder="Search customers…" value={search} onChange={(e) => setSearch(e.target.value)} />
        </div>
      </div>
      <Table>
        <thead>
          <tr>
            <Th>Customer</Th>
            {showClientColumn && <Th>Client</Th>}
            <Th>Phone</Th>
            <Th>Joined</Th>
            <Th className="text-right">Actions</Th>
          </tr>
        </thead>
        <tbody>
          {customers.map((c) => (
            <tr key={c.id} className="hover:bg-slate-50 dark:hover:bg-slate-700/40">
              <Td>
                <div>
                  <p className="font-medium text-slate-800 dark:text-slate-100">{c.fullName}</p>
                  <p className="text-xs text-slate-400">{c.email}</p>
                </div>
              </Td>
              {showClientColumn && (
                <Td>
                  {c.clientName
                    ? <Badge label={c.clientName} color="blue" />
                    : <span className="text-xs text-slate-400">—</span>}
                </Td>
              )}
              <Td className="text-slate-500">{c.phone}</Td>
              <Td className="text-slate-500">{new Date(c.joinedDate).toLocaleDateString()}</Td>
              <Td className="text-right">
                <Link to={`/customers/${c.id}`}>
                  <Button variant="ghost" size="sm"><Eye size={14} /></Button>
                </Link>
              </Td>
            </tr>
          ))}
        </tbody>
      </Table>
    </div>
  )
}
