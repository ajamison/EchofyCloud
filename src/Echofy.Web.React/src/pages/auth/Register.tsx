import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import { Button, FormField, Input } from '../../components/ui'
import { useState } from 'react'
import { api } from '../../lib/api'
import { useAuth } from '../../lib/auth'
import { Gift } from 'lucide-react'

const schema = z.object({
  fullName:     z.string().min(2, 'Name is required'),
  email:        z.string().email('Invalid email'),
  password:     z.string().min(6, 'Minimum 6 characters'),
  referralCode: z.string().optional(),
})
type FormData = z.infer<typeof schema>

export default function Register() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const [error, setError] = useState('')
  const [welcomeCoupon, setWelcomeCoupon] = useState<string | null>(null)

  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { referralCode: searchParams.get('ref') ?? '' },
  })

  const onSubmit = async (data: FormData) => {
    setError('')
    try {
      const res = await api.post('/auth/register', data)
      const coupon: string | null = res.data?.welcomeCoupon ?? null
      if (coupon) {
        setWelcomeCoupon(coupon)
        await login(data.email, data.password)
      } else {
        await login(data.email, data.password)
        navigate('/', { replace: true })
      }
    } catch (e: any) {
      setError(e.response?.data?.message ?? 'Registration failed.')
    }
  }

  if (welcomeCoupon) {
    return (
      <div>
        <div className="mb-6 rounded-xl border border-green-200 bg-green-50 p-5 text-center">
          <Gift size={32} className="mx-auto mb-2 text-green-600" />
          <h2 className="text-lg font-semibold text-green-800">Welcome gift unlocked!</h2>
          <p className="mt-1 text-sm text-green-700">Use this code at checkout to save $5 on your first order:</p>
          <code className="mt-3 inline-block rounded-lg border border-green-300 bg-white px-4 py-2 text-xl font-bold tracking-widest text-green-700">
            {welcomeCoupon}
          </code>
        </div>
        <Button className="w-full" onClick={() => navigate('/', { replace: true })}>
          Continue to your account
        </Button>
      </div>
    )
  }

  return (
    <div>
      <h2 className="mb-6 text-xl font-semibold text-slate-800">Create an account</h2>
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        <FormField label="Full Name" error={errors.fullName?.message} required>
          <Input placeholder="John Smith" {...register('fullName')} />
        </FormField>
        <FormField label="Email" error={errors.email?.message} required>
          <Input type="email" placeholder="you@example.com" {...register('email')} />
        </FormField>
        <FormField label="Password" error={errors.password?.message} required>
          <Input type="password" placeholder="Min. 6 characters" {...register('password')} />
        </FormField>
        <FormField label="Referral Code" error={errors.referralCode?.message}>
          <Input placeholder="Optional — get $5 off!" {...register('referralCode')} />
        </FormField>
        {error && <p className="rounded-lg bg-red-50 px-3 py-2 text-sm text-red-600">{error}</p>}
        <Button type="submit" loading={isSubmitting} className="w-full">Create account</Button>
      </form>
      <p className="mt-4 text-center text-sm text-slate-500">
        Already have an account?{' '}
        <Link to="/login" className="font-medium text-primary hover:underline">Sign in</Link>
      </p>
    </div>
  )
}
