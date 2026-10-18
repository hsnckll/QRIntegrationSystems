import { authApi } from "./api/endpoints"
import { sessionFromApi } from "./api/mappers"
// ── SUPERADMIN GİRİŞİ → GERÇEK BACKEND ──────────────────────────────────────
// POST /api/auth/admin-login  →  { token, role, expiresAt }
// Dönen JWT token'ı oturuma kaydediyoruz; sonraki tüm API isteklerinde
// http.js bu token'ı otomatik olarak "Authorization: Bearer ..." başlığına ekler.
export async function adminLogin(email, password) {
  const response = await authApi.adminLogin(email.trim(), password)
  const apiSession = sessionFromApi(response)
  return { ...apiSession, email: email.trim() }
}

// ── İŞLETME GİRİŞİ (OTP) → GERÇEK BACKEND ───────────────────────────────────
// POST /api/auth/send-otp   →  { email }
// POST /api/auth/verify-otp →  { email, code }
export async function sendOtp(email) {
  const cleanEmail = email.trim().toLowerCase()
  const response = await authApi.sendOtp(cleanEmail)
  return {
    message: response?.message || "Doğrulama kodu işletme e-posta adresinize gönderildi.",
    resendAt: Date.now() + 30 * 1000,
    expiresAt: Date.now() + 5 * 60 * 1000,
  }
}

export async function verifyOtp(email, code) {
  const cleanEmail = email.trim().toLowerCase()
  const cleanCode = code.trim()
  const response = await authApi.verifyOtp(cleanEmail, cleanCode)
  const apiSession = sessionFromApi(response)
  return { ...apiSession, email: cleanEmail }
}
