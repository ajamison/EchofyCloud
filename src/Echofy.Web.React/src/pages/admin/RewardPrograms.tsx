import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { ChevronDown, ChevronRight, Edit, Plus, Trash2, Trophy } from 'lucide-react'
import { api } from '../../lib/api'
import { useAuth } from '../../lib/auth'
import {
  Button, PageHeader, Table, Th, Td, Badge, Modal, FormField, Input, PageSpinner,
} from '../../components/ui'
import type { RewardProgramDto, RewardTierDto, Company } from '../../types'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'

const programSchema = z.object({
  name: z.string().min(1, 'Name is required'),
  companyId: z.coerce.number().nullable().optional(),
  isActive: z.boolean().default(true),
})
type ProgramForm = z.infer<typeof programSchema>

const tierSchema = z.object({
  label: z.string().min(1, 'Label is required'),
  minInvoiceAmount: z.coerce.number().min(0),
  pointsForReferrer: z.coerce.number().int().min(0),
  giftCardAmount: z.coerce.number().min(0),
  isActive: z.boolean().default(true),
  displayOrder: z.coerce.number().int().min(0).default(0),
})
type TierForm = z.infer<typeof tierSchema>

export default function AdminRewardPrograms() {
  const { canWrite, user } = useAuth()
  const qc = useQueryClient()
  const [expandedProgram, setExpandedProgram] = useState<number | null>(null)
  const [editingProgram, setEditingProgram] = useState<RewardProgramDto | null>(null)
  const [showProgramCreate, setShowProgramCreate] = useState(false)
  const [editingTier, setEditingTier] = useState<RewardTierDto | null>(null)
  const [tierProgramId, setTierProgramId] = useState<number | null>(null)

  const clientId = user?.clientId

  const { data: programs = [], isLoading } = useQuery<RewardProgramDto[]>({
    queryKey: ['admin-reward-programs', clientId],
    queryFn: () => api.get('/admin/reward-programs', { params: { clientId } }).then((r) => r.data),
    enabled: !!clientId,
  })

  const { data: companies = [] } = useQuery<Company[]>({
    queryKey: ['admin-companies'],
    queryFn: () => api.get('/admin/companies').then((r) => r.data),
  })

  const clientCompanies = companies.filter((c) => c.clientId === clientId)

  const programForm = useForm<ProgramForm>({ resolver: zodResolver(programSchema), defaultValues: { isActive: true } })
  const tierForm = useForm<TierForm>({ resolver: zodResolver(tierSchema), defaultValues: { isActive: true, displayOrder: 0 } })

  const saveProgramMutation = useMutation({
    mutationFn: (data: ProgramForm) => {
      const payload = {
        clientId: clientId!,
        companyId: data.companyId || null,
        name: data.name,
        isActive: data.isActive,
      }
      return editingProgram
        ? api.put(`/admin/reward-programs/${editingProgram.id}`, { name: payload.name, isActive: payload.isActive })
        : api.post('/admin/reward-programs', payload)
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['admin-reward-programs'] })
      setEditingProgram(null)
      setShowProgramCreate(false)
      programForm.reset({ isActive: true })
    },
  })

  const deleteProgramMutation = useMutation({
    mutationFn: (id: number) => api.delete(`/admin/reward-programs/${id}`),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['admin-reward-programs'] }),
  })

  const saveTierMutation = useMutation({
    mutationFn: (data: TierForm) => {
      if (editingTier) {
        return api.put(`/admin/reward-programs/${tierProgramId}/tiers/${editingTier.id}`, data)
      }
      return api.post(`/admin/reward-programs/${tierProgramId}/tiers`, data)
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['admin-reward-programs'] })
      setEditingTier(null)
      setTierProgramId(null)
      tierForm.reset({ isActive: true, displayOrder: 0 })
    },
  })

  const deleteTierMutation = useMutation({
    mutationFn: ({ programId, tierId }: { programId: number; tierId: number }) =>
      api.delete(`/admin/reward-programs/${programId}/tiers/${tierId}`),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['admin-reward-programs'] }),
  })

  const openEditProgram = (p: RewardProgramDto) => {
    setEditingProgram(p)
    programForm.reset({ name: p.name, companyId: p.companyId ?? undefined, isActive: p.isActive })
  }

  const openCreateProgram = () => {
    setEditingProgram(null)
    setShowProgramCreate(true)
    programForm.reset({ isActive: true })
  }

  const openAddTier = (programId: number) => {
    setTierProgramId(programId)
    setEditingTier(null)
    tierForm.reset({ isActive: true, displayOrder: 0 })
  }

  const openEditTier = (tier: RewardTierDto) => {
    setTierProgramId(tier.rewardProgramId)
    setEditingTier(tier)
    tierForm.reset({
      label: tier.label,
      minInvoiceAmount: tier.minInvoiceAmount,
      pointsForReferrer: tier.pointsForReferrer,
      giftCardAmount: tier.giftCardAmount,
      isActive: tier.isActive,
      displayOrder: tier.displayOrder,
    })
  }

  if (isLoading) return <PageSpinner />

  const isProgramModalOpen = !!(editingProgram || showProgramCreate)
  const isTierModalOpen = !!(tierProgramId !== null && (editingTier !== null || tierForm.formState.isDirty || tierProgramId !== null))

  return (
    <div>
      <PageHeader
        title="Reward Programs"
        subtitle="Configure tiered rewards for invoice payments"
        actions={canWrite() && <Button onClick={openCreateProgram}><Plus size={15} /> New Program</Button>}
      />

      {programs.length === 0 ? (
        <div className="rounded-xl border border-dashed border-slate-300 py-16 text-center text-slate-400 dark:border-slate-600">
          <Trophy size={32} className="mx-auto mb-3 opacity-40" />
          <p className="font-medium">No reward programs yet</p>
          <p className="text-sm mt-1">Create a program and add tiers to reward customers and referrers.</p>
        </div>
      ) : (
        <div className="space-y-4">
          {programs.map((program) => (
            <div key={program.id} className="rounded-xl border border-slate-200 bg-white shadow-sm dark:border-slate-700 dark:bg-slate-800">
              {/* Program header */}
              <div className="flex items-center gap-3 px-4 py-3">
                <button
                  onClick={() => setExpandedProgram(expandedProgram === program.id ? null : program.id)}
                  className="text-slate-400 hover:text-slate-600"
                >
                  {expandedProgram === program.id ? <ChevronDown size={16} /> : <ChevronRight size={16} />}
                </button>
                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-2 flex-wrap">
                    <span className="font-semibold text-slate-800 dark:text-slate-100">{program.name}</span>
                    <Badge
                      label={program.companyId ? `Company: ${program.companyName ?? '?'}` : 'Client Default'}
                      color={program.companyId ? 'blue' : 'purple'}
                    />
                    <Badge label={program.isActive ? 'Active' : 'Inactive'} color={program.isActive ? 'green' : 'slate'} />
                  </div>
                  <p className="text-xs text-slate-400 mt-0.5">{program.tiers.length} tier{program.tiers.length !== 1 ? 's' : ''}</p>
                </div>
                {canWrite() && (
                  <div className="flex items-center gap-1 shrink-0">
                    <Button variant="ghost" size="sm" onClick={() => openEditProgram(program)}><Edit size={14} /></Button>
                    <Button variant="ghost" size="sm" onClick={() => deleteProgramMutation.mutate(program.id)}>
                      <Trash2 size={14} className="text-red-500" />
                    </Button>
                  </div>
                )}
              </div>

              {/* Tiers */}
              {expandedProgram === program.id && (
                <div className="border-t border-slate-100 dark:border-slate-700 px-4 pb-4 pt-3">
                  <div className="flex items-center justify-between mb-3">
                    <p className="text-xs font-semibold uppercase tracking-wide text-slate-400">Tiers</p>
                    {canWrite() && (
                      <Button variant="ghost" size="sm" onClick={() => openAddTier(program.id)}>
                        <Plus size={13} /> Add Tier
                      </Button>
                    )}
                  </div>
                  {program.tiers.length === 0 ? (
                    <p className="text-sm text-slate-400 italic">No tiers configured.</p>
                  ) : (
                    <Table>
                      <thead>
                        <tr>
                          <Th>Label</Th>
                          <Th>Min Invoice</Th>
                          <Th>Points (Referrer)</Th>
                          <Th>Gift Card ($)</Th>
                          <Th>Status</Th>
                          {canWrite() && <Th className="text-right">Actions</Th>}
                        </tr>
                      </thead>
                      <tbody>
                        {program.tiers.map((tier) => (
                          <tr key={tier.id} className="hover:bg-slate-50 dark:hover:bg-slate-700/50">
                            <Td className="font-medium">{tier.label}</Td>
                            <Td>${tier.minInvoiceAmount.toFixed(2)}+</Td>
                            <Td className="text-blue-600 font-semibold">{tier.pointsForReferrer} pts</Td>
                            <Td className="text-green-600 font-semibold">
                              {tier.giftCardAmount > 0 ? `$${tier.giftCardAmount.toFixed(2)}` : '—'}
                            </Td>
                            <Td>
                              <Badge label={tier.isActive ? 'Active' : 'Inactive'} color={tier.isActive ? 'green' : 'slate'} />
                            </Td>
                            {canWrite() && (
                              <Td className="text-right">
                                <div className="flex justify-end gap-1">
                                  <Button variant="ghost" size="sm" onClick={() => openEditTier(tier)}><Edit size={13} /></Button>
                                  <Button
                                    variant="ghost" size="sm"
                                    onClick={() => deleteTierMutation.mutate({ programId: program.id, tierId: tier.id })}
                                  >
                                    <Trash2 size={13} className="text-red-500" />
                                  </Button>
                                </div>
                              </Td>
                            )}
                          </tr>
                        ))}
                      </tbody>
                    </Table>
                  )}
                </div>
              )}
            </div>
          ))}
        </div>
      )}

      {/* Program create/edit modal */}
      <Modal
        open={isProgramModalOpen}
        onClose={() => { setEditingProgram(null); setShowProgramCreate(false) }}
        title={editingProgram ? 'Edit Program' : 'New Reward Program'}
      >
        <form onSubmit={programForm.handleSubmit((d) => saveProgramMutation.mutate(d))} className="space-y-4">
          <FormField label="Name" error={programForm.formState.errors.name?.message} required>
            <Input {...programForm.register('name')} />
          </FormField>
          {!editingProgram && (
            <FormField label="Company Override (leave empty for client default)">
              <select
                {...programForm.register('companyId')}
                className="w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm dark:border-slate-600 dark:bg-slate-800"
              >
                <option value="">— Client Default —</option>
                {clientCompanies.map((c) => (
                  <option key={c.id} value={c.id}>{c.name}</option>
                ))}
              </select>
            </FormField>
          )}
          <label className="flex items-center gap-2 text-sm">
            <input type="checkbox" {...programForm.register('isActive')} /> Active
          </label>
          <div className="flex justify-end gap-2 pt-2">
            <Button variant="secondary" type="button" onClick={() => { setEditingProgram(null); setShowProgramCreate(false) }}>
              Cancel
            </Button>
            <Button type="submit" loading={saveProgramMutation.isPending}>Save</Button>
          </div>
        </form>
      </Modal>

      {/* Tier create/edit modal */}
      <Modal
        open={!!tierProgramId && !isProgramModalOpen}
        onClose={() => { setTierProgramId(null); setEditingTier(null); tierForm.reset() }}
        title={editingTier ? 'Edit Tier' : 'Add Tier'}
      >
        <form onSubmit={tierForm.handleSubmit((d) => saveTierMutation.mutate(d))} className="space-y-4">
          <FormField label="Label" error={tierForm.formState.errors.label?.message} required>
            <Input {...tierForm.register('label')} placeholder="e.g. Silver, Gold, Premium" />
          </FormField>
          <div className="grid grid-cols-2 gap-4">
            <FormField label="Min Invoice Amount ($)" error={tierForm.formState.errors.minInvoiceAmount?.message} required>
              <Input type="number" step="0.01" min="0" {...tierForm.register('minInvoiceAmount')} />
            </FormField>
            <FormField label="Display Order">
              <Input type="number" min="0" {...tierForm.register('displayOrder')} />
            </FormField>
          </div>
          <div className="grid grid-cols-2 gap-4">
            <FormField label="Points for Referrer" error={tierForm.formState.errors.pointsForReferrer?.message} required>
              <Input type="number" min="0" {...tierForm.register('pointsForReferrer')} />
            </FormField>
            <FormField label="Gift Card Amount ($)">
              <Input type="number" step="0.01" min="0" {...tierForm.register('giftCardAmount')} />
            </FormField>
          </div>
          <label className="flex items-center gap-2 text-sm">
            <input type="checkbox" {...tierForm.register('isActive')} /> Active
          </label>
          <div className="flex justify-end gap-2 pt-2">
            <Button variant="secondary" type="button" onClick={() => { setTierProgramId(null); setEditingTier(null); tierForm.reset() }}>
              Cancel
            </Button>
            <Button type="submit" loading={saveTierMutation.isPending}>Save Tier</Button>
          </div>
        </form>
      </Modal>
    </div>
  )
}
