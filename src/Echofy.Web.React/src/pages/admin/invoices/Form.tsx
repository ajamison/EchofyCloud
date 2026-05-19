import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api } from '../../../lib/api'
import { useAuth } from '../../../lib/auth'
import { PageHeader, Card, FormField, Input, Textarea, Button, PageSpinner } from '../../../components/ui'
import type { InvoiceDto, Company } from '../../../types'

const schema = z.object({
  companyId:     z.coerce.number().nullable().optional(),
  customerName:  z.string().min(1, 'Required'),
  customerEmail: z.string().email('Invalid email'),
  customerPhone: z.string().optional(),
  issuedDate:    z.string().min(1, 'Required'),
  dueDate:       z.string().min(1, 'Required'),
  totalAmount:   z.coerce.number().min(0, 'Must be ≥ 0'),
  notes:         z.string().optional(),
})
type FormData = z.infer<typeof schema>

const today = new Date().toISOString().split('T')[0]
const nextMonth = new Date(Date.now() + 30 * 86400000).toISOString().split('T')[0]

export default function InvoiceForm() {
  const { id } = useParams<{ id: string }>()
  const isEdit  = !!id
  const navigate = useNavigate()
  const qc = useQueryClient()
  const { user } = useAuth()
  const isAdmin = user?.role === 'Admin'
  const [apiError, setApiError] = useState<string | null>(null)

  const { data: companies = [] } = useQuery<Company[]>({
    queryKey: ['admin-companies'],
    queryFn: () => api.get('/admin/companies').then(r => r.data),
    enabled: isAdmin,
  })

  const { data: existing, isLoading } = useQuery<InvoiceDto>({
    queryKey: ['admin-invoices', id],
    queryFn: () => api.get(`/admin/invoices/${id}`).then(r => r.data),
    enabled: isEdit,
  })

  const { register, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: {
      issuedDate:  today,
      dueDate:     nextMonth,
      totalAmount: 0,
    },
  })

  useEffect(() => {
    if (existing) {
      reset({
        companyId:     existing.companyId ?? undefined,
        customerName:  existing.customerName,
        customerEmail: existing.customerEmail,
        customerPhone: existing.customerPhone ?? '',
        issuedDate:    existing.issuedDate.split('T')[0],
        dueDate:       existing.dueDate.split('T')[0],
        totalAmount:   existing.total,
        notes:         existing.notes ?? '',
      })
    }
  }, [existing, reset])

  const mutation = useMutation({
    mutationFn: (data: FormData) => isEdit
      ? api.put(`/admin/invoices/${id}`, data)
      : api.post('/admin/invoices', data),
    onSuccess: (res) => {
      setApiError(null)
      qc.invalidateQueries({ queryKey: ['admin-invoices'] })
      const invoiceId = isEdit ? id : res.data?.id
      navigate(`/admin/invoices/${invoiceId}`)
    },
    onError: (err: unknown) => {
      const msg = (err as { response?: { data?: { message?: string }; status?: number } })?.response
      setApiError(msg?.data?.message ?? `Request failed (${msg?.status ?? 'network error'})`)
    },
  })

  if (isEdit && isLoading) return <PageSpinner />

  return (
    <div className="space-y-6">
      <PageHeader
        title={isEdit ? `Edit ${existing?.invoiceNumber ?? '…'}` : 'New Invoice'}
        subtitle={isEdit ? 'Update invoice details.' : 'Fill in customer and invoice details.'}
      />

      <form onSubmit={handleSubmit(d => mutation.mutate(d))} className="space-y-6">
        {isAdmin && (
          <Card>
            <h2 className="mb-4 font-semibold text-slate-800 dark:text-slate-100">Company Assignment</h2>
            <FormField label="Assign to Company">
              <select
                {...register('companyId')}
                className="w-full rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm text-slate-800 focus:outline-none focus:ring-2 focus:ring-primary dark:border-slate-600 dark:bg-slate-700 dark:text-slate-200"
              >
                <option value="">— No company —</option>
                {companies.filter(c => c.isActive).map(c => (
                  <option key={c.id} value={c.id}>{c.name}</option>
                ))}
              </select>
            </FormField>
          </Card>
        )}

        <Card>
          <h2 className="mb-4 font-semibold text-slate-800 dark:text-slate-100">Customer Details</h2>
          <div className="grid gap-4 sm:grid-cols-2">
            <FormField label="Customer Name" error={errors.customerName?.message} required>
              <Input placeholder="Jane Smith" {...register('customerName')} />
            </FormField>
            <FormField label="Email" error={errors.customerEmail?.message} required>
              <Input type="email" placeholder="jane@example.com" {...register('customerEmail')} />
            </FormField>
            <FormField label="Phone" error={errors.customerPhone?.message}>
              <Input placeholder="+1 555 000 0000" {...register('customerPhone')} />
            </FormField>
          </div>
        </Card>

        <Card>
          <h2 className="mb-4 font-semibold text-slate-800 dark:text-slate-100">Invoice Details</h2>
          <div className="grid gap-4 sm:grid-cols-3">
            <FormField label="Issue Date" error={errors.issuedDate?.message} required>
              <Input type="date" {...register('issuedDate')} />
            </FormField>
            <FormField label="Due Date" error={errors.dueDate?.message} required>
              <Input type="date" {...register('dueDate')} />
            </FormField>
            <FormField label="Total Amount ($)" error={errors.totalAmount?.message} required>
              <Input type="number" step="0.01" min="0" placeholder="0.00" {...register('totalAmount')} />
            </FormField>
          </div>
          <div className="mt-4">
            <FormField label="Notes" error={errors.notes?.message}>
              <Textarea placeholder="Payment terms, bank details, thank you note…" rows={3} {...register('notes')} />
            </FormField>
          </div>
        </Card>

        {apiError && (
          <div className="rounded-lg bg-red-50 border border-red-200 px-4 py-3 text-sm text-red-700 dark:bg-red-900/20 dark:border-red-800 dark:text-red-400">
            {apiError}
          </div>
        )}

        <div className="flex justify-end gap-3">
          <Button type="button" variant="secondary" onClick={() => navigate(-1)}>Cancel</Button>
          <Button type="submit" loading={isSubmitting || mutation.isPending}>
            {isEdit ? 'Save Changes' : 'Create Invoice'}
          </Button>
        </div>
      </form>
    </div>
  )
}
