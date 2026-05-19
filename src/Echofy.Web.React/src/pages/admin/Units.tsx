import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { Edit, Plus, Trash2 } from 'lucide-react'
import { api } from '../../lib/api'
import { useAuth } from '../../lib/auth'
import { Button, PageHeader, Table, Th, Td, Badge, Modal, FormField, Input, PageSpinner } from '../../components/ui'
import type { UnitOfMeasure } from '../../types'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'

const schema = z.object({ name: z.string().min(1), abbreviation: z.string().min(1), isActive: z.boolean().default(true) })
type FormData = z.infer<typeof schema>

export default function AdminUnits() {
  const { canWrite } = useAuth()
  const [editing, setEditing] = useState<UnitOfMeasure | null>(null)
  const [showCreate, setShowCreate] = useState(false)
  const qc = useQueryClient()

  const { data: units = [], isLoading } = useQuery<UnitOfMeasure[]>({
    queryKey: ['admin-units'],
    queryFn: () => api.get('/admin/units').then((r) => r.data),
  })

  const form = useForm<FormData>({ resolver: zodResolver(schema), defaultValues: { isActive: true } })

  const saveMutation = useMutation({
    mutationFn: (data: FormData) =>
      editing ? api.put(`/admin/units/${editing.id}`, data) : api.post('/admin/units', data),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ['admin-units'] }); setEditing(null); setShowCreate(false); form.reset() },
  })

  const deleteMutation = useMutation({
    mutationFn: (id: number) => api.delete(`/admin/units/${id}`),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['admin-units'] }),
  })

  if (isLoading) return <PageSpinner />

  return (
    <div>
      <PageHeader title="Units of Measure" actions={canWrite() && <Button onClick={() => { setEditing(null); form.reset({ isActive: true }); setShowCreate(true) }}><Plus size={15} /> Add Unit</Button>} />
      <Table>
        <thead><tr><Th>Name</Th><Th>Abbreviation</Th><Th>Status</Th><Th className="text-right">Actions</Th></tr></thead>
        <tbody>
          {units.map((u) => (
            <tr key={u.id} className="hover:bg-slate-50">
              <Td className="font-medium">{u.name}</Td>
              <Td className="font-mono text-slate-500">{u.abbreviation}</Td>
              <Td><Badge label={u.isActive ? 'Active' : 'Inactive'} color={u.isActive ? 'green' : 'slate'} /></Td>
              <Td className="text-right">
                <div className="flex justify-end gap-1">
                  {canWrite() && (
                    <>
                      <Button variant="ghost" size="sm" onClick={() => { setEditing(u); form.reset(u) }}><Edit size={14} /></Button>
                      <Button variant="ghost" size="sm" onClick={() => deleteMutation.mutate(u.id)}><Trash2 size={14} className="text-red-500" /></Button>
                    </>
                  )}
                </div>
              </Td>
            </tr>
          ))}
        </tbody>
      </Table>
      <Modal open={!!(editing || showCreate)} onClose={() => { setEditing(null); setShowCreate(false) }} title={editing ? 'Edit Unit' : 'New Unit'}>
        <form onSubmit={form.handleSubmit((d) => saveMutation.mutate(d))} className="space-y-4">
          <FormField label="Name" required><Input {...form.register('name')} /></FormField>
          <FormField label="Abbreviation" required><Input {...form.register('abbreviation')} /></FormField>
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
