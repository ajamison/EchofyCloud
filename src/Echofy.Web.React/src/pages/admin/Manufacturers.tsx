import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { Edit, Plus, Trash2 } from 'lucide-react'
import { api } from '../../lib/api'
import { useAuth } from '../../lib/auth'
import { Button, PageHeader, Table, Th, Td, Badge, Modal, FormField, Input, PageSpinner } from '../../components/ui'
import type { Manufacturer } from '../../types'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'

const schema = z.object({ name: z.string().min(1), website: z.string().optional(), isActive: z.boolean().default(true) })
type FormData = z.infer<typeof schema>

export default function AdminManufacturers() {
  const { canWrite } = useAuth()
  const [editing, setEditing] = useState<Manufacturer | null>(null)
  const [showCreate, setShowCreate] = useState(false)
  const qc = useQueryClient()

  const { data: manufacturers = [], isLoading } = useQuery<Manufacturer[]>({
    queryKey: ['admin-manufacturers'],
    queryFn: () => api.get('/admin/manufacturers').then((r) => r.data),
  })

  const form = useForm<FormData>({ resolver: zodResolver(schema), defaultValues: { isActive: true } })

  const saveMutation = useMutation({
    mutationFn: (data: FormData) =>
      editing ? api.put(`/admin/manufacturers/${editing.id}`, data) : api.post('/admin/manufacturers', data),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ['admin-manufacturers'] }); setEditing(null); setShowCreate(false); form.reset() },
  })

  const deleteMutation = useMutation({
    mutationFn: (id: number) => api.delete(`/admin/manufacturers/${id}`),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['admin-manufacturers'] }),
  })

  if (isLoading) return <PageSpinner />

  return (
    <div>
      <PageHeader title="Manufacturers" actions={canWrite() && <Button onClick={() => { setEditing(null); form.reset({ isActive: true }); setShowCreate(true) }}><Plus size={15} /> Add Manufacturer</Button>} />
      <Table>
        <thead><tr><Th>Name</Th><Th>Website</Th><Th>Status</Th><Th className="text-right">Actions</Th></tr></thead>
        <tbody>
          {manufacturers.map((m) => (
            <tr key={m.id} className="hover:bg-slate-50">
              <Td className="font-medium">{m.name}</Td>
              <Td className="text-slate-500">{m.website ?? '—'}</Td>
              <Td><Badge label={m.isActive ? 'Active' : 'Inactive'} color={m.isActive ? 'green' : 'slate'} /></Td>
              <Td className="text-right">
                <div className="flex justify-end gap-1">
                  {canWrite() && (
                    <>
                      <Button variant="ghost" size="sm" onClick={() => { setEditing(m); form.reset({ ...m, website: m.website ?? undefined }) }}><Edit size={14} /></Button>
                      <Button variant="ghost" size="sm" onClick={() => deleteMutation.mutate(m.id)}><Trash2 size={14} className="text-red-500" /></Button>
                    </>
                  )}
                </div>
              </Td>
            </tr>
          ))}
        </tbody>
      </Table>
      <Modal open={!!(editing || showCreate)} onClose={() => { setEditing(null); setShowCreate(false) }} title={editing ? 'Edit Manufacturer' : 'New Manufacturer'}>
        <form onSubmit={form.handleSubmit((d) => saveMutation.mutate(d))} className="space-y-4">
          <FormField label="Name" error={form.formState.errors.name?.message} required><Input {...form.register('name')} /></FormField>
          <FormField label="Website"><Input type="url" {...form.register('website')} /></FormField>
          <label className="flex items-center gap-2 text-sm"><input type="checkbox" {...form.register('isActive')} /> Active</label>
          <div className="flex justify-end gap-2 pt-2">
            <Button variant="secondary" type="button" onClick={() => { setEditing(null); setShowCreate(false) }}>Cancel</Button>
            <Button type="submit" loading={form.formState.isSubmitting}>Save</Button>
          </div>
        </form>
      </Modal>
    </div>
  )
}
