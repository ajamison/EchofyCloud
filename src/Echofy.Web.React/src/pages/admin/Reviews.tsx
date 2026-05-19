import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { Trash2 } from 'lucide-react'
import { api } from '../../lib/api'
import { useAuth } from '../../lib/auth'
import { Button, PageHeader, Table, Th, Td, PageSpinner } from '../../components/ui'
import type { Review } from '../../types'

function Stars({ rating }: { rating: number }) {
  return <span className="text-amber-400">{'★'.repeat(rating)}{'☆'.repeat(5 - rating)}</span>
}

export default function AdminReviews() {
  const { canWrite } = useAuth()
  const qc = useQueryClient()
  const { data: reviews = [], isLoading } = useQuery<Review[]>({
    queryKey: ['admin-reviews'],
    queryFn: () => api.get('/admin/reviews').then((r) => r.data),
  })
  const deleteMutation = useMutation({
    mutationFn: (id: number) => api.delete(`/admin/reviews/${id}`),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['admin-reviews'] }),
  })

  if (isLoading) return <PageSpinner />

  return (
    <div>
      <PageHeader title="Reviews" subtitle={`${reviews.length} reviews`} />
      <Table>
        <thead><tr><Th>Product</Th><Th>User</Th><Th>Rating</Th><Th>Comment</Th><Th>Date</Th><Th className="text-right">Actions</Th></tr></thead>
        <tbody>
          {reviews.map((r) => (
            <tr key={r.id} className="hover:bg-slate-50">
              <Td className="font-medium">{r.productName}</Td>
              <Td className="text-slate-500">{r.userName}</Td>
              <Td><Stars rating={r.rating} /></Td>
              <Td className="max-w-xs truncate text-slate-600">{r.comment ?? '—'}</Td>
              <Td className="text-slate-500">{new Date(r.createdAt).toLocaleDateString()}</Td>
              <Td className="text-right">
                {canWrite() && (
                  <Button variant="ghost" size="sm" onClick={() => deleteMutation.mutate(r.id)}>
                    <Trash2 size={14} className="text-red-500" />
                  </Button>
                )}
              </Td>
            </tr>
          ))}
        </tbody>
      </Table>
    </div>
  )
}
