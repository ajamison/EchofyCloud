import { Link } from 'react-router-dom'
import {
  QrCode,
  Share2,
  BarChart3,
  CheckCircle2,
  ArrowRight,
  Zap,
  Users,
  TrendingUp,
  Shield,
  Package,
  Link2,
} from 'lucide-react'

// ── Hero ──────────────────────────────────────────────────────────────────────

function Hero() {
  return (
    <section className="relative overflow-hidden bg-gradient-to-br from-primary-50 via-white to-blue-50 py-24 md:py-32">
      <div className="pointer-events-none absolute inset-0">
        <div className="absolute -left-32 -top-32 h-96 w-96 rounded-full bg-primary/5 blur-3xl" />
        <div className="absolute -bottom-16 right-0 h-80 w-80 rounded-full bg-blue-200/30 blur-3xl" />
      </div>
      <div className="relative mx-auto max-w-6xl px-6 text-center">
        <span className="mb-4 inline-flex items-center gap-2 rounded-full bg-primary/10 px-4 py-1.5 text-sm font-semibold text-primary">
          <Zap size={14} />
          B2B Product Intelligence &amp; Referral Platform
        </span>
        <h1 className="mx-auto mt-4 max-w-4xl text-4xl font-extrabold leading-tight tracking-tight text-slate-900 md:text-6xl">
          Turn Every Product Scan<br />
          <span className="text-primary">Into a Growth Engine</span>
        </h1>
        <p className="mx-auto mt-6 max-w-2xl text-lg text-slate-600 md:text-xl">
          Echofy gives your business a complete QR &amp; NFC product experience platform and
          built-in referral engine — capturing leads, building loyalty, and growing your revenue
          through your customer network.
        </p>
        <div className="mt-10 flex flex-col items-center justify-center gap-4 sm:flex-row">
          <Link
            to="/register"
            className="flex items-center gap-2 rounded-xl bg-primary px-8 py-4 text-base font-semibold text-white shadow-lg shadow-primary/30 hover:bg-primary-600 transition-all hover:-translate-y-0.5"
          >
            Start for Free <ArrowRight size={18} />
          </Link>
          <a
            href="#how-it-works"
            className="flex items-center gap-2 rounded-xl border border-slate-200 bg-white px-8 py-4 text-base font-semibold text-slate-700 hover:border-primary/40 hover:text-primary transition-all"
          >
            See How It Works
          </a>
        </div>
        <p className="mt-4 text-sm text-slate-400">No credit card required &middot; Free 30-day trial</p>
      </div>
    </section>
  )
}

// ── Stats ─────────────────────────────────────────────────────────────────────

function Stats() {
  const items = [
    { value: '500+', label: 'Businesses Onboarded' },
    { value: '12M+', label: 'QR &amp; NFC Scans' },
    { value: '34%', label: 'Average Conversion Uplift' },
    { value: '99.9%', label: 'Platform Uptime' },
  ]
  return (
    <section className="border-y border-slate-100 bg-slate-50 py-14">
      <div className="mx-auto max-w-6xl px-6">
        <div className="grid grid-cols-2 gap-8 text-center md:grid-cols-4">
          {items.map((s) => (
            <div key={s.label}>
              <p className="text-3xl font-extrabold text-primary" dangerouslySetInnerHTML={{ __html: s.value }} />
              <p className="mt-1 text-sm text-slate-500" dangerouslySetInnerHTML={{ __html: s.label }} />
            </div>
          ))}
        </div>
      </div>
    </section>
  )
}

// ── Features ──────────────────────────────────────────────────────────────────

