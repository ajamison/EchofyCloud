import { useParams, Link } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { ArrowLeft, Copy, Check, Mail, MessageSquare, Share2, Gift } from 'lucide-react'
import { useState } from 'react'
import { api } from '../../lib/api'
import { PageHeader, Card, Badge, PageSpinner, Button } from '../../components/ui'
import type { InvoiceDto, ReferralDto } from '../../types'

const statusColor: Record<string, 'slate' | 'blue' | 'green' | 'yellow' | 'red' | 'purple'> = {
  Draft: 'slate', Sent: 'blue', Viewed: 'purple', Paid: 'green', Overdue: 'red', Cancelled: 'red',
}

function useCopy(timeout = 2000) {
  const [copied, setCopied] = useState(false)
  const copy = (text: string) => {
    navigator.clipboard.writeText(text).then(() => {
      setCopied(true)
      setTimeout(() => setCopied(false), timeout)
    })
  }
  return { copied, copy }
}

function CopyButton({ text, label = 'Copy' }: { text: string; label?: string }) {
  const { copied, copy } = useCopy()
  return (
    <button
      onClick={() => copy(text)}
      className={`inline-flex items-center gap-1.5 rounded-lg border px-3 py-1.5 text-xs font-medium transition-colors
        ${copied ? 'border-green-300 bg-green-50 text-green-700' : 'border-slate-200 bg-white text-slate-600 hover:bg-slate-50 dark:border-slate-600 dark:bg-slate-700 dark:text-slate-300'}`}
    >
      {copied ? <Check size={12} /> : <Copy size={12} />}
      {copied ? 'Copied!' : label}
    </button>
  )
}

export default function CustomerInvoiceDetail() {
  const { id } = useParams<{ id: string }>()

  const { data: inv, isLoading: invLoading } = useQuery<InvoiceDto>({
    queryKey: ['me', 'invoices', id],
    queryFn: () => api.get(`/me/invoices/${id}`).then(r => r.data),
  })

  const { data: referral } = useQuery<ReferralDto>({
    queryKey: ['me', 'referral'],
    queryFn: () => api.get('/me/referral').then(r => r.data),
  })

  if (invLoading) return <PageSpinner />
  if (!inv) return <div className="p-6 text-slate-500">Invoice not found.</div>

  const shareUrl  = referral
    ? `${window.location.origin}/register?ref=${encodeURIComponent(referral.code)}`
    : ''
  const shareText = referral
    ? `Hey! I've been using ${inv.customerName?.split(' ')[0] ? 'this amazing service' : 'an amazing service'} and wanted to invite you. Sign up with my referral code ${referral.code} and get $5 off your first order: ${shareUrl}`
    : ''
  const mailtoHref = `mailto:?subject=${encodeURIComponent("I think you'd love this service!")}&body=${encodeURIComponent(shareText)}`
  const smsHref    = `sms:?body=${encodeURIComponent(shareText)}`

  return (
    <div className="space-y-6">
      <PageHeader
        title={inv.invoiceNumber}
        subtitle={`Due ${new Date(inv.dueDate).toLocaleDateString()}`}
        actions={
          <Link to="/customer/invoices">
            <Button variant="ghost" size="sm"><ArrowLeft size={14} /> Back</Button>
          </Link>
        }
      />

      {/* Invoice summary */}
      <Card>
        <div className="flex flex-wrap items-start justify-between gap-4">
          <div>
            <p className="text-2xl font-bold text-slate-800 dark:text-slate-100">{inv.invoiceNumber}</p>
            <Badge label={inv.status} color={statusColor[inv.status] ?? 'slate'} />
          </div>
          <div className="text-right">
            <p className="text-xs text-slate-400">Total Due</p>
            <p className="text-3xl font-bold text-slate-800 dark:text-slate-100">${inv.total.toFixed(2)}</p>
          </div>
        </div>
        <div className="mt-6 grid gap-4 sm:grid-cols-2 text-sm text-slate-600 dark:text-slate-300">
          <div>
            <p className="text-xs font-semibold uppercase tracking-wide text-slate-400 mb-1">Billed To</p>
            <p className="font-medium">{inv.customerName}</p>
            <p>{inv.customerEmail}</p>
            {inv.customerPhone && <p>{inv.customerPhone}</p>}
          </div>
          <div>
            <p className="text-xs font-semibold uppercase tracking-wide text-slate-400 mb-1">Dates</p>
            <p>Issued: {new Date(inv.issuedDate).toLocaleDateString()}</p>
            <p>Due: {new Date(inv.dueDate).toLocaleDateString()}</p>
            {inv.paidAt && <p className="font-medium text-green-600">Paid: {new Date(inv.paidAt).toLocaleDateString()}</p>}
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

      {inv.rewardGiftCardAmount > 0 && inv.rewardGiftCardCode && (
        <Card>
          <div className="flex items-center gap-2 mb-2">
            <Gift size={18} className="text-green-500" />
            <h2 className="font-semibold text-slate-800 dark:text-slate-100">You earned a gift card!</h2>
          </div>
          <p className="text-sm text-slate-500 dark:text-slate-400 mb-3">
            Thank you for your payment! Here is your <strong>${inv.rewardGiftCardAmount.toFixed(2)} gift card</strong> to use on your next order.
          </p>
          <div className="flex flex-wrap items-center gap-3">
            <div className="rounded-xl border-2 border-green-400 bg-green-50 px-5 py-2 dark:bg-green-900/20">
              <span className="text-2xl font-bold tracking-widest text-green-700 dark:text-green-400">{inv.rewardGiftCardCode}</span>
            </div>
            <CopyButton text={inv.rewardGiftCardCode} label="Copy Code" />
          </div>
        </Card>
      )}

      {/* Referral section — shown to all customers */}
      {referral && (
        <Card>
          <div className="flex items-center gap-2 mb-2">
            <Share2 size={18} className="text-primary" />
            <h2 className="font-semibold text-slate-800 dark:text-slate-100">Love our service? Refer a friend!</h2>
          </div>
          <p className="text-sm text-slate-500 dark:text-slate-400 mb-4">
            Share your referral code with friends. They get <strong>$5 off</strong> their first order and you earn <strong>100 points</strong>.
          </p>
          <div className="flex flex-wrap items-center gap-3 mb-4">
            <div className="rounded-xl border-2 border-primary bg-blue-50 px-5 py-2 dark:bg-blue-900/20">
              <span className="text-2xl font-bold tracking-widest text-primary">{referral.code}</span>
            </div>
            <CopyButton text={referral.code} label="Copy Code" />
          </div>
          <div className="flex flex-wrap gap-2">
            <a
              href={mailtoHref}
              className="inline-flex items-center gap-2 rounded-lg bg-primary px-4 py-2 text-sm font-semibold text-white hover:bg-primary-600 transition-colors"
            >
              <Mail size={14} /> Share via Email
            </a>
            <a
              href={smsHref}
              className="inline-flex items-center gap-2 rounded-lg bg-green-600 px-4 py-2 text-sm font-semibold text-white hover:bg-green-700 transition-colors"
            >
              <MessageSquare size={14} /> Share via Text
            </a>
            <CopyButton text={shareText} label="Copy Message" />
          </div>
          <p className="mt-3 text-xs text-slate-400">
            You have {referral.totalReferrals} referral{referral.totalReferrals !== 1 ? 's' : ''} · {referral.totalPoints} points earned.
          </p>
        </Card>
      )}
    </div>
  )
}
