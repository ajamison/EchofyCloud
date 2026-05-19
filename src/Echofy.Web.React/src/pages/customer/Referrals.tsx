import { useQuery } from '@tanstack/react-query'
import { Copy, Check, Mail, MessageSquare, Star, Users, Clock, Share2, FileText } from 'lucide-react'
import { useState } from 'react'
import { api } from '../../lib/api'
import { PageHeader, Card, PageSpinner, Badge, StatCard } from '../../components/ui'
import type { ReferralDto, ReferralRewardDto, InvoiceListItemDto } from '../../types'

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

function CopyButton({ text, label = 'Copy', size = 'sm' }: { text: string; label?: string; size?: 'sm' | 'md' }) {
  const { copied, copy } = useCopy()
  return (
    <button
      onClick={() => copy(text)}
      className={`inline-flex items-center gap-1.5 rounded-lg border font-medium transition-colors
        ${size === 'sm' ? 'px-3 py-1.5 text-xs' : 'px-4 py-2 text-sm'}
        ${copied
          ? 'border-green-300 bg-green-50 text-green-700'
          : 'border-slate-200 bg-white text-slate-600 hover:bg-slate-50'
        }`}
    >
      {copied ? <Check size={13} /> : <Copy size={13} />}
      {copied ? 'Copied!' : label}
    </button>
  )
}

function StatusBadge({ status }: { status: string }) {
  const map: Record<string, 'blue' | 'green' | 'red' | 'yellow' | 'slate'> = {
    Pending:   'yellow',
    Approved:  'blue',
    Issued:    'green',
    Cancelled: 'red',
  }
  return <Badge label={status} color={map[status] ?? 'slate'} />
}

function PointsAlert({ reward }: { reward: ReferralRewardDto }) {
  return (
    <div className="flex items-start gap-3 rounded-xl border border-yellow-200 bg-yellow-50 p-4">
      <Star size={20} className="mt-0.5 shrink-0 text-yellow-600" />
      <div className="flex-1 min-w-0">
        <p className="font-semibold text-yellow-800">{reward.pointsEarned} points credited!</p>
        <p className="mt-0.5 text-sm text-yellow-700">Your referral reward has been issued. Points are added to your account.</p>
      </div>
    </div>
  )
}