function Features() {
  const features = [
    {
      icon: <QrCode className="text-primary" size={28} />,
      title: 'QR & NFC Product Scanning',
      description:
        'Generate unique QR codes and NFC tags for every product. When customers scan, they instantly see full product details, pricing, availability, and reviews — right on their device.',
      bullets: [
        'Branded product landing pages',
        'Real-time inventory &amp; pricing',
        'Collect reviews on scan',
      ],
    },
    {
      icon: <Share2 className="text-primary" size={28} />,
      title: 'Built-In Referral Engine',
      description:
        'Transform every satisfied customer into a brand ambassador. Issue unique referral links, track conversions end-to-end, and automatically reward successful referrals.',
      bullets: [
        'Trackable referral links per customer',
        'Configurable rewards &amp; commissions',
        'Multi-tier referral programs',
      ],
    },
    {
      icon: <BarChart3 className="text-primary" size={28} />,
      title: 'B2B Analytics Dashboard',
      description:
        'Monitor every scan, referral, and conversion from a single dashboard. Understand which products perform best and where your referral revenue is coming from.',
      bullets: [
        'Scan heatmaps &amp; trends',
        'Referral attribution reports',
        'Export-ready data',
      ],
    },
    {
      icon: <Users className="text-primary" size={28} />,
      title: 'Multi-Tenant CRM',
      description:
        'Manage your customer relationships alongside your product catalog. Link contacts, deals, and leads directly to scan activity and referrals for a complete picture.',
      bullets: [
        'Full CRM with leads &amp; deals',
        'Customer scan history',
        'Role-based access control',
      ],
    },
    {
      icon: <Package className="text-primary" size={28} />,
      title: 'Product Intelligence',
      description:
        'Maintain rich product profiles with price history, discount offers, manufacturer data, and units of measure — keeping your catalog accurate and audit-ready.',
      bullets: [
        'Automatic price history tracking',
        'Configurable discount offers',
        'Manufacturer &amp; UoM management',
      ],
    },
    {
      icon: <Shield className="text-primary" size={28} />,
      title: 'Enterprise Security',
      description:
        'Built on ASP.NET Core with row-level multi-tenancy, role-based permissions, and full audit logging — so your data stays yours and your compliance stays intact.',
      bullets: [
        'Tenant-isolated data model',
        'Full audit log trail',
        'SuperAdmin cross-tenant oversight',
      ],
    },
  ]

  return (
    <section id="features" className="py-24">
      <div className="mx-auto max-w-6xl px-6">
        <div className="text-center">
          <h2 className="text-3xl font-extrabold text-slate-900 md:text-4xl">
            Everything your business needs<br />to grow from product to sale
          </h2>
          <p className="mx-auto mt-4 max-w-xl text-slate-500">
            One platform. QR product intelligence, referral marketing, and CRM — all connected.
          </p>
        </div>
        <div className="mt-16 grid gap-8 sm:grid-cols-2 lg:grid-cols-3">
          {features.map((f) => (
            <div
              key={f.title}
              className="group rounded-2xl border border-slate-100 bg-white p-8 shadow-sm transition-all hover:border-primary/30 hover:shadow-md"
            >
              <div className="mb-4 flex h-12 w-12 items-center justify-center rounded-xl bg-primary/10">
                {f.icon}
              </div>
              <h3 className="mb-2 text-lg font-bold text-slate-900">{f.title}</h3>
              <p className="mb-4 text-sm text-slate-500 leading-relaxed">{f.description}</p>
              <ul className="space-y-2">
                {f.bullets.map((b) => (
                  <li key={b} className="flex items-start gap-2 text-sm text-slate-600">
                    <CheckCircle2 size={15} className="mt-0.5 shrink-0 text-primary" />
                    <span dangerouslySetInnerHTML={{ __html: b }} />
                  </li>
                ))}
              </ul>
            </div>
          ))}
        </div>
      </div>
    </section>
  )
}

// ── How It Works ──────────────────────────────────────────────────────────────

function HowItWorks() {
  const steps = [
    {
      step: '01',
      icon: <Package size={24} className="text-primary" />,
      title: 'Register &amp; Add Your Products',
      description:
        'Create your business account, set up your company profile, and import your product catalog. Each product gets its own intelligent profile with pricing, descriptions, and media.',
    },
    {
      step: '02',
      icon: <QrCode size={24} className="text-primary" />,
      title: 'Generate &amp; Deploy QR Codes',
      description:
        'Echofy automatically generates unique QR codes for every product. Print them on packaging, shelf labels, or marketing materials — wherever your customers will encounter them.',
    },
    {
      step: '03',
      icon: <Link2 size={24} className="text-primary" />,
      title: 'Launch Referral Programs',
      description:
        'Configure reward structures, issue referral links to customers, and let your network do the selling. Every referral is tracked automatically from click to conversion.',
    },
    {
      step: '04',
      icon: <TrendingUp size={24} className="text-primary" />,
      title: 'Measure &amp; Optimize',
      description:
        'Watch your analytics dashboard fill up with scan data, referral conversions, and revenue attribution. Identify top performers and double down on what works.',
    },
  ]

  return (
    <section id="how-it-works" className="bg-slate-50 py-24">
      <div className="mx-auto max-w-6xl px-6">
        <div className="text-center">
          <h2 className="text-3xl font-extrabold text-slate-900 md:text-4xl">
            Up and running in minutes
          </h2>
          <p className="mx-auto mt-4 max-w-xl text-slate-500">
            From product catalog to live QR codes and referral programs in four simple steps.
          </p>
        </div>
        <div className="mt-16 grid gap-8 md:grid-cols-2 lg:grid-cols-4">
          {steps.map((s, i) => (
            <div key={s.step} className="relative">
              {i < steps.length - 1 && (
                <div className="absolute left-full top-8 hidden w-full border-t-2 border-dashed border-slate-200 lg:block" style={{ width: 'calc(100% - 3rem)', left: '4rem' }} />
              )}
              <div className="flex h-16 w-16 items-center justify-center rounded-2xl bg-primary/10">
                {s.icon}
              </div>
              <p className="mt-4 text-xs font-bold uppercase tracking-widest text-primary">{s.step}</p>
              <h3 className="mt-1 text-base font-bold text-slate-900" dangerouslySetInnerHTML={{ __html: s.title }} />
              <p className="mt-2 text-sm text-slate-500 leading-relaxed" dangerouslySetInnerHTML={{ __html: s.description }} />
            </div>
          ))}
        </div>
      </div>
    </section>
  )
}

