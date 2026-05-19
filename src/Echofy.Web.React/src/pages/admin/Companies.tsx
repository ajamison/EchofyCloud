import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { Edit, Plus, Trash2 } from 'lucide-react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { api } from '../../lib/api'
import { useAuth } from '../../lib/auth'
import { Button, PageHeader, Table, Th, Td, Badge, Modal, FormField, Input, PageSpinner } from '../../components/ui'
import type { Company, Client } from '../../types'

const schema = z.object({
  clientId:  z.coerce.number({ required_error: 'Client is required' }).int().positive('Client is required'),
  name:      z.string().min(1, 'Name is required'),
  email:     z.string().email('Invalid email').optional().or(z.literal('')),
  phone:     z.string().optional(),
  website:   z.string().optional(),
  taxNumber: z.string().optional(),
  address:   z.string().optional(),
  city:      z.string().optional(),
  country:   z.string().optional(),
  isActive:  z.boolean().default(true),
})
type FormData = z.infer<typeof schema>

export default function AdminCompanies() {
  const { canWrite } = useAuth()
  const [editing, setEditing]       = useState<Company | null>(null)
  const [showCreate, setShowCreate] = useState(false)
  const qc = useQueryClient()

  const { data: companies = [], isLoading } = useQuery<Company[]>({
    queryKey: ['admin-companies'],
    queryFn: () => api.get('/admin/companies').then(r => r.data),
  })

  const { data: clients = [] } = useQuery<Client[]>({
    queryKey: ['admin-clients'],
    queryFn: () => api.get('/admin/clients').then(r => r.data),
  })

  const form = useForm<FormData>({ resolver: zodResolver(schema), defaultValues: { isActive: true } })

  const closeModal = () => { setEditing(null); setShowCreate(false); form.reset() }

  const saveMutation = useMutation({
    mutationFn: (data: FormData) =>
      editing
        ? api.put(`/admin/companies/${editing.id}`, data)
        : api.post('/admin/companies', data),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ['admin-companies'] }); closeModal() },
  })

  const deleteMutation = useMutation({
    mutationFn: (id: number) => api.delete(`/admin/companies/${id}`),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['admin-companies'] }),
  })

  const openCreate = () => {
    setEditing(null)
    form.reset({ isActive: true })
    setShowCreate(true)
  }

  const openEdit = (c: Company) => {
    setEditing(c)
    form.reset({
      clientId:  c.clientId,
      name:      c.name,
      email:     c.email ?? '',
      phone:     c.phone ?? '',
      website:   c.website ?? '',
      taxNumber: c.taxNumber ?? '',
      address:   c.address ?? '',
      city:      c.city ?? '',
      country:   c.country ?? '',
      isActive:  c.isActive,
    })
    setShowCreate(false)
  }

  if (isLoading) return <PageSpinner />

  return (
    <div>
      <PageHeader
        title="Companies"
        subtitle={`${companies.length} companies`}
        actions={canWrite() && <Button onClick={openCreate}><Plus size={15} /> Add Company</Button>}
      />

      <Table>
        <thead>
          <tr>
            <Th>Name</Th>
            <Th>Client</Th>
            <Th>Contact</Th>
            <Th>Location</Th>
            <Th>Products</Th>
            <Th>Invoices</Th>
            <Th>Status</Th>
            <Th className="text-right">Actions</Th>
          </tr>
        </thead>
        <tbody>
          {companies.length === 0 && (
            <tr>
              <td colSpan={8} className="py-12 text-center text-sm text-slate-400">
                No companies yet. Add your first one.
              </td>
            </tr>
          )}
          {companies.map(c => (
            <tr key={c.id} className="hover:bg-slate-50 dark:hover:bg-slate-700/30">
              <Td className="font-medium text-slate-800 dark:text-slate-200">{c.name}</Td>
              <Td>
                {c.clientName
                  ? <Badge label={c.clientName} color="blue" />
                  : <span className="text-xs text-slate-400">—</span>}
              </Td>
              <Td className="text-slate-500 text-sm">
                {c.email && <div>{c.email}</div>}
                {c.phone && <div>{c.phone}</div>}
                {!c.email && !c.phone && '—'}
              </Td>
              <Td className="text-slate-500 text-sm">
                {[c.city, c.country].filter(Boolean).join(', ') || '—'}
              </Td>
              <Td className="text-slate-500">{c.productCount}</Td>
              <Td className="text-slate-500">{c.invoiceCount}</Td>
              <Td>
                <Badge label={c.isActive ? 'Active' : 'Inactive'} color={c.isActive ? 'green' : 'slate'} />
              </Td>
              <Td className="text-right">
                <div className="flex justify-end gap-1">
                  {canWrite() && (
                    <>
                      <Button variant="ghost" size="sm" onClick={() => openEdit(c)}><Edit size={14} /></Button>
                      <Button variant="ghost" size="sm" onClick={() => deleteMutation.mutate(c.id)}>
                        <Trash2 size={14} className="text-red-500" />
                      </Button>
                    </>
                  )}
                </div>
              </Td>
            </tr>
          ))}
        </tbody>
      </Table>

      <Modal
        open={!!(editing || showCreate)}
        onClose={closeModal}
        title={editing ? `Edit ${editing.name}` : 'New Company'}
      >
        <form onSubmit={form.handleSubmit(d => saveMutation.mutate(d))} className="space-y-4">
          <FormField label="Client" error={form.formState.errors.clientId?.message} required>
            <select
              {...form.register('clientId')}
              className="w-full rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm text-slate-800 focus:outline-none focus:ring-2 focus:ring-primary dark:border-slate-600 dark:bg-slate-700 dark:text-slate-200"
            >
              <option value="">— Select client —</option>
              {clients.filter(cl => cl.isActive).map(cl => (
                <option key={cl.id} value={cl.id}>{cl.name}</option>
              ))}
            </select>
          </FormField>

          <FormField label="Name" error={form.formState.errors.name?.message} required>
            <Input {...form.register('name')} placeholder="Acme Corp" />
          </FormField>

          <div className="grid grid-cols-2 gap-3">
            <FormField label="Email" error={form.formState.errors.email?.message}>
              <Input type="email" {...form.register('email')} placeholder="info@acme.com" />
            </FormField>
            <FormField label="Phone">
              <Input {...form.register('phone')} placeholder="+1 555 000 0000" />
            </FormField>
          </div>

          <div className="grid grid-cols-2 gap-3">
            <FormField label="Website">
              <Input {...form.register('website')} placeholder="https://acme.com" />
            </FormField>
            <FormField label="Tax Number">
              <Input {...form.register('taxNumber')} placeholder="Tax ID / VAT" />
            </FormField>
          </div>

          <FormField label="Address">
            <Input {...form.register('address')} placeholder="123 Main St" />
          </FormField>

          <div className="grid grid-cols-2 gap-3">
            <FormField label="City">
              <Input {...form.register('city')} placeholder="New York" />
            </FormField>
            <FormField label="Country">
              <Input {...form.register('country')} placeholder="United States" />
            </FormField>
          </div>

          <label className="flex items-center gap-2 text-sm">
            <input type="checkbox" {...form.register('isActive')} className="rounded" />
            Active
          </label>

          <div className="flex justify-end gap-2 pt-2">
            <Button variant="secondary" type="button" onClick={closeModal}>Cancel</Button>
            <Button type="submit" loading={saveMutation.isPending || form.formState.isSubmitting}>Save</Button>
          </div>
        </form>
      </Modal>
    </div>
  )
}
