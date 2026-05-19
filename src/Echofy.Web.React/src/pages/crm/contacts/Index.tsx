import { useQuery } from '@tanstack/react-query'
import { Plus } from 'lucide-react'
import { api } from '../../../lib/api'
import { useAuth } from '../../../lib/auth'
import { Button, PageHeader, Table, Th, Td, PageSpinner } from '../../../components/ui'
import type { Contact } from '../../../types'

export default function Contacts() {
  const { canWrite } = useAuth()
  const { data: contacts = [], isLoading } = useQuery<Contact[]>({
    queryKey: ['contacts'],
    queryFn: () => api.get('/contacts').then((r) => r.data),
  })

  if (isLoading) return <PageSpinner />

  return (
    <div>
      <PageHeader title="Contacts" subtitle={`${contacts.length} contacts`} actions={canWrite() && <Button><Plus size={15} /> Add Contact</Button>} />
      <Table>
        <thead>
          <tr><Th>Name</Th><Th>Email</Th><Th>Phone</Th><Th>Company</Th><Th>Added</Th></tr>
        </thead>
        <tbody>
          {contacts.map((c) => (
            <tr key={c.id} className="hover:bg-slate-50">
              <Td className="font-medium text-slate-800">{c.fullName}</Td>
              <Td className="text-slate-500">{c.email}</Td>
              <Td className="text-slate-500">{c.phone ?? '—'}</Td>
              <Td className="text-slate-500">{c.company ?? '—'}</Td>
              <Td className="text-slate-500">{new Date(c.createdAt).toLocaleDateString()}</Td>
            </tr>
          ))}
        </tbody>
      </Table>
    </div>
  )
}
