import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useState, useRef } from 'react'
import { QRCodeSVG } from 'qrcode.react'
import { Printer, Plus, Trash2, Link, Unlink } from 'lucide-react'
import { api } from '../../lib/api'
import { Button, PageHeader, Table, Th, Td, Badge, Modal, FormField, Input, PageSpinner } from '../../components/ui'
import type { ProductShortId, Product } from '../../types'

export default function AdminShortIds() {
  const [assigned, setAssigned] = useState<'all' | 'true' | 'false'>('all')
  const [showGenerate, setShowGenerate] = useState(false)
  const [showAssign, setShowAssign] = useState<ProductShortId | null>(null)
  const [showPrint, setShowPrint] = useState(false)
  const [count, setCount] = useState(10)
  const [label, setLabel] = useState('')
  const [productId, setProductId] = useState('')
  const printRef = useRef<HTMLDivElement>(null)
  const qc = useQueryClient()

  const queryParam = assigned === 'all' ? undefined : assigned
  const { data: shortIds = [], isLoading } = useQuery<ProductShortId[]>({
    queryKey: ['short-ids', assigned],
    queryFn: () =>
      api.get('/short-ids', { params: queryParam !== undefined ? { assigned: queryParam } : {} }).then((r) => r.data),
  })

  const { data: products = [] } = useQuery<Product[]>({
    queryKey: ['products-slim-active'],
    queryFn: () => api.get('/products', { params: { activeOnly: true } }).then((r) => r.data),
  })

  const generateMutation = useMutation({
    mutationFn: () => api.post('/short-ids/generate', { count, label: label || null }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['short-ids'] })
      setShowGenerate(false)
      setCount(10)
      setLabel('')
    },
  })

  const assignMutation = useMutation({
    mutationFn: ({ id, pid }: { id: number; pid: number }) =>
      api.put(`/short-ids/${id}/assign`, { productId: pid }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['short-ids'] })
      setShowAssign(null)
      setProductId('')
    },
  })

  const unassignMutation = useMutation({
    mutationFn: (id: number) => api.put(`/short-ids/${id}/unassign`),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['short-ids'] }),
  })

  const deleteMutation = useMutation({
    mutationFn: (id: number) => api.delete(`/short-ids/${id}`),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['short-ids'] }),
  })

  const handlePrint = () => {
    const win = window.open('', '_blank')
    if (!win || !printRef.current) return
    win.document.write(`<html><head><title>QR Labels</title><style>
      body { margin: 0; font-family: sans-serif; }
      .grid { display: flex; flex-wrap: wrap; gap: 12px; padding: 16px; }
      .label { border: 1px solid #ccc; border-radius: 6px; padding: 10px; text-align: center; width: 110px; }
      .label svg { display: block; margin: 0 auto 4px; }
      .code { font-size: 9px; color: #555; word-break: break-all; }
      .lbl { font-size: 10px; font-weight: 600; margin-bottom: 2px; }
    </style></head><body>`)
    win.document.write(printRef.current.innerHTML)
    win.document.write('</body></html>')
    win.document.close()
    win.print()
  }

  const printableIds = shortIds.filter((s) => assigned === 'all' ? !s.productId : true)

  if (isLoading) return <PageSpinner />

  const baseUrl = window.location.origin

  return (
    <div>
      <PageHeader
        title="QR Labels"
        actions={
          <div className="flex gap-2">
            <Button variant="secondary" onClick={() => setShowPrint(true)}>
              <Printer size={15} /> Print Labels
            </Button>
            <Button onClick={() => setShowGenerate(true)}>
              <Plus size={15} /> Generate Batch
            </Button>
          </div>
        }
      />

      {/* Filter */}
      <div className="mb-4 flex gap-2">
        {(['all', 'false', 'true'] as const).map((v) => (
          <button
            key={v}
            onClick={() => setAssigned(v)}
            className={`rounded-full px-4 py-1 text-sm font-medium transition-colors ${
              assigned === v
                ? 'bg-blue-600 text-white'
                : 'bg-slate-100 text-slate-600 hover:bg-slate-200'
            }`}
          >
            {v === 'all' ? 'All' : v === 'false' ? 'Unassigned' : 'Assigned'}
          </button>
        ))}
        <span className="ml-auto text-sm text-slate-500 self-center">{shortIds.length} labels</span>
      </div>

      <Table>
        <thead>
          <tr>
            <Th>QR Code</Th>
            <Th>Code</Th>
            <Th>Label</Th>
            <Th>Product</Th>
            <Th>Created</Th>
            <Th>Assigned</Th>
            <Th className="text-right">Actions</Th>
          </tr>
        </thead>
        <tbody>
          {shortIds.map((s) => (
            <tr key={s.id} className="hover:bg-slate-50">
              <Td>
                <QRCodeSVG value={`${baseUrl}/p/${s.code}`} size={48} />
              </Td>
              <Td className="font-mono text-sm">{s.code}</Td>
              <Td className="text-slate-500">{s.label ?? '—'}</Td>
              <Td>
                {s.productId ? (
                  <span className="text-sm font-medium">{s.productName ?? `#${s.productId}`}</span>
                ) : (
                  <Badge label="Unassigned" color="slate" />
                )}
              </Td>
              <Td className="text-sm text-slate-500">{new Date(s.createdAt).toLocaleDateString()}</Td>
              <Td className="text-sm text-slate-500">
                {s.assignedAt ? new Date(s.assignedAt).toLocaleDateString() : '—'}
              </Td>
              <Td className="text-right">
                <div className="flex justify-end gap-1">
                  {s.productId ? (
                    <button
                      className="rounded p-1 text-slate-400 hover:bg-orange-50 hover:text-orange-600"
                      title="Unassign product"
                      onClick={() => unassignMutation.mutate(s.id)}
                    >
                      <Unlink size={15} />
                    </button>
                  ) : (
                    <button
                      className="rounded p-1 text-slate-400 hover:bg-blue-50 hover:text-blue-600"
                      title="Assign product"
                      onClick={() => { setShowAssign(s); setProductId('') }}
                    >
                      <Link size={15} />
                    </button>
                  )}
                  <button
                    className="rounded p-1 text-slate-400 hover:bg-red-50 hover:text-red-600"
                    title="Delete"
                    onClick={() => deleteMutation.mutate(s.id)}
                  >
                    <Trash2 size={15} />
                  </button>
                </div>
              </Td>
            </tr>
          ))}
          {shortIds.length === 0 && (
            <tr>
              <td colSpan={7} className="text-center text-slate-400 py-8 text-sm">
                No QR labels found. Generate a batch to get started.
              </td>
            </tr>
          )}
        </tbody>
      </Table>

      {/* Generate modal */}
      <Modal open={showGenerate} onClose={() => setShowGenerate(false)} title="Generate QR Labels">
        <div className="space-y-4">
          <FormField label="Count (1–100)">
            <Input
              type="number"
              min={1}
              max={100}
              value={count}
              onChange={(e) => setCount(Math.min(100, Math.max(1, Number(e.target.value))))}
            />
          </FormField>
          <FormField label="Label (optional)">
            <Input
              placeholder="e.g. Batch Jan 2026"
              value={label}
              onChange={(e) => setLabel(e.target.value)}
            />
          </FormField>
          <div className="flex justify-end gap-2 pt-2">
            <Button variant="secondary" onClick={() => setShowGenerate(false)}>Cancel</Button>
            <Button onClick={() => generateMutation.mutate()} disabled={generateMutation.isPending}>
              {generateMutation.isPending ? 'Generating…' : `Generate ${count} Labels`}
            </Button>
          </div>
        </div>
      </Modal>

      {/* Assign modal */}
      <Modal open={!!showAssign} onClose={() => setShowAssign(null)} title="Assign Product">
        {showAssign && (
          <div className="space-y-4">
            <p className="text-sm text-slate-500">
              Assign a product to label <span className="font-mono font-semibold">{showAssign.code}</span>
            </p>
            <FormField label="Product">
              <select
                className="w-full rounded-md border border-slate-200 bg-white px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                value={productId}
                onChange={(e) => setProductId(e.target.value)}
              >
                <option value="">— select product —</option>
                {products.map((p) => (
                  <option key={p.id} value={p.id}>{p.name}</option>
                ))}
              </select>
            </FormField>
            <div className="flex justify-end gap-2 pt-2">
              <Button variant="secondary" onClick={() => setShowAssign(null)}>Cancel</Button>
              <Button
                onClick={() => productId && assignMutation.mutate({ id: showAssign.id, pid: Number(productId) })}
                disabled={!productId || assignMutation.isPending}
              >
                {assignMutation.isPending ? 'Assigning…' : 'Assign'}
              </Button>
            </div>
          </div>
        )}
      </Modal>

      {/* Print modal */}
      <Modal open={showPrint} onClose={() => setShowPrint(false)} title="Print QR Labels">
        <div className="space-y-4">
          <p className="text-sm text-slate-500">
            {shortIds.length} label{shortIds.length !== 1 ? 's' : ''} will be printed.
          </p>
          {/* Hidden print grid */}
          <div ref={printRef} style={{ display: 'none' }}>
            <div className="grid">
              {shortIds.map((s) => (
                <div key={s.id} className="label">
                  {s.label && <div className="lbl">{s.label}</div>}
                  <QRCodeSVG value={`${baseUrl}/p/${s.code}`} size={80} />
                  <div className="code">{s.code}</div>
                  {s.productName && <div className="lbl" style={{ marginTop: 4 }}>{s.productName}</div>}
                </div>
              ))}
            </div>
          </div>
          <div className="flex justify-end gap-2 pt-2">
            <Button variant="secondary" onClick={() => setShowPrint(false)}>Cancel</Button>
            <Button onClick={handlePrint}><Printer size={15} /> Print</Button>
          </div>
        </div>
      </Modal>
    </div>
  )
}
