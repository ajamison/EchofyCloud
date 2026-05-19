import { useQuery } from '@tanstack/react-query'
import { Plus } from 'lucide-react'
import { api } from '../../../lib/api'
import { useAuth } from '../../../lib/auth'
import { Button, PageHeader, Card, Badge, PageSpinner } from '../../../components/ui'
import type { Deal } from '../../../types'
import { DealStage } from '../../../types'

const stageColors: Record<number, 'blue' | 'yellow' | 'green' | 'red' | 'slate'> = {
  [DealStage.Prospecting]: 'blue', [DealStage.Proposal]: 'yellow',
  [DealStage.Negotiation]: 'slate', [DealStage.ClosedWon]: 'green', [DealStage.ClosedLost]: 'red',
}

const stageOrder = [DealStage.Prospecting, DealStage.Proposal, DealStage.Negotiation, DealStage.ClosedWon, DealStage.ClosedLost]

export default function Deals() {
  const { canWrite } = useAuth()
  const { data: deals = [], isLoading } = useQuery<Deal[]>({
    queryKey: ['deals'],
    queryFn: () => api.get('/deals').then((r) => r.data),
  })

  if (isLoading) return <PageSpinner />

  return (
    <div>
      <PageHeader title="Deals" subtitle={`${deals.length} deals`} actions={canWrite() && <Button><Plus size={15} /> Add Deal</Button>} />
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-3 xl:grid-cols-5">
        {stageOrder.map((stage) => {
          const stageDeals = deals.filter((d) => d.stage === stage)
          const total = stageDeals.reduce((sum, d) => sum + d.value, 0)
          return (
            <div key={stage}>
              <div className="mb-3 flex items-center justify-between">
                <Badge label={DealStage[stage]} color={stageColors[stage]} />
                <span className="text-xs text-slate-400">{stageDeals.length}</span>
              </div>
              <div className="space-y-2">
                {stageDeals.map((deal) => (
                  <Card key={deal.id} className="cursor-pointer p-4 hover:shadow-md transition-shadow">
                    <p className="font-medium text-sm text-slate-800">{deal.title}</p>
                    <p className="mt-1 text-xs text-slate-400">{deal.leadName}</p>
                    <p className="mt-2 text-sm font-semibold text-primary">${deal.value.toLocaleString()}</p>
                    {deal.expectedCloseDate && (
                      <p className="mt-1 text-xs text-slate-400">Close: {new Date(deal.expectedCloseDate).toLocaleDateString()}</p>
                    )}
                  </Card>
                ))}
                <div className="text-right text-xs font-medium text-slate-500 pt-1">${total.toLocaleString()}</div>
              </div>
            </div>
          )
        })}
      </div>
    </div>
  )
}
