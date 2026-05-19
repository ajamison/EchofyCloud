import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { Share2, Star } from 'lucide-react'
import { useAuth } from '../../lib/auth'
import { PageHeader, Card } from '../../components/ui'
import { api } from '../../lib/api'
import type { ReferralDto } from '../../types'

export default function CustomerDashboard() {
  const { user } = useAuth()

  const { data: referral } = useQuery<ReferralDto>({
    queryKey: ['me', 'referral'],
    queryFn: () => api.get('/me/referral').then(r => r.data),
  })

  const totalPoints = referral?.totalPoints ?? 0

  return (
    <div className="space-y-6">
      <PageHeader title={`Welcome, ${user?.fullName}!`} subtitle="Manage your account and favorites." />

      {/* Points banner */}
      {totalPoints > 0 && (
        <div className="flex items-center gap-3 rounded-xl border border-yellow-200 bg-yellow-50 px-4 py-3">
          <Star size={18} className="shrink-0 text-yellow-600" />
          <p className="text-sm text-yellow-800">
            You have <strong>{totalPoints}</strong> reward point{totalPoints !== 1 ? 's' : ''}!{' '}
            <Link to="/customer/referrals" className="font-semibold underline">View referrals</Link>
          </p>
        </div>
      )}

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Card>
          <h3 className="font-semibold text-slate-700">My Account</h3>
          <p className="mt-2 text-sm text-slate-500">{user?.email}</p>
        </Card>

        {/* Referral teaser */}
        <Link to="/customer/referrals" className="block">
          <Card className="h-full transition-shadow hover:shadow-md">
            <div className="flex items-start gap-3">
              <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-blue-50">
                <Share2 size={18} className="text-primary" />
              </div>
              <div>
                <h3 className="font-semibold text-slate-700">My Referrals</h3>
                {referral ? (
                  <p className="mt-1 text-sm text-slate-500">
                    {referral.totalReferrals > 0
                      ? `${referral.totalReferrals} referral${referral.totalReferrals > 1 ? 's' : ''} · ${referral.totalPoints} points earned`
                      : 'Share your code and earn reward points!'}
                  </p>
                ) : (
                  <p className="mt-1 text-sm text-slate-500">Share your code and earn reward points!</p>
                )}
                <p className="mt-1 text-xs font-medium text-primary">Your code: {referral?.code ?? '…'}</p>
              </div>
            </div>
          </Card>
        </Link>
      </div>
    </div>
  )
}
