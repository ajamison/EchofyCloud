import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useNavigate, Link } from 'react-router-dom'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { ArrowLeft } from 'lucide-react'
import { api } from '../../../lib/api'
import { Button, FormField, Input, Select, PageHeader, Card } from '../../../components/ui'
import { LeadStatus } from '../../../types'

const schema = z.object({
  fullName: z.string().min(1, 'Name is required'),
  email: z.string().email('Invalid email'),
  company: z.string().optional(),
  phone: z.string().optional(),
  status: z.coerce.number().default(0),
  estimatedValue: z.coerce.number().min(0),
  assignedTo: z.string().optional(),
})
type FormData = z.infer<typeof schema>

export default function CreateLead() {
  const navigate = useNavigate()
  const qc = useQueryClient()
  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { estimatedValue: 0, status: 0 },
  })

  const mutation = useMutation({
    mutationFn: (data: FormData) => api.post('/leads', data),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ['leads'] }); navigate('/crm/leads') },
  })

  return (
    <div>
      <div className="mb-4">
        <Link to="/crm/leads" className="flex items-center gap-1 text-sm text-slate-500 hover:text-slate-700">
          <ArrowLeft size={14} /> Back to Leads
        </Link>
      </div>
      <PageHeader title="New Lead" />
      <form onSubmit={handleSubmit((d) => mutation.mutate(d))}>
        <Card className="max-w-xl space-y-4">
          <FormField label="Full Name" error={errors.fullName?.message} required><Input {...register('fullName')} /></FormField>
          <FormField label="Email" error={errors.email?.message} required><Input type="email" {...register('email')} /></FormField>
          <FormField label="Company"><Input {...register('company')} /></FormField>
          <FormField label="Phone"><Input {...register('phone')} /></FormField>
          <FormField label="Status">
            <Select {...register('status')}>
              {Object.entries(LeadStatus).filter(([, v]) => typeof v === 'number').map(([k, v]) => (
                <option key={v as number} value={v as number}>{k}</option>
              ))}
            </Select>
          </FormField>
          <FormField label="Estimated Value" error={errors.estimatedValue?.message}>
            <Input type="number" step="0.01" {...register('estimatedValue')} />
          </FormField>
          <FormField label="Assigned To"><Input placeholder="Name or email" {...register('assignedTo')} /></FormField>
          {mutation.isError && <p className="text-sm text-red-500">Failed to create lead.</p>}
          <Button type="submit" loading={isSubmitting}>Create Lead</Button>
        </Card>
      </form>
    </div>
  )
}
