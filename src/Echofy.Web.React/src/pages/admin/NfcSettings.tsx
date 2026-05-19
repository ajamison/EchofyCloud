import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { Eye, EyeOff, KeyRound } from 'lucide-react'
import { api } from '../../lib/api'
import { useAuth } from '../../lib/auth'
import { Button, PageHeader, Table, Th, Td, Badge, Modal, FormField, Input, PageSpinner } from '../../components/ui'
import type { NfcClientSetting, NfcClientSettingListItem } from '../../types'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'

const schema = z.object({
  password: z.union([
    z.string().regex(/^[0-9A-Fa-f]{8}$/, 'Must be exactly 8 hex characters (e.g. AABBCCDD)'),
    z.literal(''),
  ]),
})
type FormData = z.infer<typeof schema>

export default function AdminNfcSettings() {
  const { user } = useAuth()
  const isSuperAdmin = user?.role === 'SuperAdmin'
  const qc = useQueryClient()

  const [selectedClientId, setSelectedClientId] = useState<number | null>(null)
  const [showPassword, setShowPassword] = useState(false)

  const { data: clients = [], isLoading } = useQuery<NfcClientSettingListItem[]>({
    queryKey: ['nfc-settings'],
    queryFn: () => api.get('/admin/nfc-settings').then((r) => r.data),
  })

  const { data: detail, isFetching: loadingDetail } = useQuery<NfcClientSetting>({
    queryKey: ['nfc-settings', selectedClientId],
    queryFn: () => api.get(`/admin/nfc-settings/${selectedClientId}`).then((r) => r.data),
    enabled: selectedClientId !== null,
  })

  const { register, handleSubmit, reset, setError, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { password: '' },
  })

  const mutation = useMutation({
    mutationFn: ({ clientId, password }: { clientId: number; password: string | null }) =>
      api.put(`/admin/nfc-settings/${clientId}`, { password }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['nfc-settings'] })
      closeModal()
    },
    onError: (err: any) => {
      setError('password', { message: err.response?.data ?? 'Failed to save.' })
    },
  })

  function openModal(clientId: number) {
    setSelectedClientId(clientId)
    setShowPassword(false)
    reset({ password: '' })
  }

  function closeModal() {
    setSelectedClientId(null)
    setShowPassword(false)
    reset({ password: '' })
  }

  function onSubmit(data: FormData) {
    if (!selectedClientId) return
    mutation.mutate({ clientId: selectedClientId, password: data.password || null })
  }

  if (isLoading) return <PageSpinner />

  return (
    <div>
      <PageHeader
        title="NFC Card Settings"
        subtitle="Configure the password used to lock NFC cards. SuperUser can view; only SuperAdmin can change."
        icon={<KeyRound size={20} />}
      />

      <Table>
        <thead>
          <tr>
            <Th>Client</Th>
            <Th>NFC Lock Password</Th>
            <Th></Th>
          </tr>
        </thead>
        <tbody>
          {clients.map((c) => (
            <tr key={c.id}>
              <Td className="font-medium">{c.name}</Td>
              <Td>
                {c.hasPassword ? (
                  <Badge variant="success">Configured</Badge>
                ) : (
                  <Badge variant="secondary">Not set</Badge>
                )}
              </Td>
              <Td>
                <Button size="sm" variant="ghost" onClick={() => openModal(c.id)}>
                  {isSuperAdmin ? 'Manage' : 'View'}
                </Button>
              </Td>
            </tr>
          ))}
          {clients.length === 0 && (
            <tr>
              <Td colSpan={3} className="text-center text-slate-400 py-8">No clients found.</Td>
            </tr>
          )}
        </tbody>
      </Table>

      <Modal
        open={selectedClientId !== null}
        onClose={closeModal}
        title={`NFC Lock Password — ${detail?.clientName ?? '…'}`}
      >
        {loadingDetail ? (
          <div className="py-6 text-center text-slate-400 text-sm">Loading…</div>
        ) : (
          <>
            {/* Current value (visible to both roles) */}
            <div className="mb-6">
              <div className="text-xs font-medium uppercase tracking-wide text-slate-500 mb-1">
                Current password
              </div>
              {detail?.password ? (
                <div className="flex items-center gap-2">
                  <span className="font-mono text-sm tracking-widest">
                    {showPassword ? detail.password : '••••••••'}
                  </span>
                  <button
                    type="button"
                    className="text-slate-400 hover:text-slate-600"
                    onClick={() => setShowPassword((v) => !v)}
                    title={showPassword ? 'Hide' : 'Show'}
                  >
                    {showPassword ? <EyeOff size={14} /> : <Eye size={14} />}
                  </button>
                </div>
              ) : (
                <span className="text-sm text-slate-400 italic">Not configured</span>
              )}
            </div>

            {/* Edit form — SuperAdmin only */}
            {isSuperAdmin ? (
              <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
                <FormField
                  label="New password (8 hex characters)"
                  error={errors.password?.message}
                  hint="Leave blank to clear the password. The NFC app will use this value when locking cards."
                >
                  <Input
                    {...register('password')}
                    placeholder="e.g. AABBCCDD"
                    maxLength={8}
                    className="font-mono uppercase"
                    autoComplete="off"
                  />
                </FormField>

                <div className="flex items-center justify-between pt-2">
                  {detail?.password && (
                    <Button
                      type="button"
                      variant="danger-ghost"
                      size="sm"
                      onClick={() => mutation.mutate({ clientId: selectedClientId!, password: null })}
                      loading={mutation.isPending}
                    >
                      Clear password
                    </Button>
                  )}
                  <div className="flex gap-2 ml-auto">
                    <Button type="button" variant="ghost" onClick={closeModal}>Cancel</Button>
                    <Button type="submit" loading={mutation.isPending}>Save</Button>
                  </div>
                </div>
              </form>
            ) : (
              <div className="flex justify-end pt-2">
                <Button variant="ghost" onClick={closeModal}>Close</Button>
              </div>
            )}
          </>
        )}
      </Modal>
    </div>
  )
}
