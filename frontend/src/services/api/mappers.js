export const businessFromApi = (b) => ({
  id: b.id,
  name: b.name,
  slug: b.slug,
  owner: b.ownerName || "",
  email: b.email,
  phone: b.phone || "",
  address: b.address || "",
  logo: b.logoPath || "",
  banner: b.bannerPath || "",
  active: b.isActive,
  template: b.template?.id,
  deleted: false,
})
// Deliberately omits email/slug; the current UpdateBusinessDto does not accept them.
export const businessUpdateDto = (b) => ({
  name: b.name,
  ownerName: b.owner,
  phone: b.phone,
  address: b.address,
  templateId: b.template,
})
export const categoryFromApi = (c) => ({
  id: c.id,
  name: c.name,
  active: c.isActive,
  sortOrder: c.sortOrder,
})
export const categoryUpdateDto = (c, sortOrder) => ({
  name: c.name,
  isActive: c.active,
  sortOrder,
})
export const productFromApi = (p) => ({
  id: p.id,
  category: p.categoryId,
  name: p.name,
  description: p.description || "",
  price: p.price,
  image: p.imagePath || "",
  active: p.isActive,
  sortOrder: p.sortOrder,
})
export const productUpdateDto = (p, sortOrder) => ({
  categoryId: Number(p.category),
  name: p.name,
  description: p.description,
  price: Number(p.price),
  sortOrder,
  isActive: p.active,
})
export const sessionFromApi = (r) => {
  if (
    Boolean(r.token) == false ||
    ["SuperAdmin", "Business"].includes(r.role) == false ||
    Number.isFinite(Date.parse(r.expiresAt)) == false
  )
    throw Error("Sunucudan geçerli oturum yanıtı alınamadı.")
  return { token: r.token, role: r.role, expiresAt: r.expiresAt, mode: "api" }
}
