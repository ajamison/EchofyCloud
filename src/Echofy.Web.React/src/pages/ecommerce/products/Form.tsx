import { useEffect } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useNavigate, useParams, Link } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { ArrowLeft, Link2 } from 'lucide-react'
import { api } from '../../../lib/api'
import { useAuth } from '../../../lib/auth'
import { Button, FormField, Input, Textarea, Select, PageHeader, Card, PageSpinner } from '../../../components/ui'
import type { Product, Category, Manufacturer, UnitOfMeasure, ManufacturerProduct, Company } from '../../../types'

const schema = z.object({
  name: z.string().min(1, 'Name is required'),
  description: z.string().min(1, 'Description is required'),
  price: z.coerce.number().min(0, 'Price must be ≥ 0'),
  stockQuantity: z.coerce.number().int().min(0),
  sku: z.string().optional(),
  manufacturerUpc: z.string().optional(),
  size: z.string().optional(),
  companyId: z.coerce.number().optional().nullable(),
  manufacturerId: z.coerce.number().optional().nullable(),
  manufacturerProductId: z.coerce.number().optional().nullable(),
  unitOfMeasureId: z.coerce.number().optional().nullable(),
  isActive: z.boolean().default(true),
})
type FormData = z.infer<typeof schema>

export default function ProductForm() {
  const { id } = useParams<{ id?: string }>()
  const isEdit = !!id
  const navigate = useNavigate()
  const qc = useQueryClient()
  const { user } = useAuth()
  const isAdmin = user?.role === 'Admin'

  const { data: product, isLoading: loadingProduct } = useQuery<Product>({
    queryKey: ['product', id],
    queryFn: () => api.get(`/products/${id}`).then((r) => r.data),
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
  const { data: companies = [] } = useQuery<Company[]>({
    queryKey: ['admin-companies'],
    queryFn: () => api.get('/admin/companies').then((r) => r.data),
    enabled: isAdmin,
  })

  const { register, handleSubmit, reset, watch, setValue, formState: { errors, isSubmitting } } = useForm<FormData>({
    resolver: zodResolver(schema),
  })

  const selectedManufacturerId = watch('manufacturerId')
  const selectedMfgProductId = watch('manufacturerProductId')

  // Fetch manufacturer products filtered by selected manufacturer
  const { data: mfgProducts = [] } = useQuery<ManufacturerProduct[]>({
    queryKey: ['mfg-products', selectedManufacturerId],
    queryFn: () =>
      api.get('/manufacturer-products', {
        params: selectedManufacturerId ? { manufacturerId: selectedManufacturerId } : {},
      }).then((r) => r.data),
    enabled: !!selectedManufacturerId,
  })

  const selectedMfgProduct = mfgProducts.find((mp) => mp.id === Number(selectedMfgProductId))

  useEffect(() => {
    if (product) reset({
      name: product.name, description: product.description, price: product.price,
      stockQuantity: product.stockQuantity, isActive: product.isActive,
      sku: product.sku ?? undefined, manufacturerUpc: product.manufacturerUpc ?? undefined,
      size: product.size ?? undefined,
      companyId: product.companyId ?? undefined,
      manufacturerId: product.manufacturerId ?? undefined,
      manufacturerProductId: product.manufacturerProductId ?? undefined,
      unitOfMeasureId: product.unitOfMeasureId ?? undefined,
    })
  }, [product, reset])

  // Auto-fill fields when a manufacturer product is selected
  const handleMfgProductChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const mpId = e.target.value ? Number(e.target.value) : null
    setValue('manufacturerProductId', mpId)

    if (!mpId) return
    const mp = mfgProducts.find((x) => x.id === mpId)
    if (!mp) return

    if (mp.name) setValue('name', mp.name)
    if (mp.description) setValue('description', mp.description)
    if (mp.size) setValue('size', mp.size)
    if (mp.sku) setValue('sku', mp.sku)
    if (mp.msrp != null) setValue('price', mp.msrp)
    if (mp.unitOfMeasureId) setValue('unitOfMeasureId', mp.unitOfMeasureId)
  }

  const mutation = useMutation({
    mutationFn: (data: FormData) =>
      isEdit ? api.put(`/products/${id}`, data) : api.post('/products', data),
    onSuccess: (res) => {
      qc.invalidateQueries({ queryKey: ['products'] })
      if (isEdit) qc.invalidateQueries({ queryKey: ['product', id] })
      navigate(`/products/${isEdit ? id : res.data.id}`)
    },
  })

  if (isEdit && !product) return <PageSpinner />

  return (
    <div>
      <div className="mb-4">
        <Link to="/products" className="flex items-center gap-1 text-sm text-slate-500 hover:text-slate-700">
          <ArrowLeft size={14} /> Back to Products
        </Link>
      </div>
      <PageHeader title={isEdit ? 'Edit Product' : 'New Product'} />
      <form onSubmit={handleSubmit((data) => mutation.mutate(data))}>
        <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
          <div className="lg:col-span-2 space-y-6">

            {/* Company assignment — Admin only */}
            {isAdmin && (
              <Card>
                <h3 className="mb-4 font-semibold text-slate-700">Company Assignment</h3>
                <FormField label="Assign to Company">
                  <Select {...register('companyId')}>
                    <option value="">— No company —</option>
                    {companies.filter(c => c.isActive).map(c => (
                      <option key={c.id} value={c.id}>{c.name}</option>
                    ))}
                  </Select>
                </FormField>
              </Card>
            )}

            {/* Manufacturer Product linkage */}
            <Card>
              <h3 className="mb-1 font-semibold text-slate-700 flex items-center gap-2">
                <Link2 size={15} className="text-blue-500" /> Link Manufacturer Product
              </h3>
              <p className="mb-4 text-xs text-slate-400">
                Select a manufacturer product to auto-fill description, size, SKU, and price from the catalogue.
              </p>
              <div className="grid grid-cols-2 gap-4">
                <FormField label="Manufacturer">
                  <Select {...register('manufacturerId')} onChange={(e) => {
                    setValue('manufacturerId', e.target.value ? Number(e.target.value) : null)
                    setValue('manufacturerProductId', null)
                  }}>
                    <option value="">— None —</option>
                    {manufacturers.map((m) => <option key={m.id} value={m.id}>{m.name}</option>)}
                  </Select>
                </FormField>
                <FormField label="Manufacturer Product">
                  <Select
                    value={selectedMfgProductId ?? ''}
                    onChange={handleMfgProductChange}
                    disabled={!selectedManufacturerId}
                  >
                    <option value="">— None —</option>
                    {mfgProducts.map((mp) => (
                      <option key={mp.id} value={mp.id}>
                        {mp.name}{mp.manufacturerPartNumber ? ` — ${mp.manufacturerPartNumber}` : ''}
                      </option>
                    ))}
                  </Select>
                </FormField>
              </div>
              {selectedMfgProduct && (
                <div className="mt-3 rounded-md border border-blue-100 bg-blue-50 p-3 text-sm">
                  <div className="font-medium text-blue-700">{selectedMfgProduct.name}</div>
                  <div className="text-blue-600 text-xs">{selectedMfgProduct.manufacturerName}</div>
                  {selectedMfgProduct.manufacturerPartNumber && (
                    <div className="text-blue-600">Part #: {selectedMfgProduct.manufacturerPartNumber}</div>
                  )}
                  {selectedMfgProduct.description && (
                    <div className="mt-1 text-slate-600">{selectedMfgProduct.description}</div>
                  )}
                  <div className="mt-1 flex flex-wrap gap-3 text-xs text-slate-500">
                    {selectedMfgProduct.sku && <span>SKU: {selectedMfgProduct.sku}</span>}
                    {selectedMfgProduct.size && <span>Size: {selectedMfgProduct.size}</span>}
                    {selectedMfgProduct.msrp != null && <span>MSRP: ${selectedMfgProduct.msrp.toFixed(2)}</span>}
                    {selectedMfgProduct.unitOfMeasureName && <span>UOM: {selectedMfgProduct.unitOfMeasureName}</span>}
                  </div>
                </div>
              )}
            </Card>

            <Card>
              <h3 className="mb-4 font-semibold text-slate-700">Basic Information</h3>
              <div className="space-y-4">
                <FormField label="Name" error={errors.name?.message} required>
                  <Input {...register('name')} />
                </FormField>
                <FormField label="Description" error={errors.description?.message} required>
                  <Textarea rows={5} {...register('description')} />
                </FormField>
              </div>
            </Card>
            <Card>
              <h3 className="mb-4 font-semibold text-slate-700">Identifiers</h3>
              <div className="grid grid-cols-2 gap-4">
                <FormField label="SKU" error={errors.sku?.message}><Input {...register('sku')} /></FormField>
                <FormField label="Manufacturer UPC" error={errors.manufacturerUpc?.message}><Input {...register('manufacturerUpc')} /></FormField>
                <FormField label="Size" error={errors.size?.message}><Input {...register('size')} /></FormField>
              </div>
            </Card>
          </div>
          <div className="space-y-6">
            <Card>
              <h3 className="mb-4 font-semibold text-slate-700">Pricing & Stock</h3>
              <div className="space-y-4">
                <FormField label="Price" error={errors.price?.message} required><Input type="number" step="0.01" {...register('price')} /></FormField>
                <FormField label="Stock Quantity" error={errors.stockQuantity?.message} required><Input type="number" {...register('stockQuantity')} /></FormField>
              </div>
            </Card>
            <Card>
              <h3 className="mb-4 font-semibold text-slate-700">Classification</h3>
              <div className="space-y-4">
                <FormField label="Unit of Measure">
                  <Select {...register('unitOfMeasureId')}>
                    <option value="">— None —</option>
                    {units.map((u) => <option key={u.id} value={u.id}>{u.name}</option>)}
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
            <Button type="submit" loading={isSubmitting} className="w-full">
              {isEdit ? 'Save Changes' : 'Create Product'}
            </Button>
          </div>
        </div>
      </form>
    </div>
  )
}
