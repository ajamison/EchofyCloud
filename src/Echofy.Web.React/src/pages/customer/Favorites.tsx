import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { Heart } from 'lucide-react'
import { api } from '../../lib/api'
import { PageHeader, Card, PageSpinner } from '../../components/ui'
import type { Product } from '../../types'

export default function Favorites() {
  const qc = useQueryClient()
  const { data: favorites = [], isLoading } = useQuery<Product[]>({
    queryKey: ['favorites'],
    queryFn: () => api.get('/me/favorites').then((r) => r.data),
  })

  const toggleMutation = useMutation({
    mutationFn: (productId: number) => api.post(`/me/favorites/${productId}/toggle`),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['favorites'] }),
  })

  if (isLoading) return <PageSpinner />

  return (
    <div>
      <PageHeader title="My Favorites" subtitle={`${favorites.length} saved products`} />
      {favorites.length === 0 ? (
        <p className="text-sm text-slate-400">No favorites yet. Browse products and tap the heart icon!</p>
      ) : (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4">
          {favorites.map((p) => (
            <Card key={p.id} className="relative p-4">
              <button
                onClick={() => toggleMutation.mutate(p.id)}
                className="absolute right-4 top-4 text-red-400 hover:text-red-600"
              >
                <Heart size={18} fill="currentColor" />
              </button>
              <Link to={p.additionalShortIds[0] ? `/p/${p.additionalShortIds[0].code}` : `/products/${p.id}`} className="block">
                <p className="font-medium text-slate-800">{p.name}</p>
                <p className="mt-1 text-sm font-semibold text-primary">${p.effectivePrice.toFixed(2)}</p>
              </Link>
            </Card>
          ))}
        </div>
      )}
    </div>
  )
}
