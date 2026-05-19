# Echofy — Product Capabilities

## QR Code & NFC Product Intelligence

- Every product gets a unique **short ID** / QR code label — batches can be generated and assigned from a pool
- Scanning a QR code opens a public **product page** with full details, pricing, images, and reviews
- Products can also be looked up by **UPC/barcode** (used in the mobile app scanner)
- A separate **NFC card writer app** (console) writes product URLs to NTAG 213/215/216 cards, supports card password protection, and reads existing NDEF content via PC/SC readers

## Referral Engine

- Every registered user gets a unique **referral code/link** automatically
- New users who register via a referral link receive a **welcome coupon**
- When an invoice is paid, the referring user's reward is tracked and moves through an approval workflow (Pending → Approved → Issued → Cancelled)
- Admins configure **reward programs** with custom tiers — reward types include points and gift cards
- Post-invoice **thank-you notes** are sent by email with the customer's referral link embedded

## E-Commerce Catalog

- Full product CRUD with images, SKU, UPC, stock, pricing, and category / manufacturer / unit-of-measure references
- **Price history** is automatically recorded on create and on every price change
- **Discount offers** (percentage or fixed amount) with start/end dates per product
- Product **favorites** (bookmarking per user) and **reviews** (one review per user)
- Manufacturer's own product catalog managed separately and linkable to internal products

## CRM & Sales Pipeline

- **Leads** with status tracking (New → Contacted → Qualified → Converted / Lost)
- **Deals** with multi-stage pipeline (Prospecting → Closed-Won / Closed-Lost) and value tracking
- **Contacts** database linked to the CRM
- Deal analytics endpoint for pipeline value and stage distribution

## Invoicing & Accounting

- Full invoice lifecycle: Draft → Sent → Paid / Overdue / Cancelled
- Line items, tax rate, company and customer association
- Invoice delivery by email; mark-paid and cancel actions
- Customer portal: customers can view their own invoice history

## Customer Portal

- Customers see their own dashboard, invoices, favorites, and referral status
- Role `Customer` gets a restricted nav — only their own data is accessible

## Admin & Multi-Tenancy

- Each **Client** (tenant) has independent feature flags: e-commerce, CRM, kanban, calendar, chat
- Per-client companies, NFC settings, and reward programs
- Full **user management** with roles: SuperAdmin, SuperUser, Admin, Manager, Sales, Support, Customer
- **Audit log** captures every create / update / delete with user attribution and timestamps

## Recommendation Engine (Standalone Service)

- Personalized recommendations using **Jaccard similarity** on user favorites
- Popular products ranked by ratings (60%) and favorite count (40%)
- Similar products by category and manufacturer

## Mobile App (.NET MAUI / Android)

- Barcode / QR **scanner** that looks up products by UPC via the API
- Product detail view with images
- Authenticated login with JWT

## Tech Stack

| Layer | Technology |
|-------|------------|
| Backend | .NET 10, ASP.NET Core, EF Core, PostgreSQL (Npgsql), JWT auth |
| Frontend SPA | React 19, TypeScript, Tailwind CSS, TanStack Query, Vite |
| Mobile | .NET MAUI (Android), ZXing.Net.MAUI for scanning |
| NFC App | .NET console, PCSC library, NTAG 213/215/216 support |
| Recommendation API | Standalone .NET minimal API |

## API Surface (summary)

| Area | Key Endpoints |
|------|---------------|
| Auth | `POST /api/auth/register`, `POST /api/auth/login` |
| Products | CRUD, images, short IDs, discount offers, barcode lookup |
| Customers | CRUD |
| Invoices | CRUD, send, mark-paid, cancel, thank-you note |
| CRM | Leads, Deals, Contacts + analytics |
| Referrals | User code, admin approve / issue / cancel |
| Reward Programs | CRUD + tier management |
| Admin | Users, Clients, Companies, Categories, Manufacturers, Units, Reviews, Audit Logs, NFC Settings |
| Customer Portal | Favorites toggle, reviews, my invoices, my referral link |
| Dashboard | Aggregate stats |
| Recommendations | Personalized, popular, similar-product |
