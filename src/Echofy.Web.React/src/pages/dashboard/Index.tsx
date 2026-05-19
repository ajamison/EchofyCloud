import { useQuery } from '@tanstack/react-query'
import { Users, Package } from 'lucide-react'
import { api } from '../../lib/api'
import { PageSpinner, PageHeader, StatCard, Card } from '../../components/ui'
import type { DashboardStats } from '../../types'
import { useAuth } from '../../lib/auth'

export default function Dashboard() {
  const { user } = useAuth()
  const { data: stats, isLoading } = useQuery<DashboardStats>({
    queryKey: ['dashboard-stats'],
    queryFn: () => api.get('/dashboard/stats').then((r) => r.data),
  })

  if (isLoading) return <PageSpinner />

  return (
    <div>
      <PageHeader
        title={`Welcome back, ${user?.fullName?.split(' ')[0]}!`}
        subtitle="Here's what's happening today."
      />

      {stats && (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-2">
          <StatCard label="New Customers" value={stats.newCustomers} icon={<Users size={22} />} color="bg-violet-500" />
          <StatCard label="Out of Stock" value={stats.outOfStockProducts} icon={<Package size={22} />} color="bg-amber-500" />
        </div>
      )}

      <div className="mt-6 grid grid-cols-1 gap-6 lg:grid-cols-2">
        <Card>
          <h3 className="mb-4 font-semibold text-slate-700">Discount Distribution</h3>
          <div className="space-y-3">
            <div className="flex justify-between text-sm">
              <span className="text-slate-500">Percentage discounts</span>
              <span className="font-medium">{stats?.percentageDiscountShare.toFixed(1) ?? 0}%</span>
            </div>
            <div className="flex justify-between text-sm">
              <span className="text-slate-500">Fixed amount discounts</span>
              <span className="font-medium">{stats?.fixedProductDiscountShare.toFixed(1) ?? 0}%</span>
            </div>
          </div>
        </Card>
      </div>
    </div>
  )
}
