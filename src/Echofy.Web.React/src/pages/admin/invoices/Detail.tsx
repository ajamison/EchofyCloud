import { useParams, useNavigate, Link } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { Send, CheckCircle, XCircle, Edit, ArrowLeft, Printer, Heart, Gift, Trophy } from 'lucide-react'
import { api } from '../../../lib/api'
import { PageHeader, Card, Badge, Button, PageSpinner, Modal, Textarea, FormField } from '../../../components/ui'
import type { InvoiceDto } from '../../../types'

const statusColor: Record<string, 'slate' | 'blue' | 'green' | 'yellow' | 'red' | 'purple'> = {
  Draft: 'slate', Sent: 'blue', Viewed: 'purple', Paid: 'green', Overdue: 'red', Cancelled: 'red',
}

export default function AdminInvoiceDetail() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const qc = useQueryClient()

  const [thankYouOpen, setThankYouOpen] = useState(false)
  const [customMessage, setCustomMessage] = useState('')

  const { data: inv, isLoading } = useQuery<InvoiceDto>({
    queryKey: ['admin-invoices', id],
    queryFn: () => api.get(`/admin/invoices/${id}`).then(r => r.data),
  })

  const invalidate = () => qc.invalidateQueries({ queryKey: ['admin-invoices'] })
  const sendMutation       = useMutation({ mutationFn: () => api.post(`/admin/invoices/${id}/send`),      onSuccess: invalidate })
  const paidMutation       = useMutation({ mutationFn: () => api.post(`/admin/invoices/${id}/mark-paid`), onSuccess: invalidate })
  const cancelMutation     = useMutation({ mutationFn: () => api.post(`/admin/invoices/${id}/cancel`),    onSuccess: invalidate })
  const thankYouMutation   = useMutation({
    mutationFn: () => api.post(`/admin/invoices/${id}/send-thank-you`, { customMessage: customMessage || null }),
    onSuccess: () => { invalidate(); setThankYouOpen(false); setCustomMessage('') },
  })

  if (isLoading) return <PageSpinner />
  if (!inv) return <div className="p-6 text-slate-500">Invoice not found.</div>

  const canEdit   = inv.status === 'Draft'
  const canSend   = inv.status !== 'Paid' && inv.status !== 'Cancelled'
  const canPaid   = inv.status !== 'Paid' && inv.status !== 'Cancelled'
  const canCancel = inv.status !== 'Paid' && inv.status !== 'Cancelled'
  const canThankYou = inv.status !== 'Draft' && inv.status !== 'Cancelled' && !inv.thankYouNote

  return (
    <div className="space-y-6">
      <PageHeader
        title={inv.invoiceNumber}
        subtitle={`Issued ${new Date(inv.issuedDate).toLocaleDateString()} · Due ${new Date(inv.dueDate).toLocaleDateString()}`}
        actions={
          <div className="flex items-center gap-2">
            <Button variant="ghost" size="sm" onClick={() => navigate('/admin/invoices')}>
              <ArrowLeft size={14} /> Back
            </Button>
            {canEdit && (
              <Link to={`/admin/invoices/${id}/edit`}>
                <Button variant="secondary" size="sm"><Edit size={14} /> Edit</Button>
              </Link>
            )}
            <Button variant="ghost" size="sm" onClick={() => window.print()} title="Print">
              <Printer size={14} />
            </Button>
          </div>
        }
      />

      {/* Header card */}
      <Card>
        <div className="flex flex-wrap items-start justify-between gap-4">
          <div>
            <p className="text-2xl font-bold text-slate-800 dark:text-slate-100">{inv.invoiceNumber}</p>
            <Badge label={inv.status} color={statusColor[inv.status] ?? 'slate'} />
          </div>
          <div className="flex flex-wrap gap-2">
            {canSend && (
              <Button size="sm" onClick={() => sendMutation.mutate()} loading={sendMutation.isPending}>
                <Send size={14} /> Mark Sent
              </Button>
            )}
            {canPaid && (
              <Button size="sm" variant="secondary" onClick={() => paidMutation.mutate()} loading={paidMutation.isPending}
                title="Marks as paid and automatically sends a thank-you email with invoice link and referral code">
                <CheckCircle size={14} /> Mark Paid
              </Button>
            )}
            {canCancel && (
              <Button size="sm" variant="danger" onClick={() => cancelMutation.mutate()} loading={cancelMutation.isPending}>
                <XCircle size={14} /> Cancel
              </Button>
            )}
            {canThankYou && (
              <Button size="sm" onClick={() => setThankYouOpen(true)}
                className="bg-pink-600 hover:bg-pink-700 text-white">
                <Heart size={14} /> Send Thank You
              </Button>
            )}
          </div>
        </div>

        {/* Thank-you sent badge */}
        {inv.thankYouNote && (
          <div className="mt-4 flex items-start gap-2 rounded-lg bg-pink-50 border border-pink-200 px-3 py-2 dark:bg-pink-900/20 dark:border-pink-800">
            <Heart size={14} className="mt-0.5 text-pink-500 shrink-0" />
            <div className="text-sm text-pink-700 dark:text-pink-300">
              <span>Thank-you email sent {new Date(inv.thankYouNote.sentAt).toLocaleDateString()} — includes invoice link</span>
              {inv.thankYouNote.referralIncluded && inv.thankYouNote.referralCode && (
                <> and referral code <strong>{inv.thankYouNote.referralCode}</strong></>
              )}
            </div>
          </div>
        )}

        <div className="mt-6 grid gap-6 sm:grid-cols-2">
          <div>
            <p className="text-xs font-semibold uppercase tracking-wide text-slate-400">Billed To</p>
            <p className="mt-1 font-semibold text-slate-800 dark:text-slate-100">{inv.customerName}</p>
            <p className="text-sm text-slate-500">{inv.customerEmail}</p>
            {inv.customerPhone && <p className="text-sm text-slate-500">{inv.customerPhone}</p>}
          </div>
          <div className="sm:text-right">
            <p className="text-xs font-semibold uppercase tracking-wide text-slate-400">Dates</p>
            <p className="mt-1 text-sm text-slate-600 dark:text-slate-300">
              Issued: {new Date(inv.issuedDate).toLocaleDateString()}
            </p>
            <p className="text-sm text-slate-600 dark:text-slate-300">
              Due: {new Date(inv.dueDate).toLocaleDateString()}
            </p>
            {inv.paidAt && (
              <p className="text-sm font-medium text-green-600">
                Paid: {new Date(inv.paidAt).toLocaleDateString()}
              </p>
            )}
          </div>
        </div>
      </Card>

      {/* Total */}
      <Card>
        <div className="flex items-center justify-between">
          <span className="text-sm font-semibold uppercase tracking-wide text-slate-400">Total Amount</span>
          <span className="text-3xl font-bold text-slate-800 dark:text-slate-100">${inv.total.toFixed(2)}</span>
        </div>
      </Card>

      {inv.notes && (
        <Card>
          <p className="text-xs font-semibold uppercase tracking-wide text-slate-400">Notes</p>
          <p className="mt-2 whitespace-pre-wrap text-sm text-slate-600 dark:text-slate-300">{inv.notes}</p>
        </Card>
      )}

      {inv.status === 'Paid' && (inv.rewardPointsAwarded > 0 || inv.rewardGiftCardAmount > 0) && (
        <Card>
          <div className="flex items-center gap-2 mb-3">
            <Trophy size={16} className="text-yellow-500" />
            <p className="text-xs font-semibold uppercase tracking-wide text-slate-400">Reward Applied</p>
          </div>
          <div className="flex flex-wrap gap-4 text-sm">
            {inv.rewardPointsAwarded > 0 && (
              <div>
                <p className="text-xs text-slate-400">Points Awarded (Referrer)</p>
                <p className="font-bold text-blue-600">{inv.rewardPointsAwarded} pts</p>
              </div>
            )}
            {inv.rewardGiftCardAmount > 0 && (
              <div>
                <p className="text-xs text-slate-400">Gift Card (Customer)</p>
                <p className="font-bold text-green-600">${inv.rewardGiftCardAmount.toFixed(2)}</p>
                {inv.rewardGiftCardCode && (
                  <p className="font-mono text-xs text-slate-500">{inv.rewardGiftCardCode}</p>
                )}
              </div>
            )}
          </div>
        </Card>
      )}

      {/* Thank You modal */}
      <Modal open={thankYouOpen} onClose={() => setThankYouOpen(false)} title="Send Thank You Note">
        <div className="space-y-4">
          <div className="rounded-lg bg-pink-50 border border-pink-200 p-3 dark:bg-pink-900/20 dark:border-pink-800">
            <p className="text-sm text-pink-800 dark:text-pink-300">
              An email will be sent to <strong>{inv.customerEmail}</strong> thanking them for their business.
            </p>
            {inv.thankYouNote === null && inv.status === 'Paid' && (
              <div className="mt-2 flex items-start gap-2">
                <Gift size={14} className="mt-0.5 shrink-0 text-pink-600" />
                <p className="text-xs text-pink-700 dark:text-pink-400">
                  If this customer has an account, their referral code will be included so they can earn rewards by sharing.
                </p>
              </div>
            )}
          </div>

          <FormField label="Personal message (optional)">
            <Textarea
              placeholder="e.g. It was a pleasure working on your project. We hope to see you again soon!"
              rows={4}
              value={customMessage}
              onChange={e => setCustomMessage(e.target.value)}
            />
          </FormField>

          <div className="rounded-lg bg-slate-50 border border-slate-200 p-3 text-xs text-slate-500 dark:bg-slate-700 dark:border-slate-600 dark:text-slate-400">
            <p className="font-medium text-slate-700 dark:text-slate-300 mb-1">Email will include:</p>
            <ul className="space-y-0.5 list-disc list-inside">
              <li>Thank-you greeting</li>
              {customMessage && <li>Your personal message</li>}
              <li>A button to view their invoice</li>
              {inv.status === 'Paid' && <li>Their referral code + share link (if registered)</li>}
              {inv.status === 'Paid' && <li>How the referral rewards work</li>}
            </ul>
          </div>

          <div className="flex justify-end gap-2 pt-1">
            <Button variant="secondary" onClick={() => setThankYouOpen(false)}>Cancel</Button>
            <Button
              onClick={() => thankYouMutation.mutate()}
              loading={thankYouMutation.isPending}
              className="bg-pink-600 hover:bg-pink-700 text-white"
            >
              <Heart size={14} /> Send Thank You
            </Button>
          </div>
        </div>
      </Modal>
    </div>
  )
}
