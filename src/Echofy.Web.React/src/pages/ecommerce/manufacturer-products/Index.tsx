import { useState } from 'react'
import { Link } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { Plus, Trash2, Eye, Package } from 'lucide-react'
import { api } from '../../../lib/api'
import { Button, Badge, PageHeader, Table, Th, Td, PageSpinner } from '../../../components/ui'
import type { ManufacturerProduct } from '../../../types'
import { useAuth } from '../../../lib/auth'

export default function ManufacturerProducts() {
  const { user } = useAuth()
  const canEdit = user?.role === 'Admin' || user?.role === 'Manager'
  const qc = useQueryClient()
  const [search, setSearch] = useState('')

  const { data: items = [], isLoading } = useQuery<ManufacturerProduct[]>({
    queryKey: ['manufacturer-products'],
    queryFn: () => api.get('/manufacturer-products').then((r) => r.data),
  })

  const deleteMutation = useMutation({
    mutationFn: (id: number) => api.delete(`/manufacturer-products/${id}`),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['manufacturer-products'] }),
  })

  const filtered = items.filter((mp) =>
    !search ||
    mp.name.toLowerCase().includes(search.toLowerCase()) ||
    mp.manufacturerName.toLowerCase().includes(search.toLowerCase()) ||
    (mp.manufacturerPartNumber ?? '').toLowerCase().includes(search.toLowerCase())
  )

  if (isLoading) return <PageSpinner />

  return (
    <div>
      <PageHeader
        title="Manufacturer Products"
        subtitle={`${items.length} entries`}
        actions={
          canEdit && (
            <Link to="/manufacturer-products/new">
              <Button><Plus size={14} /> New</Button>
            </Link>
          )
        }
      />
      <div className="mb-4">
        <input
          type="text"
          placeholder="Search by manufacturer or part number…"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          className="w-full max-w-sm rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/30"
        />
      </div>
      <Table>
        <thead>
          <tr>
            <Th>Image</Th>
            <Th>Manufacturer</Th>
            <Th>Name</Th>
            <Th>Part Number</Th>
            <Th>Products</Th>
            <Th>Status</Th>
            <Th>Created</Th>
            <Th />
          </tr>
        </thead>
        <tbody>
          {filtered.map((mp) => (
            <tr key={mp.id} className="hover:bg-slate-50">
              <Td className="w-14">
                {mp.mainImageFileName ? (
                  <img
                    src={`/uploads/manufacturer-products/${mp.mainImageFileName}`}
                    alt=""
                    className="h-10 w-10 rounded-lg object-cover"
                  />
                ) : (
                  <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-slate-100">
                    <Package size={16} className="text-slate-400" />
                  </div>
                )}
              </Td>
              <Td className="font-medium">{mp.manufacturerName}</Td>
              <Td>{mp.name}</Td>
              <Td>{mp.manufacturerPartNumber ?? <span className="text-slate-400">—</span>}</Td>
              <Td>{mp.productCount}</Td>
              <Td>
                <Badge label={mp.isActive ? 'Active' : 'Inactive'} color={mp.isActive ? 'green' : 'slate'} />
              </Td>
              <Td>{new Date(mp.createdAt).toLocaleDateString()}</Td>
              <Td className="text-right">
                <div className="flex items-center justify-end gap-2">
                  <Link to={`/manufacturer-products/${mp.id}`}>
                    <Button variant="ghost" size="sm"><Eye size={14} /></Button>
                  </Link>
                  {canEdit && (
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => {
                        if (confirm('Delete this manufacturer product?')) deleteMutation.mutate(mp.id)
                      }}
                    >
                      <Trash2 size={14} className="text-red-500" />
                    </Button>
                  )}
                </div>
              </Td>
            </tr>
          ))}
          {filtered.length === 0 && (
            <tr>
              <td colSpan={7} className="py-10 text-center text-sm text-slate-400">No entries found.</td>
            </tr>
          )}
        </tbody>
      </Table>
    </div>
  )
}