export default function CustomerReferrals() {
  const [selectedInvoiceId, setSelectedInvoiceId] = useState<number | null>(null)

  const { data: referral, isLoading } = useQuery<ReferralDto>({
    queryKey: ['me', 'referral'],
    queryFn: () => api.get('/me/referral').then(r => r.data),
  })

  const { data: invoices = [] } = useQuery<InvoiceListItemDto[]>({
    queryKey: ['me', 'invoices'],
    queryFn: () => api.get('/me/invoices').then(r => r.data),
    enabled: !!referral,
  })

  if (isLoading) return <PageSpinner />
  if (!referral) return null

  const paidInvoices = invoices.filter(inv => inv.status === 'Paid')
  const selectedInvoice = paidInvoices.find(inv => inv.id === selectedInvoiceId) ?? null

  const shareUrl  = `${window.location.origin}/register?ref=${encodeURIComponent(referral.code)}`
  const shareText = selectedInvoice
    ? `Hey! I've been a customer${selectedInvoice.companyName ? ` of ${selectedInvoice.companyName}` : ''} (invoice ${selectedInvoice.invoiceNumber} — $${selectedInvoice.total.toFixed(2)}) and wanted to invite you. Sign up with my referral code ${referral.code} and you'll get $5 off your first order: ${shareUrl}`
    : `Hey! I've been using Echofy and wanted to invite you. Sign up with my referral code ${referral.code} and you'll get $5 off your first order. Sign up here: ${shareUrl}`

  const mailtoHref = `mailto:?subject=${encodeURIComponent("You're invited — get $5 off!")}&body=${encodeURIComponent(shareText)}`
  const smsHref    = `sms:?body=${encodeURIComponent(shareText)}`

  const newlyIssuedRewards = referral.rewards.filter(r => r.status === 'Issued')

  return (
    <div className="space-y-6">
      <PageHeader
        title="My Referrals"
        subtitle="Share your code, earn points. Your contacts get $5 off their first order."
      />

      {/* Points alerts */}
      {newlyIssuedRewards.length > 0 && (
        <div className="space-y-3">
          {newlyIssuedRewards.map(r => <PointsAlert key={r.id} reward={r} />)}
        </div>
      )}

      {/* Stats */}
      <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
        <StatCard icon={<Users size={20} />}  label="Total Referrals"  value={referral.totalReferrals}               color="bg-blue-500" />
        <StatCard icon={<Star size={20} />}   label="Points Earned"    value={referral.totalPoints}                  color="bg-yellow-500" />
        <StatCard icon={<Clock size={20} />}  label="Pending Points"   value={referral.pendingPoints}                color="bg-orange-500" />
        <StatCard icon={<Star size={20} />}   label="Points per Refer" value="100 pts"                               color="bg-purple-600" />
      </div>

      {/* Share card */}
      <Card>
        <div className="mb-1 flex items-center gap-2">
          <Share2 size={18} className="text-primary" />
          <h2 className="font-semibold text-slate-800">Your Referral Code</h2>
        </div>
        <p className="mb-5 text-sm text-slate-500">
          Share your code. Your contact gets <strong>$5 off</strong> their first order, and you earn <strong>100 points</strong> once approved.
        </p>

        <div className="mb-5 flex flex-wrap items-center gap-3">
          <div className="rounded-xl border-2 border-primary bg-blue-50 px-6 py-3">
            <span className="text-3xl font-bold tracking-widest text-primary">{referral.code}</span>
          </div>
          <CopyButton text={referral.code} label="Copy Code" size="md" />
        </div>

        {paidInvoices.length === 0 ? (
          <div className="flex items-start gap-3 rounded-xl border border-amber-200 bg-amber-50 p-4">
            <FileText size={18} className="mt-0.5 shrink-0 text-amber-500" />
            <div>
              <p className="font-medium text-amber-800">A paid invoice is required to share your referral</p>
              <p className="mt-0.5 text-sm text-amber-700">
                Once you have at least one paid invoice on your account, you'll be able to share your referral link.
              </p>
            </div>
          </div>
        ) : (
          <>
            <div className="mb-5">
              <label className="mb-1.5 block text-sm font-medium text-slate-700">
                <FileText size={13} className="inline mr-1 text-slate-400" />
                Select an invoice to share
              </label>
              <select
                value={selectedInvoiceId ?? ''}
                onChange={e => setSelectedInvoiceId(e.target.value ? Number(e.target.value) : null)}
                className="w-full rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm text-slate-700 focus:outline-none focus:ring-2 focus:ring-primary/30"
              >
                <option value="">— choose an invoice —</option>
                {paidInvoices.map(inv => (
                  <option key={inv.id} value={inv.id}>
                    {inv.invoiceNumber}{inv.companyName ? ` — ${inv.companyName}` : ''} · ${inv.total.toFixed(2)} · {new Date(inv.paidAt!).toLocaleDateString()}
                  </option>
                ))}
              </select>
            </div>

            {selectedInvoice ? (
              <>
                <div className="mb-5">
                  <label className="mb-1.5 block text-sm font-medium text-slate-700">Referral Link</label>
                  <div className="flex gap-2">
                    <input
                      readOnly
                      value={shareUrl}
                      className="flex-1 rounded-lg border border-slate-200 bg-slate-50 px-3 py-2 text-sm text-slate-600 focus:outline-none"
                    />
                    <CopyButton text={shareUrl} label="Copy Link" size="md" />
                  </div>
                </div>

                <p className="mb-4 text-xs text-slate-500 leading-relaxed bg-slate-50 rounded-lg px-3 py-2 border border-slate-100">
                  <span className="font-medium text-slate-600">Message preview: </span>{shareText}
                </p>

                <div className="flex flex-wrap gap-2">
                  <a href={mailtoHref} className="inline-flex items-center gap-2 rounded-lg bg-primary px-4 py-2 text-sm font-semibold text-white hover:bg-primary-600 transition-colors">
                    <Mail size={15} /> Share via Email
                  </a>
                  <a href={smsHref} className="inline-flex items-center gap-2 rounded-lg bg-green-600 px-4 py-2 text-sm font-semibold text-white hover:bg-green-700 transition-colors">
                    <MessageSquare size={15} /> Share via Text / SMS
                  </a>
                  <CopyButton text={shareText} label="Copy Message" size="md" />
                </div>
              </>
            ) : (
              <p className="text-sm text-slate-400 italic">Select an invoice above to unlock sharing.</p>
            )}
          </>
        )}
      </Card>

      {/* How it works */}
      <Card>
        <h2 className="mb-5 font-semibold text-slate-800">How It Works</h2>
        <div className="grid gap-6 sm:grid-cols-3">
          {[
            { icon: <Share2 size={22} className="text-primary" />,      bg: 'bg-blue-50',   step: '1', title: 'Share Your Code',     body: 'Send your referral link via email or text. Your contact gets $5 off just for signing up.' },
            { icon: <Users size={22} className="text-green-600" />,      bg: 'bg-green-50',  step: '2', title: 'They Sign Up & Save', body: 'Your contact registers with your code and instantly receives a $5 discount coupon.' },
            { icon: <Star size={22} className="text-yellow-600" />,      bg: 'bg-yellow-50', step: '3', title: 'You Earn Points',     body: 'Once approved by our team, 100 points are credited to your account for each successful referral.' },
          ].map(({ icon, bg, step, title, body }) => (
            <div key={step} className="text-center">
              <div className={`mx-auto mb-3 flex h-14 w-14 items-center justify-center rounded-full ${bg}`}>
                {icon}
              </div>
              <h3 className="font-semibold text-slate-800">{step}. {title}</h3>
              <p className="mt-1 text-sm text-slate-500">{body}</p>
            </div>
          ))}
        </div>
      </Card>

      {/* Activity */}
      {referral.recentUses.length > 0 ? (
        <Card>
          <div className="mb-4 flex items-center justify-between">
            <h2 className="font-semibold text-slate-800">Referral Activity</h2>
            <Badge label={`${referral.totalReferrals} total`} color="blue" />
          </div>
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-slate-100 text-xs font-semibold uppercase tracking-wide text-slate-400">
                  <th className="py-2 text-left">Contact</th>
                  <th className="py-2 text-left">Signed Up</th>
                  <th className="py-2 text-left">Their Discount</th>
                  <th className="py-2 text-left">Your Reward</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-50">
                {referral.recentUses.map((use, i) => (
                  <tr key={i} className="text-slate-700">
                    <td className="py-2.5 font-medium">{use.usedByEmail}</td>
                    <td className="py-2.5 text-slate-500">{new Date(use.usedAt).toLocaleDateString()}</td>
                    <td className="py-2.5">
                      {use.welcomeCouponCode
                        ? <code className="rounded bg-green-50 px-1.5 py-0.5 text-xs text-green-700">{use.welcomeCouponCode}</code>
                        : <span className="text-slate-300">—</span>}
                    </td>
                    <td className="py-2.5">
                      {use.hasReward
                        ? <StatusBadge status={use.rewardStatus} />
                        : <Badge label="Processing" color="slate" />}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </Card>
      ) : (
        <Card>
          <div className="py-10 text-center">
            <Share2 size={36} className="mx-auto mb-3 text-slate-300" />
            <h3 className="font-medium text-slate-500">No referrals yet</h3>
            <p className="mt-1 text-sm text-slate-400">
              {selectedInvoice
                ? 'Start sharing — your contacts get $5 off and you earn 100 points!'
                : 'Select an invoice above and start sharing your referral link.'}
            </p>
            {selectedInvoice && (
              <div className="mt-4 flex justify-center gap-2">
                <a href={mailtoHref} className="inline-flex items-center gap-2 rounded-lg bg-primary px-4 py-2 text-sm font-semibold text-white hover:bg-primary-600">
                  <Mail size={14} /> Send Email Invite
                </a>
                <a href={smsHref} className="inline-flex items-center gap-2 rounded-lg bg-green-600 px-4 py-2 text-sm font-semibold text-white hover:bg-green-700">
                  <MessageSquare size={14} /> Send a Text
                </a>
              </div>
            )}
          </div>
        </Card>
      )}

      {/* Points history */}
      {referral.rewards.length > 0 && (
        <Card>
          <h2 className="mb-4 font-semibold text-slate-800">Points History</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-slate-100 text-xs font-semibold uppercase tracking-wide text-slate-400">
                  <th className="py-2 text-left">Points</th>
                  <th className="py-2 text-left">Status</th>
                  <th className="py-2 text-left">Description</th>
                  <th className="py-2 text-left">Earned</th>
                  <th className="py-2 text-left">Issued</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-50">
                {referral.rewards.map(r => (
                  <tr key={r.id} className="text-slate-700">
                    <td className="py-2.5">
                      <span className="inline-flex items-center gap-1 font-semibold text-yellow-600">
                        <Star size={13} /> {r.pointsEarned}
                      </span>
                    </td>
                    <td className="py-2.5"><StatusBadge status={r.status} /></td>
                    <td className="py-2.5 text-slate-500 text-xs">{r.description}</td>
                    <td className="py-2.5 text-slate-500">{new Date(r.createdAt).toLocaleDateString()}</td>
                    <td className="py-2.5 text-slate-500">{r.issuedAt ? new Date(r.issuedAt).toLocaleDateString() : '—'}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </Card>
      )}
    </div>
  )
}
