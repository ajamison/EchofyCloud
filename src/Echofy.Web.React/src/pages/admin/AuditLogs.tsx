import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { ChevronLeft, ChevronRight } from 'lucide-react'
import { api } from '../../lib/api'
import { Badge, Card, PageHeader, PageSpinner, Table, Td, Th } from '../../components/ui'
import type { AuditLog, AuditLogPage } from '../../types'

const ENTITY_NAMES = ['Product', 'Order', 'Customer', 'Lead', 'Deal', 'ManufacturerProduct']
const ACTIONS = ['Created', 'Updated', 'Deleted']

function actionColor(action: string): 'green' | 'blue' | 'red' {
  if (action === 'Created') return 'green'
  if (action === 'Deleted') return 'red'
  return 'blue'
}

function JsonDiff({ label, json }: { label: string; json: string | null }) {
  if (!json) return null
  let parsed: Record<string, unknown>
  try { parsed = JSON.parse(json) } catch { return <pre className="text-xs">{json}</pre> }
  return (
    <div>
      <p className="mb-1 text-xs font-semibold text-slate-500">{label}</p>
      <div className="space-y-0.5">
        {Object.entries(parsed).map(([k, v]) => (
          <div key={k} className="flex gap-2 text-xs">
            <span className="w-36 shrink-0 text-slate-400">{k}</span>
            <span className="break-all font-mono text-slate-700">{v === null ? 'null' : String(v)}</span>
          </div>
        ))}
      </div>
    </div>
  )
}

export default function AuditLogs() {
  const [entityName, setEntityName] = useState('')
  const [action, setAction] = useState('')
  const [page, setPage] = useState(1)
  const [expanded, setExpanded] = useState<number | null>(null)
  const pageSize = 50

  const { data, isLoading } = useQuery<AuditLogPage>({
    queryKey: ['audit-logs', entityName, action, page],
    queryFn: () => {
      const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) })
      if (entityName) params.set('entityName', entityName)
      if (action) params.set('action', action)
      return api.get(`/admin/audit-logs?${params}`).then((r) => r.data)
    },
  })

  const totalPages = data ? Math.ceil(data.total / pageSize) : 1

  function handleFilterChange() {
    setPage(1)
    setExpanded(null)
  }

  if (isLoading) return <PageSpinner />

  return (
    <div>
      <PageHeader title="Audit Logs" subtitle={data ? `${data.total.toLocaleString()} entries` : ''} />

      <div className="mb-4 flex flex-wrap gap-3">
        <select
          value={entityName}
          onChange={(e) => { setEntityName(e.target.value); handleFilterChange() }}
          className="rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/30"
        >
          <option value="">All entities</option>
          {ENTITY_NAMES.map((n) => <option key={n} value={n}>{n}</option>)}
        </select>
        <select
          value={action}
          onChange={(e) => { setAction(e.target.value); handleFilterChange() }}
          className="rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/30"
        >
          <option value="">All actions</option>
          {ACTIONS.map((a) => <option key={a} value={a}>{a}</option>)}
        </select>
      </div>

      <Table>
        <thead>
          <tr>
            <Th>When</Th>
            <Th>Entity</Th>
            <Th>ID</Th>
            <Th>Action</Th>
            <Th>Changed By</Th>
            <Th />
          </tr>
        </thead>
        <tbody>
          {data?.logs.map((log: AuditLog) => (
            <>
              <tr
                key={log.id}
                className="cursor-pointer hover:bg-slate-50"
                onClick={() => setExpanded(expanded === log.id ? null : log.id)}
              >
                <Td className="whitespace-nowrap text-xs text-slate-500">
                  {new Date(log.changedAt).toLocaleString()}
                </Td>
                <Td className="font-medium">{log.entityName}</Td>
                <Td className="font-mono text-xs">{log.entityId ?? '—'}</Td>
                <Td>
                  <Badge label={log.action} color={actionColor(log.action)} />
                </Td>
                <Td className="font-mono text-xs">{log.changedByUserId?.slice(0, 8) ?? '—'}</Td>
                <Td className="text-right text-xs text-slate-400">
                  {expanded === log.id ? '▲' : '▼'}
                </Td>
              </tr>
              {expanded === log.id && (
                <tr key={`${log.id}-detail`}>
                  <td colSpan={6} className="border-t border-slate-100 bg-slate-50 px-6 py-4">
                    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                      {log.action !== 'Created' && (
                        <Card className="!p-4">
                          <JsonDiff label="Before" json={log.oldValues} />
                        </Card>
                      )}
                      {log.action !== 'Deleted' && (
                        <Card className="!p-4">
                          <JsonDiff label="After" json={log.newValues} />
                        </Card>
                      )}
                    </div>
                  </td>
                </tr>
              )}
            </>
          ))}
          {!data?.logs.length && (
            <tr>
              <td colSpan={6} className="py-10 text-center text-sm text-slate-400">No audit logs found.</td>
            </tr>
          )}
        </tbody>
      </Table>

      {totalPages > 1 && (
        <div className="mt-4 flex items-center justify-end gap-2">
          <button
            onClick={() => setPage((p) => Math.max(1, p - 1))}
            disabled={page === 1}
            className="rounded-lg border border-slate-200 p-1.5 hover:bg-slate-50 disabled:opacity-40"
          >
            <ChevronLeft size={16} />
          </button>
          <span className="text-sm text-slate-600">Page {page} of {totalPages}</span>
          <button
            onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
            disabled={page === totalPages}
            className="rounded-lg border border-slate-200 p-1.5 hover:bg-slate-50 disabled:opacity-40"
          >
            <ChevronRight size={16} />
          </button>
        </div>
      )}
    </div>
  )
}
