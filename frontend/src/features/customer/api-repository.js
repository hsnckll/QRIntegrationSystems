// ─────────────────────────────────────────────────────────────────────────────
// CUSTOMER (İŞLETME) API REPOSITORY  (MVC "Model / Service" katmanı)
//
// İşletme panelinin (.NET 8 Web API) ile olan tüm veri alışverişini yönetir:
//   • loadCustomerData()     → İşletme profili, kategoriler ve ürünleri çeker.
//   • Kategori CRUD          → Ekle, Güncelle, Sil, Durum Değiştir
//   • Ürün CRUD              → Ekle (görsel upload dahil), Güncelle, Sil, Durum
//   • Profil Güncelleme      → İletişim & adres bilgilerini günceller
// ─────────────────────────────────────────────────────────────────────────────

import {
  businessApi,
  categoryApi,
  productApi,
  uploadApi,
} from "../../services/api/endpoints"
import { buildPublicMenuUrl } from "../../shared/session"

const dayOnly = (value) => (Boolean(value) == true ? String(value).slice(0, 10) : "")

// Paneldeki resim seçici base64 dataUrl üretir; sunucu gerçek dosya bekler
async function uploadProductImage(dataUrl) {
  const blob = await (await fetch(dataUrl)).blob()
  const file = new File([blob], "product.webp", { type: blob.type })
  const res = await uploadApi.image(file, "products")
  return res.url
}

// ── 1) VERİLERİ YÜKLE ────────────────────────────────────────────────────────
export async function loadCustomerData() {
  const [profile, categories, products] = await Promise.all([
    businessApi.profile(),
    categoryApi.list(),
    productApi.list(),
  ])

  const subscription = profile.activeSubscription || profile.latestSubscription
  return {
    business: {
      id: profile.id,
      name: profile.name,
      slug: profile.slug,
      owner: profile.ownerName || "",
      phone: profile.phone || "",
      email: profile.email,
      address: profile.address || "",
      logo: profile.logoPath || "",
      banner: profile.bannerPath || "",
      active: profile.isActive,
      template: profile.template ? profile.template.id : null,
      templateName: profile.template ? profile.template.folderName : "",
      qrUrl: buildPublicMenuUrl(profile.slug, profile.activeQRCode ? profile.activeQRCode.targetUrl : ""),
      qrImage: profile.activeQRCode ? profile.activeQRCode.qrImagePath : "",
    },
    categories: categories.map((c) => ({
      id: c.id,
      name: c.name,
      active: c.isActive,
      sortOrder: c.sortOrder,
    })),
    products: products.map((p) => ({
      id: p.id,
      category: p.categoryId,
      name: p.name,
      description: p.description || "",
      price: p.price,
      active: p.isActive,
      image: p.imagePath || "",
      sortOrder: p.sortOrder,
    })),
    subscription: subscription ? { start: subscription.startDate, end: subscription.endDate } : { start: "", end: "" },
  }
}

// ── 2) KATEGORİ İŞLEMLERİ ────────────────────────────────────────────────────
export function createCategory(item) {
  return categoryApi.create({
    name: item.name.trim(),
    sortOrder: Number(item.sortOrder || 0),
  })
}

export function updateCategory(item) {
  return categoryApi.update(item.id, {
    name: item.name.trim(),
    sortOrder: Number(item.sortOrder || 0),
    isActive: item.active,
  })
}

export function deleteCategory(id) {
  return categoryApi.remove(id)
}

export function toggleCategory(id) {
  return categoryApi.toggle(id)
}

// ── 3) ÜRÜN İŞLEMLERİ ────────────────────────────────────────────────────────
export async function createProduct(item) {
  let uploadedPath = ""
  if (Boolean(item.image) == true && item.image.startsWith("data:") == true) {
    uploadedPath = await uploadProductImage(item.image)
  }

  const created = await productApi.create({
    categoryId: Number(item.category),
    name: item.name.trim(),
    description: item.description || "",
    price: Number(item.price),
    sortOrder: Number(item.sortOrder || 0),
  })

  if (Boolean(uploadedPath) == true && created != null && created.id != null) {
    await productApi.image(created.id, uploadedPath)
  }

  return created
}

export async function updateProduct(item, oldProduct) {
  if (Boolean(item.image) == true && item.image.startsWith("data:") == true) {
    const uploadedPath = await uploadProductImage(item.image)
    await productApi.image(item.id, uploadedPath)
  }

  return await productApi.update(item.id, {
    categoryId: Number(item.category),
    name: item.name.trim(),
    description: item.description || "",
    price: Number(item.price),
    sortOrder: Number(item.sortOrder || 0),
    isActive: item.active,
  })
}

export function deleteProduct(id) {
  return productApi.remove(id)
}

export function toggleProduct(id) {
  return productApi.toggle(id)
}

// ── 4) İŞLETME BİLGİLERİ GÜNCELLEME ─────────────────────────────────────────
export function updateBusinessProfile(business) {
  return businessApi.update(business.id, {
    name: business.name.trim(),
    email: business.email.trim().toLowerCase(),
    ownerName: business.owner,
    phone: business.phone,
    address: business.address,
    templateId: business.template == null || business.template === "" ? null : Number(business.template),
  })
}

// ── 5) İŞLETME İSTATİSTİKLERİ ───────────────────────────────────────────────
export function getBusinessStatistics() {
  return businessApi.statistics()
}
