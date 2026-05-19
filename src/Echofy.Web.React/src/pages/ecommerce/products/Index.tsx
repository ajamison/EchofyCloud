import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { Plus, Search, Edit, Eye } from 'lucide-react'
import { api } from '../../../lib/api'
import { Button, PageHeader, Table, Th, Td, Badge, PageSpinner, Input } from '../../../components/ui'
import type { Product } from '../../../types'
import { useAuth } from '../../../lib/auth'

export default function Products() {
  const { user, hasAdminAccess, canWrite } = useAuth()
  const [search, setSearch] = useState('')
  // Super roles see all tenants, so show the Client column
  const showClientColumn = hasAdminAccess()
  const canEdit = (user?.role === 'Admin' || user?.role === 'Manager' || user?.role === 'SuperAdmin') && canWrite()

  const { data: products = [], isLoading } = useQuery<Product[]>({
    queryKey: ['products', search],
    queryFn: () => api.get('/products', { params: { search: search || undefined } }).then((r) => r.data),
  })

  if (isLoading) return <PageSpinner />

  return (
    <div>
      <PageHeader
        title="Products"
        subtitle={`${products.length} products`}
        actions={
          canEdit && (
            <Link to="/products/new">
              <Button><Plus size={15} /> Add Product</Button>
            </Link>
          )
        }
      />
      <div className="mb-4 flex items-center gap-3">
        <div className="relative flex-1 max-w-xs">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" size={14} />
          <Input className="pl-8" placeholder="Search products…" value={search} onChange={(e) => setSearch(e.target.value)} />
        </div>
      </div>
      <Table>
        <thead>
          <tr>
            <Th>Product</Th>
            {showClientColumn && <Th>Client</Th>}
            <Th>SKU</Th>
            <Th>Price</Th>
            <Th>Stock</Th>
            <Th>Status</Th>
            <Th className="text-right">Actions</Th>
          </tr>
        </thead>
        <tbody>
          {products.map((p) => (
            <tr key={p.id} className="hover:bg-slate-50">
              <Td>
                <div className="flex items-center gap-3">
                  {p.imageUrl && (
                    <img src={`/uploads/products/${p.imageUrl}`} alt={p.name} className="h-8 w-8 rounded object-cover" />
                  )}
                  <span className="font-medium text-slate-800">{p.name}</span>
                </div>
              </Td>
              {showClientColumn && (
                <Td>
                  {p.clientName
                    ? <Badge label={p.clientName} color="blue" />
                    : <span className="text-xs text-slate-400">—</span>}
                </Td>
              )}
              <Td className="text-slate-500">{p.sku ?? '—'}</Td>
              <Td>
                {p.activeOffer ? (
                  <div>
                    <span className="font-medium text-slate-800">${p.effectivePrice.toFixed(2)}</span>
                    <span className="ml-1 text-xs text-slate-400 line-through">${p.price.toFixed(2)}</span>
                  </div>
                ) : (
                  <span>${p.price.toFixed(2)}</span>
                )}
              </Td>
              <Td>
                {p.stockQuantity === 0 ? (
                  <Badge label="Out of stock" color="red" />
                ) : p.stockQuantity < 5 ? (
                  <Badge label={`${p.stockQuantity} left`} color="yellow" />
                ) : (
                  <span>{p.stockQuantity}</span>
                )}
              </Td>
              <Td>
                <Badge label={p.isActive ? 'Active' : 'Inactive'} color={p.isActive ? 'green' : 'slate'} />
              </Td>
              <Td className="text-right">
                <div className="flex justify-end gap-1">
                  <Link to={`/products/${p.id}`}>
                    <Button variant="ghost" size="sm"><Eye size={14} /></Button>
                  </Link>
                  {canEdit && (
                    <Link to={`/products/${p.id}/edit`}>
                      <Button variant="ghost" size="sm"><Edit size={14} /></Button>
                    </Link>
                  )}
                </div>
              </Td>
            </tr>
          ))}
        </tbody>
      </Table>
    </div>
  )
}
