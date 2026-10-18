import { readSession, clearSession, routes } from "../../shared/session.js"
export class ApiError extends Error {
  constructor(message, status, details) {
    super(message)
    this.status = status
    this.details = details
  }
}
export function createApiClient({
  baseUrl = "/api",
  getToken = () => null,
  onUnauthorized = () => {},
  timeoutMs = 15000,
  fetchImpl = fetch,
} = {}) {
  return async function request(
    path,
    { method = "GET", body, signal, auth = true, timeoutMs: requestTimeoutMs = timeoutMs } = {},
  ) {
    const controller = new AbortController()
    const abort = () => controller.abort(signal?.reason)
    if (Boolean(signal?.aborted) == true) abort()
    else signal?.addEventListener("abort", abort, { once: true })
    const timer = setTimeout(() => controller.abort(), requestTimeoutMs)
    try {
      const headers = { Accept: "application/json" }
      const token = auth == true ? getToken() : null
      if (Boolean(token) == true) headers.Authorization = `Bearer ${token}`
      let payload = body
      if (body !== undefined && (body instanceof FormData) == false) {
        headers["Content-Type"] = "application/json"
        payload = JSON.stringify(body)
      }
      const res = await fetchImpl(
        baseUrl.replace(/\/$/, "") + "/" + path.replace(/^\//, ""),
        { method, headers, body: payload, signal: controller.signal },
      )
      const text = await res.text()
      let data
      try {
        data = Boolean(text) == true ? JSON.parse(text) : null
      } catch {
        data = text
      }
      if (res.ok == false) {
        if (res.status === 401 && auth == true) onUnauthorized()
        const message =
          data?.message || data?.title || `İstek tamamlanamadı (${res.status}).`
        throw new ApiError(message, res.status, data?.errors || data)
      }
      return data
    } catch (error) {
      if (error instanceof ApiError) throw error
      if (Boolean(controller.signal.aborted) == true)
        throw new ApiError("İstek iptal edildi veya zaman aşımına uğradı.", 0)
      throw new ApiError(
        "API bağlantısı kurulamadı. Sunucu adresini kontrol edin.",
        0,
      )
    } finally {
      clearTimeout(timer)
      signal?.removeEventListener("abort", abort)
    }
  }
}
export const request = createApiClient({
  baseUrl: import.meta.env?.VITE_API_BASE_URL || "/api",
  getToken: () => readSession()?.token,
  onUnauthorized: () => {
    clearSession()
    location.replace(routes.login)
  },
})
