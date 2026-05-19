import { useQuery } from '@tanstack/react-query'
import { Target, TrendingUp, DollarSign, Award } from 'lucide-react'
import { api } from '../../lib/api'
import { PageHeader, StatCard, Card, PageSpinner } from '../../components/ui'
import type { CrmAnalytics } from '../../types'
import { DealStage } from '../../types'

export default function CrmDashboard() {
  const { data: analytics, isLoading } = useQuery<CrmAnalytics>({
    queryKey: ['crm-analytics'],
    queryFn: () => api.get('/deals/analytics').then((r) => r.data),
  })

  if (isLoading) return <PageSpinner />

  return (
    <div>
      <PageHeader title="CRM Dashboard" />
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <StatCard label="Total Deals" value={analytics?.totalDeals ?? 0} icon={<Target size={22} />} color="bg-primary" />
        <StatCard label="Total Value" value={`$${(analytics?.totalValue ?? 0).toLocaleString()}`} icon={<DollarSign size={22} />} color="bg-emerald-500" />
        <StatCard label="Won Deals" value={analytics?.wonDeals ?? 0} icon={<Award size={22} />} color="bg-violet-500" />
        <StatCard label="Won Value" value={`$${(analytics?.wonValue ?? 0).toLocaleString()}`} icon={<TrendingUp size={22} />} color="bg-amber-500" />
      </div>
      <div className="mt-6">
        <Card>
          <h3 className="mb-4 font-semibold text-slate-700">Pipeline by Stage</h3>
          <div className="space-y-3">
            {Object.values(DealStage).filter((v) => typeof v === 'string').map((stageName) => {
              const key = stageName as string
              const count = analytics?.byStage?.[key] ?? 0
              return (
                <div key={key} className="flex items-center gap-3">
                  <span className="w-28 text-sm text-slate-500">{key}</span>
                  <div className="flex-1 rounded-full bg-slate-100 h-2">
                    <div
                      className="h-2 rounded-full bg-primary"
                      style={{ width: `${analytics?.totalDeals ? (count / analytics.totalDeals) * 100 : 0}%` }}
                    />
                  </div>
                  <span className="w-8 text-right text-sm font-medium text-slate-700">{count}</span>
                </div>
              )
            })}
          </div>
        </Card>
      </div>
    </div>
  )
}
