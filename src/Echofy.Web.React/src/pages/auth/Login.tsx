import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useNavigate, Link } from 'react-router-dom'
import { useAuth } from '../../lib/auth'
import { Button, FormField, Input } from '../../components/ui'
import { useState } from 'react'

const schema = z.object({
  email: z.string().email('Invalid email'),
  password: z.string().min(1, 'Password is required'),
})
type FormData = z.infer<typeof schema>

export default function Login() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const [error, setError] = useState('')

  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<FormData>({
    resolver: zodResolver(schema),
  })

  const onSubmit = async (data: FormData) => {
    setError('')
    try {
      await login(data.email, data.password)
      navigate('/dashboard', { replace: true })
    } catch {
      setError('Invalid email or password.')
    }
  }

  return (
    <div>
      <h2 className="mb-6 text-xl font-semibold text-slate-800">Sign in to your account</h2>
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        <FormField label="Email" error={errors.email?.message} required>
          <Input type="email" placeholder="you@example.com" {...register('email')} />
        </FormField>
        <FormField label="Password" error={errors.password?.message} required>
          <Input type="password" placeholder="••••••••" {...register('password')} />
        </FormField>
        {error && <p className="rounded-lg bg-red-50 px-3 py-2 text-sm text-red-600">{error}</p>}
        <Button type="submit" loading={isSubmitting} className="w-full">Sign in</Button>
      </form>
      <p className="mt-4 text-center text-sm text-slate-500">
        Don't have an account?{' '}
        <Link to="/register" className="font-medium text-primary hover:underline">Register</Link>
      </p>
    </div>
  )
}