// ── Social Proof ──────────────────────────────────────────────────────────────

function SocialProof() {
  const testimonials = [
    {
      quote:
        "We went from zero referral program to 400 active advocates in six weeks. The QR product pages alone cut our support tickets by 30%.",
      name: 'Maria Santos',
      title: 'Head of Sales, NovaTech Distribution',
    },
    {
      quote:
        "Echofy replaced three separate tools. Our field reps scan a product, see live pricing, and send a referral link — all in one tap.",
      name: 'Daniel Oliveira',
      title: 'Operations Director, Iberstock',
    },
    {
      quote:
        "The multi-tenant architecture means each of our subsidiary brands has its own environment. The SuperAdmin view across all of them is a game-changer.",
      name: 'Carla Mendes',
      title: 'CTO, Grupo Meridian',
    },
  ]

  return (
    <section className="py-24">
      <div className="mx-auto max-w-6xl px-6">
        <h2 className="mb-12 text-center text-3xl font-extrabold text-slate-900 md:text-4xl">
          Trusted by growing businesses
        </h2>
        <div className="grid gap-8 md:grid-cols-3">
          {testimonials.map((t) => (
            <div key={t.name} className="rounded-2xl border border-slate-100 bg-white p-8 shadow-sm">
              <p className="text-slate-600 leading-relaxed">"{t.quote}"</p>
              <div className="mt-6">
                <p className="font-semibold text-slate-900">{t.name}</p>
                <p className="text-sm text-slate-400">{t.title}</p>
              </div>
            </div>
          ))}
        </div>
      </div>
    </section>
  )
}

// ── CTA ───────────────────────────────────────────────────────────────────────

function Cta() {
  return (
    <section id="pricing" className="py-24">
      <div className="mx-auto max-w-4xl px-6">
        <div className="rounded-3xl bg-gradient-to-br from-primary to-primary-700 px-10 py-16 text-center text-white shadow-2xl shadow-primary/20">
          <h2 className="text-3xl font-extrabold md:text-4xl">
            Ready to amplify your product reach?
          </h2>
          <p className="mx-auto mt-4 max-w-lg text-primary-100 text-lg">
            Join 500+ businesses using Echofy to turn every scan into a revenue opportunity.
            Start your 30-day free trial today — no credit card required.
          </p>
          <div className="mt-10 flex flex-col items-center justify-center gap-4 sm:flex-row">
            <Link
              to="/register"
              className="flex items-center gap-2 rounded-xl bg-white px-8 py-4 text-base font-bold text-primary shadow-lg hover:bg-slate-50 transition-all hover:-translate-y-0.5"
            >
              Start Free Trial <ArrowRight size={18} />
            </Link>
            <a
              href="mailto:sales@echofy.dev"
              className="flex items-center gap-2 rounded-xl border border-white/30 bg-white/10 px-8 py-4 text-base font-semibold text-white hover:bg-white/20 transition-all"
            >
              Talk to Sales
            </a>
          </div>
        </div>
      </div>
    </section>
  )
}

// ── Page ──────────────────────────────────────────────────────────────────────

export default function LandingPage() {
  return (
    <>
      <Hero />
      <Stats />
      <Features />
      <HowItWorks />
      <SocialProof />
      <Cta />
    </>
  )
}
