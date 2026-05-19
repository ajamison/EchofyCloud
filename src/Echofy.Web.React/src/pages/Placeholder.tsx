import { PageHeader, Card } from '../components/ui'

interface Props { title: string; description?: string }

export default function Placeholder({ title, description }: Props) {
  return (
    <div>
      <PageHeader title={title} />
      <Card className="flex h-64 items-center justify-center">
        <p className="text-slate-400">{description ?? `${title} — coming soon`}</p>
      </Card>
    </div>
  )
}
