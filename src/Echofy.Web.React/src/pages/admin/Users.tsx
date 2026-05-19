import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { Plus, Edit, KeyRound } from 'lucide-react'
import { api } from '../../lib/api'
import { useAuth } from '../../lib/auth'
import { Button, PageHeader, Table, Th, Td, Badge, Modal, FormField, Input, Select, PageSpinner } from '../../components/ui'
import type { AppUser, Client } from '../../types'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'

const createSchema = z.object({
  fullName: z.string().min(1, 'Required'),
  email: z.string().email(),
  password: z.string().min(6, 'Min 6 chars'),
  role: z.string().optional(),
  clientId: z.coerce.number().optional().nullable(),
})
type CreateForm = z.infer<typeof createSchema>

const editSchema = z.object({
  fullName: z.string().min(1, 'Required'),
  role: z.string().optional(),
  clientId: z.coerce.number().optional().nullable(),
})
type EditForm = z.infer<typeof editSchema>

const passwordSchema = z.object({
  newPassword: z.string().min(6, 'Min 6 chars'),
  confirmPassword: z.string().min(6, 'Min 6 chars'),
}).refine((d) => d.newPassword === d.confirmPassword, {
  message: 'Passwords do not match',
  path: ['confirmPassword'],
})
type PasswordForm = z.infer<typeof passwordSchema>

