import { useRef, useState } from 'react'
import { useParams, Link } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { ArrowLeft, Edit, Upload, Trash2, Star } from 'lucide-react'
import { api } from '../../../lib/api'
import { Button, Badge, Card, PageSpinner, PageHeader } from '../../../components/ui'
import type { Product } from '../../../types'
import { useAuth } from '../../../lib/auth'

export default function ProductDetails() {
  const { id } = useParams<{ id: string }>()
  const { user } = useAuth()
  const canEdit = user?.role === 'Admin' || user?.role === 'Manager'
  const qc = useQueryClient()
  const fileInputRef = useRef<HTMLInputElement>(null)
  const [uploading, setUploading] = useState(false)

  const { data: product, isLoading } = useQuery<Product>({
    queryKey: ['product', id],
    queryFn: () => api.get(`/products/${id}`).then((r) => r.data),
  })

  const deleteImageMutation = useMutation({
    mutationFn: (imageId: number) => api.delete(`/products/${id}/images/${imageId}`),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['product', id] }),
  })

  const setMainMutation = useMutation({
    mutationFn: (imageId: number) => api.put(`/products/${id}/images/${imageId}/main`, {}),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['product', id] }),
  })

  async function handleFilesSelected(e: React.ChangeEvent<HTMLInputElement>) {
    const files = Array.from(e.target.files ?? [])
    if (!files.length) return
    setUploading(true)
    for (const file of files) {
      const form = new FormData()
      form.append('file', file)
      await api.post(`/products/${id}/images`, form, {
        headers: { 'Content-Type': 'multipart/form-data' },
      })
    }
    await qc.invalidateQueries({ queryKey: ['product', id] })
    setUploading(false)
    if (fileInputRef.current) fileInputRef.current.value = ''
  }

  if (isLoading) return <PageSpinner />
  if (!product) return <p className="text-slate-500">Product not found.</p>

  return (
    <div>
      <div className="mb-4">
        <Link to="/products" className="flex items-center gap-1 text-sm text-slate-500 hover:text-slate-700">
          <ArrowLeft size={14} /> Back to Products
        </Link>
      </div>
      <PageHeader
        title={product.name}
        subtitle={product.additionalShortIds.length > 0 ? `${product.additionalShortIds.length} QR code${product.additionalShortIds.length > 1 ? 's' : ''}` : undefined}
        actions={canEdit && <Link to={`/products/${id}/edit`}><Button><Edit size={14} /> Edit</Button></Link>}
      />
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
        <div className="lg:col-span-2 space-y-6">
          <Card>
            <h3 className="mb-3 font-semibold text-slate-700">Details</h3>
            <p className="text-sm text-slate-600">{product.description}</p>
            <dl className="mt-4 grid grid-cols-2 gap-3 text-sm">
              <div><dt className="text-slate-400">SKU</dt><dd className="font-medium">{product.sku ?? '—'}</dd></div>
              <div><dt className="text-slate-400">UPC</dt><dd className="font-medium">{product.manufacturerUpc ?? '—'}</dd></div>
              <div><dt className="text-slate-400">Size</dt><dd className="font-medium">{product.size ?? '—'}</dd></div>
              <div><dt className="text-slate-400">Unit</dt><dd className="font-medium">{product.unitOfMeasureName ?? '—'}</dd></div>
              <div><dt className="text-slate-400">Manufacturer</dt><dd className="font-medium">{product.manufacturerName ?? '—'}</dd></div>
              <div><dt className="text-slate-400">Categories</dt><dd className="font-medium">{product.categoryNames.join(', ') || '—'}</dd></div>
            </dl>
          </Card>

          <Card>
            <div className="mb-4 flex items-center justify-between">
              <h3 className="font-semibold text-slate-700">Images ({product.images.length})</h3>
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
            {product.images.length === 0 ? (
              <p className="py-8 text-center text-sm text-slate-400">No images uploaded yet.</p>
            ) : (
              <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 md:grid-cols-4">
                {product.images.map((img) => (
                  <div key={img.id} className="group relative">
                    <img
                      src={`/uploads/products/${img.fileName}`}
                      alt={img.altText ?? product.name}
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

          {product.discountOffers.length > 0 && (
            <Card>
              <h3 className="mb-3 font-semibold text-slate-700">Discount Offers</h3>
              <div className="space-y-2">
                {product.discountOffers.map((offer) => (
                  <div key={offer.id} className="flex items-center justify-between rounded-lg border border-slate-100 px-4 py-2.5 text-sm">
                    <span className="font-medium">{offer.name}</span>
                    <div className="flex items-center gap-2">
                      <span className="text-slate-600">{offer.discountType === 0 ? `${offer.discountValue}%` : `$${offer.discountValue}`} off</span>
                      <Badge label={offer.isCurrentlyRunning ? 'Active' : 'Inactive'} color={offer.isCurrentlyRunning ? 'green' : 'slate'} />
                    </div>
                  </div>
                ))}
              </div>
            </Card>
          )}
        </div>
        <div className="space-y-6">
          <Card>
            <h3 className="mb-3 font-semibold text-slate-700">Pricing</h3>
            <div className="text-3xl font-bold text-slate-800">${product.effectivePrice.toFixed(2)}</div>
            {product.activeOffer && (
              <div className="mt-1 text-sm text-slate-400 line-through">${product.price.toFixed(2)}</div>
            )}
            <div className="mt-4 space-y-2 text-sm">
              <div className="flex justify-between"><span className="text-slate-400">Stock</span><span className="font-medium">{product.stockQuantity}</span></div>
              <div className="flex justify-between"><span className="text-slate-400">Status</span><Badge label={product.isActive ? 'Active' : 'Inactive'} color={product.isActive ? 'green' : 'slate'} /></div>
            </div>
          </Card>
          <Card>
            <h3 className="mb-3 font-semibold text-slate-700">Price History</h3>
            {product.priceHistory.length === 0 ? (
              <p className="text-sm text-slate-400">No history.</p>
            ) : (
              <div className="space-y-2 text-sm">
                {product.priceHistory.slice(0, 5).map((h) => (
                  <div key={h.id} className="flex justify-between">
                    <span className="text-slate-500">{new Date(h.effectiveFrom).toLocaleDateString()}</span>
                    <span className="font-medium">${h.price.toFixed(2)}</span>
                  </div>
                ))}
              </div>
            )}
          </Card>
        </div>
      </div>
    </div>
  )
}
