import { useParams } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { Heart } from 'lucide-react'
import { api } from '../../lib/api'
import { Button, Card, Badge, PageSpinner, FormField, Textarea } from '../../components/ui'
import type { Product, Review } from '../../types'
import { useAuth } from '../../lib/auth'

function Stars({ rating, onRate }: { rating: number; onRate?: (r: number) => void }) {
  return (
    <div className="flex gap-0.5">
      {[1, 2, 3, 4, 5].map((star) => (
        <button
          key={star}
          type="button"
          onClick={() => onRate?.(star)}
          className={star <= rating ? 'text-amber-400' : 'text-slate-300'}
        >
          ★
        </button>
      ))}
    </div>
  )
}

export default function ProductPage() {
  const { shortId } = useParams<{ shortId: string }>()
  const { user } = useAuth()
  const qc = useQueryClient()

  const [rating, setRating] = useState(5)
  const [comment, setComment] = useState('')

  const { data: product, isLoading } = useQuery<Product>({
    queryKey: ['product-public', shortId],
    queryFn: () => api.get(`/products/short/${shortId}`).then((r) => r.data),
  })

  const { data: reviews = [] } = useQuery<Review[]>({
    queryKey: ['product-reviews', product?.id],
    queryFn: () => api.get(`/products/${product!.id}/reviews`).then((r) => r.data),
    enabled: !!product,
  })

  const toggleFavMutation = useMutation({
    mutationFn: () => api.post(`/me/favorites/${product!.id}/toggle`),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['favorites'] }),
  })

  const reviewMutation = useMutation({
    mutationFn: () => api.post(`/me/reviews/${product!.id}`, { rating, comment }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['product-reviews', product?.id] })
      setComment('')
      setRating(5)
    },
  })

  if (isLoading) return <PageSpinner />
  if (!product) return <p className="text-slate-500">Product not found.</p>

  const mainImage = product.images.find((i) => i.isMain) ?? product.images[0]
  const avgRating = reviews.length ? reviews.reduce((s, r) => s + r.rating, 0) / reviews.length : 0
  const hasReviewed = user && reviews.some((r) => r.appUserId === user.id)

  return (
    <div className="grid grid-cols-1 gap-8 lg:grid-cols-2">
      <div>
        {mainImage ? (
          <img src={`/uploads/products/${mainImage.fileName}`} alt={product.name} className="w-full rounded-xl object-cover" />
        ) : (
          <div className="aspect-square w-full rounded-xl bg-slate-100" />
        )}
        {product.images.length > 1 && (
          <div className="mt-3 grid grid-cols-5 gap-2">
            {product.images.map((img) => (
              <img key={img.id} src={`/uploads/products/${img.fileName}`} alt="" className="aspect-square w-full rounded-lg object-cover cursor-pointer ring-1 ring-slate-200 hover:ring-primary" />
            ))}
          </div>
        )}
      </div>
      <div className="space-y-6">
        <div>
          <h1 className="text-3xl font-bold text-slate-800">{product.name}</h1>
          {product.manufacturerName && <p className="mt-1 text-sm text-slate-400">by {product.manufacturerName}</p>}
          <div className="mt-3 flex items-center gap-3">
            <span className="text-3xl font-bold text-primary">${product.effectivePrice.toFixed(2)}</span>
            {product.activeOffer && <span className="text-lg text-slate-400 line-through">${product.price.toFixed(2)}</span>}
            {product.activeOffer && <Badge label={product.activeOffer.name} color="red" />}
          </div>
          <div className="mt-2 flex items-center gap-2">
            <Stars rating={Math.round(avgRating)} />
            <span className="text-sm text-slate-400">({reviews.length})</span>
          </div>
        </div>
        <p className="text-slate-600">{product.description}</p>
        <div className="flex items-center gap-3">
          <Badge label={product.stockQuantity > 0 ? `${product.stockQuantity} in stock` : 'Out of stock'} color={product.stockQuantity > 0 ? 'green' : 'red'} />
          {product.size && <Badge label={product.size} color="slate" />}
          {product.unitOfMeasureName && <Badge label={product.unitOfMeasureAbbreviation ?? product.unitOfMeasureName} color="slate" />}
        </div>
        {user && (
          <Button variant="secondary" onClick={() => toggleFavMutation.mutate()}>
            <Heart size={16} /> {toggleFavMutation.isPending ? 'Saving…' : 'Save to Favorites'}
          </Button>
        )}

        {/* Reviews */}
        <div className="border-t border-slate-200 pt-6">
          <h2 className="mb-4 text-lg font-semibold text-slate-800">Reviews</h2>
          {reviews.length === 0 && <p className="text-sm text-slate-400">No reviews yet. Be the first!</p>}
          <div className="space-y-4">
            {reviews.map((r) => (
              <Card key={r.id} className="p-4">
                <div className="flex items-center justify-between">
                  <p className="font-medium text-sm text-slate-800">{r.userName}</p>
                  <Stars rating={r.rating} />
                </div>
                {r.comment && <p className="mt-2 text-sm text-slate-600">{r.comment}</p>}
                <p className="mt-1 text-xs text-slate-400">{new Date(r.createdAt).toLocaleDateString()}</p>
              </Card>
            ))}
          </div>
          {user && !hasReviewed && (
            <form
              onSubmit={(e) => { e.preventDefault(); reviewMutation.mutate() }}
              className="mt-6 space-y-3"
            >
              <h3 className="font-medium text-slate-700">Write a Review</h3>
              <Stars rating={rating} onRate={setRating} />
              <FormField label="Comment">
                <Textarea value={comment} onChange={(e) => setComment(e.target.value)} />
              </FormField>
              <Button type="submit" loading={reviewMutation.isPending} size="sm">Submit Review</Button>
            </form>
          )}
        </div>
      </div>
    </div>
  )
}