export default function AdminUsers() {
  const { canWrite } = useAuth()
  const [showCreate, setShowCreate] = useState(false)
  const [editing, setEditing] = useState<AppUser | null>(null)
  const [passwordTarget, setPasswordTarget] = useState<AppUser | null>(null)
  const qc = useQueryClient()

  const { data: users = [], isLoading } = useQuery<AppUser[]>({
    queryKey: ['admin-users'],
    queryFn: () => api.get('/admin/users').then((r) => r.data),
  })
  const { data: roles = [] } = useQuery<string[]>({
    queryKey: ['admin-roles'],
    queryFn: () => api.get('/admin/roles').then((r) => r.data),
  })
  const { data: clients = [] } = useQuery<Client[]>({
    queryKey: ['admin-clients'],
    queryFn: () => api.get('/admin/clients').then((r) => r.data),
  })

  const createForm = useForm<CreateForm>({ resolver: zodResolver(createSchema) })
  const editForm = useForm<EditForm>({ resolver: zodResolver(editSchema) })
  const passwordForm = useForm<PasswordForm>({ resolver: zodResolver(passwordSchema) })

  const createMutation = useMutation({
    mutationFn: (data: CreateForm) => api.post('/admin/users', data),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ['admin-users'] }); setShowCreate(false); createForm.reset() },
  })

  const editMutation = useMutation({
    mutationFn: (data: EditForm) => api.put(`/admin/users/${editing!.id}`, data),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ['admin-users'] }); setEditing(null) },
  })

  const passwordMutation = useMutation({
    mutationFn: (data: PasswordForm) => api.post(`/admin/users/${passwordTarget!.id}/reset-password`, { newPassword: data.newPassword }),
    onSuccess: () => { setPasswordTarget(null); passwordForm.reset() },
  })

  function openEdit(u: AppUser) {
    editForm.reset({ fullName: u.fullName, role: u.role ?? '', clientId: u.clientId ?? null })
    setEditing(u)
  }

  function openPasswordChange(u: AppUser) {
    passwordForm.reset()
    setPasswordTarget(u)
  }

  if (isLoading) return <PageSpinner />

  return (
    <div>
      <PageHeader title="Users" subtitle={`${users.length} users`} actions={canWrite() && <Button onClick={() => setShowCreate(true)}><Plus size={15} /> Create User</Button>} />
      <Table>
        <thead>
          <tr>
            <Th>Name</Th>
            <Th>Email</Th>
            <Th>Role</Th>
            <Th>Client</Th>
            {canWrite() && <Th className="text-right">Actions</Th>}
          </tr>
        </thead>
        <tbody>
          {users.map((u) => (
            <tr key={u.id} className="hover:bg-slate-50">
              <Td className="font-medium text-slate-800">{u.fullName}</Td>
              <Td className="text-slate-500">{u.email}</Td>
              <Td><Badge label={u.role || '—'} color="blue" /></Td>
              <Td className="text-slate-500">{u.clientName ?? '—'}</Td>
              {canWrite() && (
                <Td className="text-right">
                  <div className="flex justify-end gap-1">
                    <Button variant="ghost" size="sm" onClick={() => openEdit(u)}><Edit size={14} /></Button>
                    <Button variant="ghost" size="sm" onClick={() => openPasswordChange(u)}><KeyRound size={14} /></Button>
                  </div>
                </Td>
              )}
            </tr>
          ))}
        </tbody>
      </Table>

      {/* Create Modal */}
      <Modal open={showCreate} onClose={() => setShowCreate(false)} title="Create User">
        <form onSubmit={createForm.handleSubmit((d) => createMutation.mutate(d))} className="space-y-4">
          <FormField label="Full Name" error={createForm.formState.errors.fullName?.message} required><Input {...createForm.register('fullName')} /></FormField>
          <FormField label="Email" error={createForm.formState.errors.email?.message} required><Input type="email" {...createForm.register('email')} /></FormField>
          <FormField label="Password" error={createForm.formState.errors.password?.message} required><Input type="password" {...createForm.register('password')} /></FormField>
          <FormField label="Role">
            <Select {...createForm.register('role')}>
              <option value="">— None —</option>
              {roles.map((r) => <option key={r} value={r}>{r}</option>)}
            </Select>
          </FormField>
          <FormField label="Client">
            <Select {...createForm.register('clientId')}>
              <option value="">— None —</option>
              {clients.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
            </Select>
          </FormField>
          <div className="flex justify-end gap-2 pt-2">
            <Button variant="secondary" type="button" onClick={() => setShowCreate(false)}>Cancel</Button>
            <Button type="submit" loading={createForm.formState.isSubmitting}>Create</Button>
          </div>
        </form>
      </Modal>

      {/* Edit Modal */}
      <Modal open={!!editing} onClose={() => setEditing(null)} title={`Edit ${editing?.fullName ?? 'User'}`}>
        <form onSubmit={editForm.handleSubmit((d) => editMutation.mutate(d))} className="space-y-4">
          <FormField label="Full Name" error={editForm.formState.errors.fullName?.message} required><Input {...editForm.register('fullName')} /></FormField>
          <FormField label="Email">
            <Input value={editing?.email ?? ''} disabled className="bg-slate-50 text-slate-400" />
          </FormField>
          <FormField label="Role">
            <Select {...editForm.register('role')}>
              <option value="">— None —</option>
              {roles.map((r) => <option key={r} value={r}>{r}</option>)}
            </Select>
          </FormField>
          <FormField label="Client">
            <Select {...editForm.register('clientId')}>
              <option value="">— None —</option>
              {clients.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
            </Select>
          </FormField>
          <div className="flex justify-end gap-2 pt-2">
            <Button variant="secondary" type="button" onClick={() => setEditing(null)}>Cancel</Button>
            <Button type="submit" loading={editForm.formState.isSubmitting}>Save</Button>
          </div>
        </form>
      </Modal>

      {/* Change Password Modal */}
      <Modal open={!!passwordTarget} onClose={() => setPasswordTarget(null)} title={`Change Password — ${passwordTarget?.fullName ?? ''}`}>
        <form onSubmit={passwordForm.handleSubmit((d) => passwordMutation.mutate(d))} className="space-y-4">
          <FormField label="New Password" error={passwordForm.formState.errors.newPassword?.message} required>
            <Input type="password" {...passwordForm.register('newPassword')} />
          </FormField>
          <FormField label="Confirm Password" error={passwordForm.formState.errors.confirmPassword?.message} required>
            <Input type="password" {...passwordForm.register('confirmPassword')} />
          </FormField>
          <div className="flex justify-end gap-2 pt-2">
            <Button variant="secondary" type="button" onClick={() => setPasswordTarget(null)}>Cancel</Button>
            <Button type="submit" loading={passwordForm.formState.isSubmitting}>Change Password</Button>
          </div>
        </form>
      </Modal>
    </div>
  )
}
