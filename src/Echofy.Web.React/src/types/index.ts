// ── Invoices ──────────────────────────────────────────────────────────────────
export interface InvoiceListItemDto {
  id: number
  companyId: number | null
  companyName: string | null
  invoiceNumber: string
  customerName: string
  customerEmail: string
  status: string
  issuedDate: string
  dueDate: string
  total: number
  sentAt: string | null
  paidAt: string | null
}

export interface ThankYouNoteDto {
  sentAt: string
  referralIncluded: boolean
  referralCode: string | null
}

export interface InvoiceDto extends InvoiceListItemDto {
  customerPhone: string | null
  notes: string | null
  createdAt: string
  rewardPointsAwarded: number
  rewardGiftCardAmount: number
  rewardGiftCardCode: string | null
  thankYouNote: ThankYouNoteDto | null
}

// ── Reward Programs ───────────────────────────────────────────────────────────
export interface RewardTierDto {
  id: number
  rewardProgramId: number
  label: string
  minInvoiceAmount: number
  pointsForReferrer: number
  giftCardAmount: number
  isActive: boolean
  displayOrder: number
}

export interface RewardProgramDto {
  id: number
  clientId: number
  companyId: number | null
  companyName: string | null
  name: string
  isActive: boolean
  createdAt: string
  tiers: RewardTierDto[]
}

// ── Referrals ─────────────────────────────────────────────────────────────────
export interface ReferralUseDto {
  usedByEmail: string
  usedAt: string
  hasReward: boolean
  rewardStatus: string
  welcomeCouponCode: string | null
}

export interface ReferralRewardDto {
  id: number
  pointsEarned: number
  status: string
  description: string
  createdAt: string
  issuedAt: string | null
}

export interface ReferralDto {
  codeId: number
  code: string
  shareUrl: string
  shareText: string
  totalReferrals: number
  totalPoints: number
  pendingPoints: number
  recentUses: ReferralUseDto[]
  rewards: ReferralRewardDto[]
}

export interface AdminReferralDto {
  rewardId: number
  referrerId: string
  referrerEmail: string
  referrerName: string
  referralCode: string
  usedByEmail: string
  welcomeCoupon: string | null
  pointsEarned: number
  status: string
  usedAt: string
  approvedAt: string | null
  issuedAt: string | null
}

// ── Auth ───────────────────────────────────────────────────────────────────────
export interface LoginResponse {
  token: string
  email: string
  fullName: string
  role: string
  modules: string[]
  clientId: number | null
  clientName: string | null
  expires: string
}

export interface AuthUser {
  id: string
  email: string
  fullName: string
  role: string
  modules: string[]
  clientId: number | null
  clientName: string | null
}

// ── Enums ──────────────────────────────────────────────────────────────────────
export enum LeadStatus { New, Contacted, Qualified, Lost, Converted }
export enum DealStage { Prospecting, Proposal, Negotiation, ClosedWon, ClosedLost }
export enum DiscountType { Percentage, FixedAmount }

// ── Dashboard ─────────────────────────────────────────────────────────────────
export interface DashboardStats {
  outOfStockProducts: number
  newCustomers: number
  percentageDiscountShare: number
  fixedCartDiscountShare: number
  fixedProductDiscountShare: number
}

// ── Products ──────────────────────────────────────────────────────────────────
export interface ProductImage {
  id: number
  productId: number
  fileName: string
  altText: string | null
  sku: string | null
  isMain: boolean
  displayOrder: number
  uploadedAt: string
}

export interface ProductPriceHistory {
  id: number
  productId: number
  price: number
  effectiveFrom: string
  effectiveTo: string | null
  changedByUserId: string | null
}

export interface DiscountOffer {
  id: number
  productId: number
  name: string
  discountType: DiscountType
  discountValue: number
  startDate: string
  endDate: string
  isActive: boolean
  isCurrentlyRunning: boolean
}

export interface ProductShortId {
  id: number
  productId: number | null
  productName: string | null
  code: string
  label: string | null
  createdAt: string
  assignedAt: string | null
}

export interface Company {
  id: number
  clientId: number
  clientName: string | null
  name: string
  email: string | null
  phone: string | null
  website: string | null
  taxNumber: string | null
  address: string | null
  city: string | null
  country: string | null
  isActive: boolean
  createdAt: string
  productCount: number
  invoiceCount: number
}

