import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { Plus, Eye } from 'lucide-react'
import { api } from '../../../lib/api'
import { useAuth } from '../../../lib/auth'
import { Button, PageHeader, Table, Th, Td, Badge, PageSpinner } from '../../../components/ui'
import type { Lead } from '../../../types'
import { LeadStatus } from '../../../types'

const statusColors: Record<number, 'blue' | 'yellow' | 'green' | 'red' | 'slate' | 'purple'> = {
  [LeadStatus.New]: 'blue', [LeadStatus.Contacted]: 'yellow',
  [LeadStatus.Qualified]: 'green', [LeadStatus.Lost]: 'red', [LeadStatus.Converted]: 'purple',
}

export default function Leads() {
  const { canWrite } = useAuth()
  const { data: leads = [], isLoading } = useQuery<Lead[]>({
    queryKey: ['leads'],
    queryFn: () => api.get('/leads').then((r) => r.data),
  })

  if (isLoading) return <PageSpinner />

  return (
    <div>
      <PageHeader
        title="Leads"
        subtitle={`${leads.length} leads`}
        actions={canWrite() && <Link to="/crm/leads/new"><Button><Plus size={15} /> Add Lead</Button></Link>}
      />
      <Table>
        <thead>
          <tr>
            <Th>Lead</Th>
            <Th>Company</Th>
            <Th>Status</Th>
            <Th>Est. Value</Th>
            <Th>Assigned To</Th>
            <Th>Deals</Th>
            <Th className="text-right">Actions</Th>
          </tr>
        </thead>
        <tbody>
          {leads.map((l) => (
            <tr key={l.id} className="hover:bg-slate-50">
              <Td>
                <div>
                  <p className="font-medium text-slate-800">{l.fullName}</p>
                  <p className="text-xs text-slate-400">{l.email}</p>
                </div>
              </Td>
              <Td className="text-slate-500">{l.company ?? '—'}</Td>
              <Td><Badge label={LeadStatus[l.status]} color={statusColors[l.status]} /></Td>
              <Td>${l.estimatedValue.toLocaleString()}</Td>
              <Td className="text-slate-500">{l.assignedTo ?? '—'}</Td>
              <Td>{l.dealCount}</Td>
              <Td className="text-right">
                <Button variant="ghost" size="sm"><Eye size={14} /></Button>
              </Td>
            </tr>
          ))}
        </tbody>
      </Table>
    </div>
  )
}
