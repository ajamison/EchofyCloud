import { useRef, useState } from 'react'
import { useParams, Link } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { ArrowLeft, Edit, Upload, Trash2, Star } from 'lucide-react'
import { api } from '../../../lib/api'
import { Button, Badge, Card, PageSpinner, PageHeader } from '../../../components/ui'
import type { ManufacturerProduct } from '../../../types'
import { useAuth } from '../../../lib/auth'

export default function ManufacturerProductDetails() {
  const { id } = useParams<{ id: string }>()
  const { user } = useAuth()
  const canEdit = user?.role === 'Admin' || user?.role === 'Manager'
  const qc = useQueryClient()
  const fileInputRef = useRef<HTMLInputElement>(null)
  const [uploading, setUploading] = useState(false)

  const { data: item, isLoading } = useQuery<ManufacturerProduct>({
    queryKey: ['manufacturer-product', id],
    queryFn: () => api.get(`/manufacturer-products/${id}`).then((r) => r.data),
  })

  const deleteImageMutation = useMutation({
    mutationFn: (imageId: number) => api.delete(`/manufacturer-products/${id}/images/${imageId}`),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['manufacturer-product', id] }),
  })

  const setMainMutation = useMutation({
    mutationFn: (imageId: number) => api.put(`/manufacturer-products/${id}/images/${imageId}/main`, {}),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['manufacturer-product', id] }),
  })

  async function handleFilesSelected(e: React.ChangeEvent<HTMLInputElement>) {
    const files = Array.from(e.target.files ?? [])
    if (!files.length) return
    setUploading(true)
    for (const file of files) {
      const form = new FormData()
      form.append('file', file)
      await api.post(`/manufacturer-products/${id}/images`, form, {
        headers: { 'Content-Type': 'multipart/form-data' },
      })
    }
    await qc.invalidateQueries({ queryKey: ['manufacturer-product', id] })
    setUploading(false)
    if (fileInputRef.current) fileInputRef.current.value = ''
  }

  if (isLoading) return <PageSpinner />
  if (!item) return <p className="text-slate-500">Not found.</p>

  return (
    <div>
      <div className="mb-4">
        <Link to="/manufacturer-products" className="flex items-center gap-1 text-sm text-slate-500 hover:text-slate-700">
          <ArrowLeft size={14} /> Back to Manufacturer Products
        </Link>
      </div>
      <PageHeader
        title={item.name || item.manufacturerName}
        subtitle={`${item.manufacturerName}${item.manufacturerPartNumber ? ` — ${item.manufacturerPartNumber}` : ''}`}
        actions={
          canEdit && (
            <Link to={`/manufacturer-products/${id}/edit`}>
              <Button><Edit size={14} /> Edit</Button>
            </Link>
          )
        }
      />
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
        <div className="lg:col-span-2">
          <Card>
            <div className="mb-4 flex items-center justify-between">
              <h3 className="font-semibold text-slate-700">Images ({item.images?.length ?? 0})</h3>
              {canEdit && (
                <>
                  <Button
                    variant="secondary"
                    size="sm"
                    loading={uploading}
                    onClick={() => fileInputRef.current?.click()}
                  >
                    <Upload size={14} /> Upload Images
                  </Button>
                  <input
                    ref={fileInputRef}
                    type="file"
                    accept="image/*"
                    multiple
                    className="hidden"
                    onChange={handleFilesSelected}
                  />
                </>
              )}
            </div>
            {!item.images?.length ? (
              <p className="py-8 text-center text-sm text-slate-400">No images uploaded yet.</p>
            ) : (
              <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 md:grid-cols-4">
                {item.images.map((img) => (
                  <div key={img.id} className="group relative">
                    <img
                      src={img.url}
                      alt={img.altText ?? ''}
                      className="aspect-square w-full rounded-lg object-cover"
                    />
                    {img.isMain && (
                      <span className="absolute left-1.5 top-1.5 rounded-full bg-primary px-2 py-0.5 text-xs font-medium text-white">
                        Main
                      </span>
                    )}
                    {canEdit && (
                      <div className="absolute inset-0 flex items-center justify-center gap-1.5 rounded-lg bg-black/40 opacity-0 transition-opacity group-hover:opacity-100">
                        {!img.isMain && (
                          <button
                            title="Set as main"
                            onClick={() => setMainMutation.mutate(img.id)}
                            className="rounded-full bg-white p-1.5 hover:bg-yellow-50"
                          >
                            <Star size={14} className="text-yellow-500" />
                          </button>
                        )}
                        <button
                          title="Delete"
                          onClick={() => {
                            if (confirm('Delete this image?')) deleteImageMutation.mutate(img.id)
                          }}
                          className="rounded-full bg-white p-1.5 hover:bg-red-50"
                        >
                          <Trash2 size={14} className="text-red-500" />
                        </button>
                      </div>
                    )}
                  </div>
                ))}
              </div>
            )}
          </Card>
        </div>
        <div>
          <Card>
            <h3 className="mb-3 font-semibold text-slate-700">Info</h3>
            <dl className="space-y-3 text-sm">
              <div>
                <dt className="text-slate-400">Name</dt>
                <dd className="font-medium">{item.name || <span className="text-slate-400 italic">Not set</span>}</dd>
              </div>
              <div>
                <dt className="text-slate-400">Manufacturer</dt>
                <dd className="font-medium">{item.manufacturerName}</dd>
              </div>
              <div>
                <dt className="text-slate-400">Part Number</dt>
                <dd className="font-medium">{item.manufacturerPartNumber ?? '—'}</dd>
              </div>
              <div>
                <dt className="text-slate-400">SKU</dt>
                <dd className="font-medium">{item.sku ?? '—'}</dd>
              </div>
              <div>
                <dt className="text-slate-400">Size</dt>
                <dd className="font-medium">{item.size ?? '—'}</dd>
              </div>
              <div>
                <dt className="text-slate-400">MSRP</dt>
                <dd className="font-medium">{item.msrp != null ? `$${item.msrp.toFixed(2)}` : '—'}</dd>
              </div>
              <div>
                <dt className="text-slate-400">Unit of Measure</dt>
                <dd className="font-medium">
                  {item.unitOfMeasureName
                    ? `${item.unitOfMeasureName} (${item.unitOfMeasureAbbreviation})`
                    : '—'}
                </dd>
              </div>
              <div>
                <dt className="text-slate-400">Products linked</dt>
                <dd className="font-medium">{item.productCount}</dd>
              </div>
              <div>
                <dt className="text-slate-400">Status</dt>
                <dd><Badge label={item.isActive ? 'Active' : 'Inactive'} color={item.isActive ? 'green' : 'slate'} /></dd>
              </div>
              <div>
                <dt className="text-slate-400">Created</dt>
                <dd className="font-medium">{new Date(item.createdAt).toLocaleDateString()}</dd>
              </div>
            </dl>
            {item.description && (
              <div className="mt-4 border-t border-slate-100 pt-4">
                <dt className="text-xs font-medium uppercase tracking-wider text-slate-400">Description</dt>
                <dd className="mt-1 text-sm text-slate-600">{item.description}</dd>
              </div>
            )}
          </Card>
        </div>
      </div>
    </div>
  )
}
