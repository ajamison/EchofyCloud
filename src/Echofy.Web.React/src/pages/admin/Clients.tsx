import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { Edit, Plus } from 'lucide-react'
import { api } from '../../lib/api'
import { useAuth } from '../../lib/auth'
import { Button, PageHeader, Table, Th, Td, Badge, Modal, FormField, Input, PageSpinner } from '../../components/ui'
import type { Client } from '../../types'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'

const schema = z.object({
  name: z.string().min(1), slug: z.string().min(1),
  hasECommerce: z.boolean().default(false), hasCrm: z.boolean().default(false),
  hasKanban: z.boolean().default(false), hasCalendar: z.boolean().default(false),
  hasChat: z.boolean().default(false), isActive: z.boolean().default(true),
  allowCompanyRewardOverride: z.boolean().default(true),
})
type FormData = z.infer<typeof schema>

export default function AdminClients() {
  const { canWrite } = useAuth()
  const [editing, setEditing] = useState<Client | null>(null)
  const [showCreate, setShowCreate] = useState(false)
  const qc = useQueryClient()

  const { data: clients = [], isLoading } = useQuery<Client[]>({
    queryKey: ['admin-clients'],
    queryFn: () => api.get('/admin/clients').then((r) => r.data),
  })

  const form = useForm<FormData>({ resolver: zodResolver(schema) })

  const saveMutation = useMutation({
    mutationFn: (data: FormData) =>
      editing ? api.put(`/admin/clients/${editing.id}`, data) : api.post('/admin/clients', data),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ['admin-clients'] }); setEditing(null); setShowCreate(false); form.reset() },
  })

  const modules = [
    { key: 'hasECommerce', label: 'E-Commerce' },
    { key: 'hasCrm', label: 'CRM' },
    { key: 'hasKanban', label: 'Kanban' },
    { key: 'hasCalendar', label: 'Calendar' },
    { key: 'hasChat', label: 'Chat' },
  ] as const

  if (isLoading) return <PageSpinner />

  return (
    <div>
      <PageHeader title="Clients" actions={canWrite() && <Button onClick={() => { setEditing(null); form.reset({ isActive: true }); setShowCreate(true) }}><Plus size={15} /> Add Client</Button>} />
      <Table>
        <thead><tr><Th>Name</Th><Th>Slug</Th><Th>Modules</Th><Th>Status</Th><Th className="text-right">Actions</Th></tr></thead>
        <tbody>
          {clients.map((c) => (
            <tr key={c.id} className="hover:bg-slate-50">
              <Td className="font-medium">{c.name}</Td>
              <Td className="font-mono text-xs text-slate-500">{c.slug}</Td>
              <Td>
                <div className="flex flex-wrap gap-1">
                  {c.hasECommerce && <Badge label="E-Commerce" color="blue" />}
                  {c.hasCrm && <Badge label="CRM" color="purple" />}
                  {c.hasKanban && <Badge label="Kanban" color="slate" />}
                  {c.hasCalendar && <Badge label="Calendar" color="yellow" />}
                  {c.hasChat && <Badge label="Chat" color="green" />}
                </div>
              </Td>
              <Td><Badge label={c.isActive ? 'Active' : 'Inactive'} color={c.isActive ? 'green' : 'slate'} /></Td>
              <Td className="text-right">
                {canWrite() && (
                  <Button variant="ghost" size="sm" onClick={() => { setEditing(c); form.reset(c); }}><Edit size={14} /></Button>
                )}
              </Td>
            </tr>
          ))}
        </tbody>
      </Table>
      <Modal open={!!(editing || showCreate)} onClose={() => { setEditing(null); setShowCreate(false) }} title={editing ? 'Edit Client' : 'New Client'}>
        <form onSubmit={form.handleSubmit((d) => saveMutation.mutate(d))} className="space-y-4">
          <FormField label="Name" required><Input {...form.register('name')} /></FormField>
          <FormField label="Slug" required><Input {...form.register('slug')} /></FormField>
          <div className="grid grid-cols-2 gap-2">
            {modules.map(({ key, label }) => (
              <label key={key} className="flex items-center gap-2 text-sm">
                <input type="checkbox" {...form.register(key)} /> {label}
              </label>
            ))}
          </div>
          <label className="flex items-center gap-2 text-sm"><input type="checkbox" {...form.register('isActive')} /> Active</label>
          <label className="flex items-center gap-2 text-sm"><input type="checkbox" {...form.register('allowCompanyRewardOverride')} /> Allow companies to override reward programs</label>
          <div className="flex justify-end gap-2 pt-2">
            <Button variant="secondary" type="button" onClick={() => { setEditing(null); setShowCreate(false) }}>Cancel</Button>
            <Button type="submit" loading={form.formState.isSubmitting}>Save</Button>
          </div>
        </form>
      </Modal>
    </div>
  )
}
