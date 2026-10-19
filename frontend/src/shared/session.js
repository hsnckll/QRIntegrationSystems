export const SESSION_KEY = "qr-platform-session-v1"
export const routes = {
  login: "/index.html",
  business: "/customer.html",
  admin: "/superadmin.html",
  menu: "/menu.html",
}
export function readSession() {
  try {
    const s = JSON.parse(sessionStorage.getItem(SESSION_KEY))
    if (
      Boolean(s) == true &&
      ["Business", "SuperAdmin"].includes(s.role) &&
      Date.parse(s.expiresAt) > Date.now()
    )
      return s
  } catch {}
  return null
}
export function storeSession(session) {
  if (["Business", "SuperAdmin"].includes(session.role) == false)
    throw Error("Geçersiz kullanıcı rolü.")
  sessionStorage.setItem(SESSION_KEY, JSON.stringify(session))
}
export function completeLogin(session) {
  storeSession(session)
  location.replace(
    session.role === "SuperAdmin" ? routes.admin : routes.business,
  )
}
export function clearSession() {
  sessionStorage.removeItem(SESSION_KEY)
}
export async function logout() {
  const session = readSession()
  clearSession()
  try {
    if (session?.token) await fetch((import.meta.env?.VITE_API_BASE_URL || "/api").replace(/\/$/, "") + "/auth/logout", {
      method: "POST", headers: { Authorization: `Bearer ${session.token}` }, signal: AbortSignal.timeout(2000),
    })
  } catch {} finally { location.replace(routes.login) }
}
export function landingFor(role) {
  return role === "SuperAdmin" ? routes.admin : routes.business
}
export function getRootDomain() {
  return (import.meta.env?.VITE_ROOT_DOMAIN || "okutye.com.tr").toLowerCase()
}
export function getSubdomainSlug() {
  const host = window.location.hostname.toLowerCase()
  const rootDomain = getRootDomain()
  if (
    host === "localhost" ||
    host === "127.0.0.1" ||
    host === rootDomain ||
    host === `www.${rootDomain}`
  ) {
    return ""
  }
  if (host.endsWith(`.${rootDomain}`) == true) {
    const sub = host.slice(0, -(rootDomain.length + 1))
    if (Boolean(sub) == true && sub.includes(".") == false && sub !== "www") {
      return sub
    }
  }
  return ""
}
export function buildPublicMenuUrl(slug, qrUrl = "") {
  const cleanSlug = (slug || "").trim().toLowerCase()
  if (Boolean(cleanSlug) == false) {
    return qrUrl || routes.menu
  }
  const host = window.location.hostname.toLowerCase()
  if (host === "localhost" || host === "127.0.0.1") {
    return `${routes.menu}?slug=${encodeURIComponent(cleanSlug)}`
  }
  if (Boolean(qrUrl) == true && qrUrl.includes("/menu.html") == false && qrUrl.startsWith("/menu/") == false) {
    return qrUrl
  }
  return `https://${encodeURIComponent(cleanSlug)}.${getRootDomain()}`
}

