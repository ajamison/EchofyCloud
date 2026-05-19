import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { Edit, Plus, Trash2 } from 'lucide-react'
import { api } from '../../lib/api'
import { useAuth } from '../../lib/auth'
import { Button, PageHeader, Table, Th, Td, Badge, Modal, FormField, Input, PageSpinner } from '../../components/ui'
import type { Category } from '../../types'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'

const schema = z.object({ name: z.string().min(1), slug: z.string().min(1), description: z.string().optional(), isActive: z.boolean().default(true) })
type FormData = z.infer<typeof schema>

export default function AdminCategories() {
  const { canWrite } = useAuth()
  const [editing, setEditing] = useState<Category | null>(null)
  const [showCreate, setShowCreate] = useState(false)
  const qc = useQueryClient()

  const { data: categories = [], isLoading } = useQuery<Category[]>({
    queryKey: ['admin-categories'],
    queryFn: () => api.get('/admin/categories').then((r) => r.data),
  })

  const form = useForm<FormData>({ resolver: zodResolver(schema), defaultValues: { isActive: true } })

  const saveMutation = useMutation({
    mutationFn: (data: FormData) =>
      editing ? api.put(`/admin/categories/${editing.id}`, data) : api.post('/admin/categories', data),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ['admin-categories'] }); setEditing(null); setShowCreate(false); form.reset() },
  })

  const deleteMutation = useMutation({
    mutationFn: (id: number) => api.delete(`/admin/categories/${id}`),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['admin-categories'] }),
  })

  const openEdit = (cat: Category) => { setEditing(cat); form.reset({ ...cat, description: cat.description ?? undefined }) }
  const openCreate = () => { setEditing(null); form.reset({ isActive: true }); setShowCreate(true) }

  if (isLoading) return <PageSpinner />

  return (
    <div>
      <PageHeader title="Categories" actions={canWrite() && <Button onClick={openCreate}><Plus size={15} /> Add Category</Button>} />
      <Table>
        <thead><tr><Th>Name</Th><Th>Slug</Th><Th>Products</Th><Th>Status</Th><Th className="text-right">Actions</Th></tr></thead>
        <tbody>
          {categories.map((c) => (
            <tr key={c.id} className="hover:bg-slate-50">
              <Td className="font-medium">{c.name}</Td>
              <Td className="text-slate-500 font-mono text-xs">{c.slug}</Td>
              <Td>{c.productCount}</Td>
              <Td><Badge label={c.isActive ? 'Active' : 'Inactive'} color={c.isActive ? 'green' : 'slate'} /></Td>
              <Td className="text-right">
                <div className="flex justify-end gap-1">
                  {canWrite() && (
                    <>
                      <Button variant="ghost" size="sm" onClick={() => openEdit(c)}><Edit size={14} /></Button>
                      <Button variant="ghost" size="sm" onClick={() => deleteMutation.mutate(c.id)}><Trash2 size={14} className="text-red-500" /></Button>
                    </>
                  )}
                </div>
              </Td>
            </tr>
          ))}
        </tbody>
      </Table>
      <Modal open={!!(editing || showCreate)} onClose={() => { setEditing(null); setShowCreate(false) }} title={editing ? 'Edit Category' : 'New Category'}>
        <form onSubmit={form.handleSubmit((d) => saveMutation.mutate(d))} className="space-y-4">
          <FormField label="Name" error={form.formState.errors.name?.message} required><Input {...form.register('name')} /></FormField>
          <FormField label="Slug" error={form.formState.errors.slug?.message} required><Input {...form.register('slug')} /></FormField>
          <FormField label="Description"><Input {...form.register('description')} /></FormField>
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
