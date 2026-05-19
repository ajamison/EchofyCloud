import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api } from '../../../lib/api'
import { useAuth } from '../../../lib/auth'
import { PageHeader, Card, FormField, Input, Textarea, Button, PageSpinner } from '../../../components/ui'
import type { Customer, Client } from '../../../types'

const schema = z.object({
  fullName:  z.string().min(1, 'Required'),
  email:     z.string().email('Invalid email'),
  phone:     z.string().optional(),
  notes:     z.string().optional(),
  street:    z.string().optional(),
  city:      z.string().optional(),
  province:  z.string().optional(),
  country:   z.string().optional(),
  clientId:  z.coerce.number().nullable().optional(),
})
type FormData = z.infer<typeof schema>

export default function CustomerForm() {
  const { id } = useParams<{ id: string }>()
  const isEdit = !!id
  const navigate = useNavigate()
  const qc = useQueryClient()
  const { user } = useAuth()
  const isAdmin = user?.role === 'Admin'
  const [apiError, setApiError] = useState<string | null>(null)

  const { data: existing, isLoading: loadingCustomer } = useQuery<Customer>({
    queryKey: ['customer', id],
    queryFn: () => api.get(`/customers/${id}`).then(r => r.data),
    enabled: isEdit,
  })

  const { data: clients = [] } = useQuery<Client[]>({
    queryKey: ['admin-clients'],
    queryFn: () => api.get('/admin/clients').then(r => r.data),
    enabled: isAdmin,
  })

  const { register, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { clientId: user?.clientId ?? null },
  })

  useEffect(() => {
    if (existing) {
      reset({
        fullName:  existing.fullName,
        email:     existing.email,
        phone:     existing.phone ?? '',
        notes:     existing.notes ?? '',
        street:    existing.street ?? '',
        city:      existing.city ?? '',
        province:  existing.province ?? '',
        country:   existing.country ?? '',
        clientId:  existing.clientId,
      })
    }
  }, [existing, reset])

  const mutation = useMutation({
    mutationFn: (data: FormData) => isEdit
      ? api.put(`/customers/${id}`, data)
      : api.post('/customers', data),
    onSuccess: (res) => {
      setApiError(null)
      qc.invalidateQueries({ queryKey: ['customers'] })
      const customerId = isEdit ? id : res.data?.id
      navigate(`/customers/${customerId}`)
    },
    onError: (err: unknown) => {
      const msg = (err as { response?: { data?: { message?: string }; status?: number } })?.response
      setApiError(msg?.data?.message ?? `Request failed (${msg?.status ?? 'network error'})`)
    },
  })

  if (isEdit && loadingCustomer) return <PageSpinner />

  return (
    <div className="space-y-6">
      <PageHeader
        title={isEdit ? `Edit ${existing?.fullName ?? '…'}` : 'New Customer'}
        subtitle={isEdit ? 'Update customer details.' : 'Add a new customer to your account.'}
      />

      <form onSubmit={handleSubmit(d => mutation.mutate(d))} className="space-y-6">
        {/* Client assignment — only Admin can choose; Managers auto-assigned */}
        {isAdmin && (
          <Card>
            <h2 className="mb-4 font-semibold text-slate-800 dark:text-slate-100">Client Assignment</h2>
            <FormField label="Assign to Client" error={errors.clientId?.message}>
              <select
                {...register('clientId')}
                className="w-full rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm text-slate-800 focus:outline-none focus:ring-2 focus:ring-primary dark:border-slate-600 dark:bg-slate-700 dark:text-slate-200"
              >
                <option value="">— No client (global) —</option>
                {clients.filter(c => c.isActive).map(c => (
                  <option key={c.id} value={c.id}>{c.name}</option>
                ))}
              </select>
            </FormField>
          </Card>
        )}

        {/* Basic info */}
        <Card>
          <h2 className="mb-4 font-semibold text-slate-800 dark:text-slate-100">Contact Details</h2>
          <div className="grid gap-4 sm:grid-cols-2">
            <FormField label="Full Name" error={errors.fullName?.message} required>
              <Input placeholder="Jane Smith" {...register('fullName')} />
            </FormField>
            <FormField label="Email" error={errors.email?.message} required>
              <Input type="email" placeholder="jane@example.com" {...register('email')} />
            </FormField>
            <FormField label="Phone" error={errors.phone?.message}>
              <Input placeholder="+1 555 000 0000" {...register('phone')} />
            </FormField>
          </div>
          <div className="mt-4">
            <FormField label="Notes" error={errors.notes?.message}>
              <Textarea placeholder="Any internal notes…" rows={3} {...register('notes')} />
            </FormField>
          </div>
        </Card>

        {/* Address */}
        <Card>
          <h2 className="mb-4 font-semibold text-slate-800 dark:text-slate-100">Address</h2>
          <div className="grid gap-4 sm:grid-cols-2">
            <FormField label="Street" error={errors.street?.message} className="sm:col-span-2">
              <Input placeholder="123 Main St" {...register('street')} />
            </FormField>
            <FormField label="City" error={errors.city?.message}>
              <Input placeholder="New York" {...register('city')} />
            </FormField>
            <FormField label="Province / State" error={errors.province?.message}>
              <Input placeholder="NY" {...register('province')} />
            </FormField>
            <FormField label="Country" error={errors.country?.message}>
              <Input placeholder="USA" {...register('country')} />
            </FormField>
          </div>
        </Card>

        {apiError && (
          <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700 dark:bg-red-900/20 dark:border-red-800 dark:text-red-400">
            {apiError}
          </div>
        )}

        <div className="flex justify-end gap-3">
          <Button type="button" variant="secondary" onClick={() => navigate(-1)}>Cancel</Button>
          <Button type="submit" loading={isSubmitting || mutation.isPending}>
            {isEdit ? 'Save Changes' : 'Create Customer'}
          </Button>
        </div>
      </form>
    </div>
  )
}
