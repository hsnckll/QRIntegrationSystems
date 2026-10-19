// ─────────────────────────────────────────────────────────────────────────────
// SUPERADMIN API REPOSITORY  (MVC'deki "Model / Service" katmanı)
//
// Bu dosyanın tek görevi: .NET backend ile konuşmak.
//   • load...()  → Backend'den veriyi OKUR ve panelin anladığı şekle çevirir.
//   • diğerleri  → Panelde yapılan değişiklikleri backend'e YAZAR.
//
// Ekran (SuperAdminDashboard.jsx) hiçbir zaman doğrudan fetch/URL bilmez;
// sadece buradaki fonksiyonları çağırır. Yarın backend adresi veya alan adları
// değişirse yalnızca bu dosyayı güncellemek yeterli olur.
// ─────────────────────────────────────────────────────────────────────────────
import {
  adminMenuApi,
  logApi,
  businessApi,
  subscriptionApi,
  templateApi,
  dashboardApi,
  uploadApi,
} from "../../services/api/endpoints"
import { readSession } from "../../shared/session"

// ── 1) DÖNÜŞTÜRÜCÜLER (Backend DTO  →  Panel nesnesi) ───────────────────────
// Backend "ownerName", "isActive" gibi isimler kullanıyor; panel bileşenleri ise
// demo zamanından kalma "owner", "active" isimlerini bekliyor. Bu fonksiyonlar
// iki dünya arasında tercümanlık yapar.

// "2026-10-04T12:30:00Z" → "2026-10-04"  (paneldeki tarih hesapları bu formatı bekliyor)
const dayOnly = (value) => (value ? String(value).slice(0, 10) : "")

function toBusiness(detail) {
  return {
    id: detail.id,
    name: detail.name,
    slug: detail.slug,
    owner: detail.ownerName || "",
    email: detail.email,
    phone: detail.phone || "",
    address: detail.address || "",
    logo: detail.logoPath || "",
    banner: detail.bannerPath || "",
    active: detail.isActive,
    deleted: false, // Backend silinmiş işletmeleri listeye hiç göndermiyor
    template: detail.template ? detail.template.id : "",
    createdAt: dayOnly(detail.createdAt),
  }
}

function toTemplate(t) {
  return {
    id: t.id,
    name: t.name,
    folder: t.folderName,
    entryUrl: t.entryUrl,
    version: t.version,
    color: "#883d33",
  }
}

function toSubscription(s, businessId) {
  return {
    id: s.id,
    business: businessId,
    start: dayOnly(s.startDate),
    end: dayOnly(s.endDate),
    amount: s.amountPaid == null ? "" : s.amountPaid,
    method: s.paymentMethod || "",
    note: s.note || "",
    deleted: false,
  }
}

// İşletme detayındaki "activeQRCode" alanını QR paneli kartına çevirir
function toQr(detail) {
  const qr = detail.activeQRCode
  if (qr == null) return null
  return {
    id: qr.id,
    business: detail.id,
    label: "Menü QR kodu",
    image: qr.qrImagePath,
    url: qr.targetUrl,
    active: qr.isActive,
    createdAt: dayOnly(qr.createdAt),
  }
}

// Backend son 7 günü { "2026-10-01": 5, ... } şeklinde gönderiyor.
// Grafik bileşeni ise [{ date: "1 Eki", count: 5 }, ...] bekliyor.
function toSeries(dailyScanStats) {
  const stats = dailyScanStats || {}
  return Object.keys(stats)
    .sort()
    .map((key) => ({
      date: new Date(key + "T00:00:00").toLocaleDateString("tr-TR", {
        day: "numeric",
        month: "short",
      }),
      count: stats[key],
    }))
}

// Backend aylık ciro dağılımını { "2026-05": 500, "2026-06": 1200, ... } şeklinde gönderir.
function toMonthlyRevenueSeries(monthlyStats) {
  const stats = monthlyStats || {}
  const monthNames = [
    "Oca", "Şub", "Mar", "Nis", "May", "Haz",
    "Tem", "Ağu", "Eyl", "Eki", "Kas", "Ara"
  ]
  return Object.keys(stats)
    .sort()
    .map((key) => {
      const parts = key.split("-")
      const year = parts[0]
      const monthIdx = Number(parts[1]) - 1
      const shortName = monthNames[monthIdx] || parts[1]
      return {
        key,
        date: `${shortName} ${year}`,
        monthShort: shortName,
        year,
        count: Number(stats[key] || 0),
      }
    })
}