export interface Product {
  id: number
  companyId: number | null
  companyName: string | null
  name: string
  description: string
  price: number
  effectivePrice: number
  stockQuantity: number
  categoryIds: number[]
  categoryNames: string[]
  sku: string | null
  manufacturerUpc: string | null
  imageUrl: string | null
  size: string | null
  manufacturerId: number | null
  manufacturerName: string | null
  manufacturerWebsite: string | null
  manufacturerProductId: number | null
  manufacturerPartNumber: string | null
  unitOfMeasureId: number | null
  unitOfMeasureName: string | null
  unitOfMeasureAbbreviation: string | null
  isActive: boolean
  createdAt: string
  images: ProductImage[]
  priceHistory: ProductPriceHistory[]
  discountOffers: DiscountOffer[]
  additionalShortIds: ProductShortId[]
  activeOffer: DiscountOffer | null
}

// ── Customers ─────────────────────────────────────────────────────────────────
export interface CustomerListItem {
  id: number
  clientId: number | null
  clientName: string | null
  fullName: string
  email: string
  phone: string
  avatarUrl: string | null
  joinedDate: string
}

export interface Customer {
  id: number
  clientId: number | null
  clientName: string | null
  fullName: string
  email: string
  phone: string
  avatarUrl: string | null
  joinedDate: string
  notes: string | null
  street: string
  city: string
  province: string
  country: string
  reviews: Review[]
}

// ── Leads ─────────────────────────────────────────────────────────────────────
export interface Lead {
  id: number
  fullName: string
  email: string
  company: string | null
  phone: string | null
  status: LeadStatus
  estimatedValue: number
  assignedTo: string | null
  createdAt: string
  dealCount: number
}

// ── Deals ─────────────────────────────────────────────────────────────────────
export interface Deal {
  id: number
  title: string
  leadId: number
  leadName: string
  stage: DealStage
  value: number
  expectedCloseDate: string | null
  createdAt: string
}

export interface CrmAnalytics {
  totalDeals: number
  totalValue: number
  wonDeals: number
  wonValue: number
  byStage: Record<string, number>
}

// ── Contacts ──────────────────────────────────────────────────────────────────
export interface Contact {
  id: number
  firstName: string
  lastName: string
  fullName: string
  email: string
  phone: string | null
  company: string | null
  createdAt: string
}

// ── Reviews ───────────────────────────────────────────────────────────────────
export interface Review {
  id: number
  productId: number
  productName?: string
  appUserId: string
  userName: string
  rating: number
  comment: string | null
  createdAt: string
}

// ── Audit ─────────────────────────────────────────────────────────────────────
export interface AuditLog {
  id: number
  entityName: string
  action: 'Created' | 'Updated' | 'Deleted'
  entityId: string | null
  oldValues: string | null
  newValues: string | null
  changedByUserId: string | null
  changedAt: string
}

export interface AuditLogPage {
  total: number
  page: number
  pageSize: number
  logs: AuditLog[]
}

// ── Admin ─────────────────────────────────────────────────────────────────────
export interface AppUser {
  id: string
  fullName: string
  email: string
  role: string
  clientId: number | null
  clientName: string | null
}

export interface Client {
  id: number
  name: string
  slug: string
  hasECommerce: boolean
  hasCrm: boolean
  hasKanban: boolean
  hasCalendar: boolean
  hasChat: boolean
  isActive: boolean
  allowCompanyRewardOverride: boolean
}

export interface NfcClientSetting {
  clientId: number
  clientName: string
  password: string | null
}

export interface NfcClientSettingListItem {
  id: number
  name: string
  hasPassword: boolean
}

export interface Category {
  id: number
  name: string
  slug: string
  description: string | null
  isActive: boolean
  productCount: number
}

export interface Manufacturer {
  id: number
  name: string
  website: string | null
  isActive: boolean
}

export interface ManufacturerProductImage {
  id: number
  manufacturerProductId: number
  fileName: string
  altText: string | null
  isMain: boolean
  displayOrder: number
  uploadedAt: string
  url: string
}

export interface ManufacturerProduct {
  id: number
  manufacturerId: number
  manufacturerName: string
  name: string
  manufacturerPartNumber: string | null
  sku: string | null
  description: string | null
  size: string | null
  msrp: number | null
  unitOfMeasureId: number | null
  unitOfMeasureName: string | null
  unitOfMeasureAbbreviation: string | null
  isActive: boolean
  createdAt: string
  productCount: number
  mainImageFileName: string | null
  images: ManufacturerProductImage[]
}

export interface UnitOfMeasure {
  id: number
  name: string
  abbreviation: string
  isActive: boolean
}
