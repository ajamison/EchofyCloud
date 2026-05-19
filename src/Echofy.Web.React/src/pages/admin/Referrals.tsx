import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { Star, Users, Clock, Check, X } from 'lucide-react'
import { api } from '../../lib/api'
import { useAuth } from '../../lib/auth'
import { PageHeader, Card, PageSpinner, Badge, StatCard, Table, Th, Td, Button } from '../../components/ui'
import type { AdminReferralDto } from '../../types'

function StatusBadge({ status }: { status: string }) {
  const map: Record<string, 'blue' | 'green' | 'red' | 'yellow' | 'slate'> = {
    Pending:   'yellow',
    Approved:  'blue',
    Issued:    'green',
    Cancelled: 'red',
  }
  return <Badge label={status} color={map[status] ?? 'slate'} />
}

export default function AdminReferrals() {
  const { canWrite } = useAuth()
  const qc = useQueryClient()

  const { data: referrals = [], isLoading } = useQuery<AdminReferralDto[]>({
    queryKey: ['admin-referrals'],
    queryFn: () => api.get('/admin/referrals').then(r => r.data),
  })

  const approve = useMutation({
    mutationFn: (id: number) => api.post(`/admin/referrals/${id}/approve`),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['admin-referrals'] }),
  })
  const issue = useMutation({
    mutationFn: (id: number) => api.post(`/admin/referrals/${id}/issue`),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['admin-referrals'] }),
  })
  const cancel = useMutation({
    mutationFn: (id: number) => api.post(`/admin/referrals/${id}/cancel`),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['admin-referrals'] }),
  })

  if (isLoading) return <PageSpinner />

  const pending  = referrals.filter(r => r.status === 'Pending')
  const approved = referrals.filter(r => r.status === 'Approved')
  const issued   = referrals.filter(r => r.status === 'Issued')
  const totalPointsIssued = issued.reduce((s, r) => s + r.pointsEarned, 0)

  return (
    <div className="space-y-6">
      <PageHeader title="Referral Management" subtitle="Review and process customer referral rewards." />

      <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
        <StatCard icon={<Users size={20} />}  label="Total Referrals"  value={referrals.length}       color="bg-blue-500" />
        <StatCard icon={<Clock size={20} />}  label="Pending"          value={pending.length}         color="bg-yellow-500" />
        <StatCard icon={<Check size={20} />}  label="Approved"         value={approved.length}        color="bg-blue-600" />
        <StatCard icon={<Star size={20} />}   label="Points Issued"    value={totalPointsIssued}      color="bg-green-600" />
      </div>

      <Card>
        <Table>
          <thead>
            <tr>
              <Th>Referrer</Th>
              <Th>Code</Th>
              <Th>New Member</Th>
              <Th>Welcome Coupon</Th>
              <Th>Points</Th>
              <Th>Status</Th>
              <Th>Used At</Th>
              {canWrite() && <Th>Actions</Th>}
            </tr>
          </thead>
          <tbody>
            {referrals.length === 0 && (
              <tr>
                <td colSpan={canWrite() ? 8 : 7} className="py-10 text-center text-sm text-slate-400">No referrals yet.</td>
              </tr>
            )}
            {referrals.map(r => (
              <tr key={r.rewardId} className="hover:bg-slate-50">
                <Td>
                  <div className="font-medium text-slate-800">{r.referrerName}</div>
                  <div className="text-xs text-slate-400">{r.referrerEmail}</div>
                </Td>
                <Td>
                  <code className="rounded bg-blue-50 px-1.5 py-0.5 text-xs font-bold text-blue-700">{r.referralCode}</code>
                </Td>
                <Td className="text-slate-500">{r.usedByEmail}</Td>
                <Td>
                  {r.welcomeCoupon
                    ? <code className="rounded bg-green-50 px-1.5 py-0.5 text-xs text-green-700">{r.welcomeCoupon}</code>
                    : <span className="text-slate-300">—</span>}
                </Td>
                <Td>
                  <span className="inline-flex items-center gap-1 font-semibold text-yellow-600">
                    <Star size={13} /> {r.pointsEarned}
                  </span>
                </Td>
                <Td><StatusBadge status={r.status} /></Td>
                <Td className="text-slate-500">{new Date(r.usedAt).toLocaleDateString()}</Td>
                {canWrite() && (
                  <Td>
                    <div className="flex items-center gap-1.5">
                      {r.status === 'Pending' && (
                        <>
                          <Button size="sm" onClick={() => approve.mutate(r.rewardId)} loading={approve.isPending} title="Approve">
                            <Check size={13} /> Approve
                          </Button>
                          <Button size="sm" variant="danger" onClick={() => cancel.mutate(r.rewardId)} loading={cancel.isPending} title="Cancel">
                            <X size={13} />
                          </Button>
                        </>
                      )}
                      {r.status === 'Approved' && (
                        <>
                          <Button size="sm" onClick={() => issue.mutate(r.rewardId)} loading={issue.isPending} title="Issue Points">
                            <Star size={13} /> Issue
                          </Button>
                          <Button size="sm" variant="danger" onClick={() => cancel.mutate(r.rewardId)} loading={cancel.isPending} title="Cancel">
                            <X size={13} />
                          </Button>
                        </>
                      )}
                      {(r.status === 'Issued' || r.status === 'Cancelled') && (
                        <span className="text-xs text-slate-400">—</span>
                      )}
                    </div>
                  </Td>
                )}
              </tr>
            ))}
          </tbody>
        </Table>
      </Card>
    </div>
  )
}