// ── 2) OKUMA: Panelin ihtiyaç duyduğu her şeyi tek seferde yükle ────────────
export async function loadAdminData() {
  // Birbirinden bağımsız 3 isteği AYNI ANDA gönderiyoruz (Promise.all),
  // böylece sırayla beklemekten daha hızlı yüklenir.
  const { businesses: details, templates, summary, logs, subscriptions } = await dashboardApi.bootstrap()
  const session = readSession()

  return {
    profile: {
      name: "Sistem Yöneticisi",
      email: session == null ? "" : session.email || "",
    },
    templates: templates.map(toTemplate),
    businesses: details.map(toBusiness),
    subscriptions: subscriptions.map(s => toSubscription(s, s.businessId)),
    qrs: details.map(toQr).filter((q) => q != null),
    logs: logs.map(toLog),
    categories: [],
    products: [],
    dashboard: {
      totalBusinesses: summary.totalBusinesses,
      activeBusinesses: summary.activeBusinesses,
      inactiveBusinesses: summary.inactiveBusinesses,
      totalScans: summary.totalQRScans,
      todayScans: summary.todayQRScans,
      monthScans: summary.thisMonthQRScans,
      totalRevenue: Number(summary.totalRevenue || 0),
      monthRevenue: Number(summary.thisMonthRevenue || 0),
      todayRevenue: Number(summary.todayRevenue || 0),
      revenueSeries: toMonthlyRevenueSeries(summary.monthlyRevenueStats),
      series: toSeries(summary.dailyScanStats),
    },
  }
}

// ── 3) YAZMA: İşletme işlemleri ─────────────────────────────────────────────
export function createBusiness(x) {
  const payload = {
    name: x.name.trim(),
    slug: x.slug.trim(),
    ownerName: x.owner,
    phone: x.phone,
    email: x.email.trim(),
    address: x.address,
    templateId: x.template === "" || x.template == null ? null : Number(x.template), // <select> değeri string gelir, backend int bekler
  }

  if (x.subscriptionPlan === "custom" && Boolean(x.subscriptionEndDate) == true) {
    payload.subscriptionEndDate = new Date(x.subscriptionEndDate + "T23:59:59Z").toISOString()
  } else if (Boolean(x.subscriptionPlan) == true) {
    payload.subscriptionMonths = Number(x.subscriptionPlan)
  }

  if (x.subscriptionPrice !== undefined && x.subscriptionPrice !== "") {
    payload.subscriptionAmountPaid = Number(x.subscriptionPrice)
  }

  if (Boolean(x.subscriptionMethod) == true) {
    payload.subscriptionPaymentMethod = x.subscriptionMethod
  }

  if (Boolean(x.subscriptionNote) == true) {
    payload.subscriptionNote = x.subscriptionNote
  }

  return businessApi.create(payload)
}

export function updateBusiness(x) {
  // Menü adresi sabittir; giriş e-postası işletme bilgileriyle güncellenebilir.
  return businessApi.update(x.id, {
    name: x.name.trim(),
    email: x.email.trim().toLowerCase(),
    ownerName: x.owner,
    phone: x.phone,
    address: x.address,
    templateId: x.template === "" || x.template == null ? null : Number(x.template),
  })
}

export function toggleBusiness(id) {
  return businessApi.toggle(id)
}

export function deleteBusiness(id) {
  return businessApi.remove(id) // Backend tarafında "soft delete" (IsDeleted = true)
}

