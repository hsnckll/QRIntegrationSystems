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
