import { useEffect } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useNavigate, useParams, Link } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { ArrowLeft } from 'lucide-react'
import { api } from '../../../lib/api'
import { Button, FormField, Input, Textarea, Select, PageHeader, Card, PageSpinner } from '../../../components/ui'
import type { ManufacturerProduct, Manufacturer, UnitOfMeasure } from '../../../types'

const schema = z.object({
  manufacturerId: z.coerce.number().min(1, 'Manufacturer is required'),
  name: z.string().min(1, 'Name is required'),
  manufacturerPartNumber: z.string().optional(),
  sku: z.string().optional(),
  description: z.string().optional(),
  size: z.string().optional(),
  msrp: z.coerce.number().min(0).optional().nullable(),
  unitOfMeasureId: z.coerce.number().optional().nullable(),
  isActive: z.boolean().default(true),
})
type FormData = z.infer<typeof schema>

export default function ManufacturerProductForm() {
  const { id } = useParams<{ id?: string }>()
  const isEdit = !!id
  const navigate = useNavigate()
  const qc = useQueryClient()

  const { data: item, isLoading: loadingItem } = useQuery<ManufacturerProduct>({
    queryKey: ['manufacturer-product', id],
    queryFn: () => api.get(`/manufacturer-products/${id}`).then((r) => r.data),
    enabled: isEdit,
  })

  const { data: manufacturers = [] } = useQuery<Manufacturer[]>({
    queryKey: ['admin-manufacturers'],
    queryFn: () => api.get('/admin/manufacturers').then((r) => r.data),
  })

  const { data: units = [] } = useQuery<UnitOfMeasure[]>({
    queryKey: ['admin-units'],
    queryFn: () => api.get('/admin/units').then((r) => r.data),
  })

  const { register, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm<FormData>({
    resolver: zodResolver(schema),
  })

  useEffect(() => {
    if (item) reset({
      manufacturerId: item.manufacturerId,
      name: item.name,
      manufacturerPartNumber: item.manufacturerPartNumber ?? undefined,
      sku: item.sku ?? undefined,
      description: item.description ?? undefined,
      size: item.size ?? undefined,
      msrp: item.msrp ?? undefined,
      unitOfMeasureId: item.unitOfMeasureId ?? undefined,
      isActive: item.isActive,
    })
  }, [item, reset])

  const mutation = useMutation({
    mutationFn: (data: FormData) =>
      isEdit ? api.put(`/manufacturer-products/${id}`, data) : api.post('/manufacturer-products', data),
    onSuccess: (res) => {
      qc.invalidateQueries({ queryKey: ['manufacturer-products'] })
      if (isEdit) qc.invalidateQueries({ queryKey: ['manufacturer-product', id] })
      const newId = isEdit ? id : res.data.id
      navigate(`/manufacturer-products/${newId}`)
    },
  })

  if (isEdit && loadingItem) return <PageSpinner />

  return (
    <div>
      <div className="mb-4">
        <Link to="/manufacturer-products" className="flex items-center gap-1 text-sm text-slate-500 hover:text-slate-700">
          <ArrowLeft size={14} /> Back to Manufacturer Products
        </Link>
      </div>
      <PageHeader title={isEdit ? 'Edit Manufacturer Product' : 'New Manufacturer Product'} />
      <form onSubmit={handleSubmit((data) => mutation.mutate(data))}>
        <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
          <div className="lg:col-span-2 space-y-6">
            <Card>
              <h3 className="mb-4 font-semibold text-slate-700">Basic Information</h3>
              <div className="space-y-4">
                <FormField label="Manufacturer" error={errors.manufacturerId?.message} required>
                  <Select {...register('manufacturerId')}>
                    <option value="">— Select manufacturer —</option>
                    {manufacturers.map((m) => (
                      <option key={m.id} value={m.id}>{m.name}</option>
                    ))}
                  </Select>
                </FormField>
                <FormField label="Name" error={errors.name?.message} required>
                  <Input {...register('name')} />
                </FormField>
                <FormField label="Description" error={errors.description?.message}>
                  <Textarea rows={4} {...register('description')} />
                </FormField>
              </div>
            </Card>
            <Card>
              <h3 className="mb-4 font-semibold text-slate-700">Identifiers</h3>
              <div className="grid grid-cols-2 gap-4">
                <FormField label="Part Number" error={errors.manufacturerPartNumber?.message}>
                  <Input {...register('manufacturerPartNumber')} />
                </FormField>
                <FormField label="SKU" error={errors.sku?.message}>
                  <Input {...register('sku')} />
                </FormField>
                <FormField label="Size" error={errors.size?.message}>
                  <Input {...register('size')} />
                </FormField>
              </div>
            </Card>
          </div>
          <div className="space-y-6">
            <Card>
              <h3 className="mb-4 font-semibold text-slate-700">Pricing & Units</h3>
              <div className="space-y-4">
                <FormField label="MSRP" error={errors.msrp?.message}>
                  <Input type="number" step="0.01" {...register('msrp')} />
                </FormField>
                <FormField label="Unit of Measure">
                  <Select {...register('unitOfMeasureId')}>
                    <option value="">— None —</option>
                    {units.map((u) => (
                      <option key={u.id} value={u.id}>{u.name} ({u.abbreviation})</option>
                    ))}
                  </Select>
                </FormField>
                <FormField label="Status">
                  <label className="flex items-center gap-2 text-sm">
                    <input type="checkbox" {...register('isActive')} className="rounded" />
                    Active
                  </label>
                </FormField>
              </div>
            </Card>
            {mutation.isError && (
              <p className="text-sm text-red-500">Failed to save. Please try again.</p>
            )}
            <Button type="submit" loading={mutation.isPending} className="w-full">
              {isEdit ? 'Save Changes' : 'Create'}
            </Button>
          </div>
        </div>
      </form>
    </div>
  )
}