// Paneldeki görsel seçici resmi "data:image/webp;base64,..." metnine çeviriyor.
// Backend ise gerçek bir dosya bekliyor → metni tekrar dosyaya çevirip yüklüyoruz.
async function uploadDataUrl(dataUrl, folder, businessId) {
  const blob = await (await fetch(dataUrl)).blob()
  const file = new File([blob], folder + ".webp", { type: blob.type })
  const res = await uploadApi.image(file, folder, businessId)
  return res.url // Örn: "/uploads/logos/abc123.webp"
}

export async function saveDesign(oldBusiness, next) {
  // 1) Şablon değiştiyse işletme bilgilerini güncelle
  if (String(oldBusiness.template) !== String(next.template)) {
    await updateBusiness(next)
  }

  // 2) Logo ve banner için: yeni resim seçildiyse yükle, kaldırıldıysa boşalt
  const images = [
    ["logo", "logos", businessApi.logo],
    ["banner", "banners", businessApi.banner],
  ]
  for (const [key, folder, save] of images) {
    if (oldBusiness[key] === next[key]) continue
    const isNewFile = next[key].startsWith("data:") === true
    const path = isNewFile ? await uploadDataUrl(next[key], folder, oldBusiness.id) : ""
    await save(next.id, path)
  }
}

// ── 4) YAZMA: Abonelik işlemleri ────────────────────────────────────────────
function subscriptionBody(x) {
  return {
    startDate: x.start,
    endDate: x.end,
    amountPaid: x.amount === "" ? null : Number(x.amount),
    paymentMethod: x.method,
    note: x.note,
  }
}

export function createSubscription(x) {
  return subscriptionApi.create(Number(x.business), subscriptionBody(x))
}

// Backend'de "abonelik güncelle" uç noktası yok. Bu yüzden düzenleme
// "eski kaydı sil + yenisini ekle" olarak yapılıyor.
export async function replaceSubscription(oldSub, next) {
  await subscriptionApi.remove(Number(oldSub.business), oldSub.id)
  await createSubscription(next)
}

function parseSnapshot(value) {
  if (value == null) return null
  try { return JSON.parse(value) } catch { return value }
}

function toLog(l) {
  return { id: l.id, business: l.businessId, action: l.action,
    actor: l.actorType, at: l.createdAt, old: parseSnapshot(l.oldValues), next: parseSnapshot(l.newValues) }
}

export async function loadBusinessLogs(businessId) {
  const logs = await logApi.list(businessId)
  return logs.map(toLog).filter(l => Number(l.business) === Number(businessId))
}

export async function loadBusinessMenu(businessId) {
  const [categories, products] = await Promise.all([
    adminMenuApi('category', businessId).list(), adminMenuApi('product', businessId).list(),
  ])
  return {
    categories: categories.map(c => ({ id: c.id, business: businessId, name: c.name,
      active: c.isActive, sortOrder: c.sortOrder })),
    products: products.map(p => ({ id: p.id, business: businessId, category: p.categoryId,
      name: p.name, description: p.description || '', price: p.price,
      active: p.isActive, image: p.imagePath || '', sortOrder: p.sortOrder })),
  }
}

export async function saveMenuItem(collection, item, existing) {
  const api = adminMenuApi(collection === 'categories' ? 'category' : 'product', item.business)
  const body = { name: item.name.trim(), sortOrder: Number(item.sortOrder ?? existing?.sortOrder ?? 0) }
  if (existing) body.isActive = item.active
  if (collection === 'products') Object.assign(body, {
    categoryId: Number(item.category), description: item.description || '', price: Number(item.price),
  })
  const result = existing ? await api.update(item.id, body) : await api.create(body)
  if (!existing && item.active === false) await api.update(result.id, { ...body, isActive: false })
  if (collection === 'products' && item.image !== (existing?.image || '')) {
    const path = item.image?.startsWith('data:') ? await uploadDataUrl(item.image, 'products', item.business) : item.image || ''
    await api.image(existing ? item.id : result.id, path)
  }
  return result
}

export function deleteMenuItem(collection, item) {
  return adminMenuApi(collection === 'categories' ? 'category' : 'product', item.business).remove(item.id)
}

export async function createTemplate(selection) {
  return toTemplate(await templateApi.upload(selection))
}

export function deleteTemplate(templateId) {
  return templateApi.remove(templateId)
}
