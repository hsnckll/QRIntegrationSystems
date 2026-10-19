import { request } from "./http"
const id = (n) => {
  if (Number.isSafeInteger(Number(n)) == false || Number(n) <= 0)
    throw Error("API işlemleri gerçek sayısal kayıt kimliği gerektirir.")
  return Number(n)
}
export const authApi = {
  adminLogin: (email, password) =>
    request("auth/admin-login", {
      method: "POST",
      body: { email, password },
      auth: false,
    }),
  sendOtp: (email) =>
    request("auth/send-otp", { method: "POST", body: { email }, auth: false }),
  verifyOtp: (email, code) =>
    request("auth/verify-otp", {
      method: "POST",
      body: { email, code },
      auth: false,
    }),
}
export const businessApi = {
  list: () => request("business"),
  get: (n) => request(`business/${id(n)}`),
  profile: () => request("business/profile"),
  statistics: () => request("business/statistics"),
  create: (body) => request("business", { method: "POST", body }),
  update: (n, body) => request(`business/${id(n)}`, { method: "PUT", body }),
  remove: (n) => request(`business/${id(n)}`, { method: "DELETE" }),
  toggle: (n) =>
    request(`business/${id(n)}/toggle-active`, { method: "PATCH" }),
  logo: (n, imagePath) =>
    request(`business/${id(n)}/logo`, { method: "PATCH", body: { imagePath } }),
  banner: (n, imagePath) =>
    request(`business/${id(n)}/banner`, {
      method: "PATCH",
      body: { imagePath },
    }),
}
function tenantCollection(name) {
  return {
    list: () => request(name),
    get: (n) => request(`${name}/${id(n)}`),
    create: (body) => request(name, { method: "POST", body }),
    update: (n, body) => request(`${name}/${id(n)}`, { method: "PUT", body }),
    remove: (n) => request(`${name}/${id(n)}`, { method: "DELETE" }),
    toggle: (n) =>
      request(`${name}/${id(n)}/toggle-active`, { method: "PATCH" }),
  }
}
// Business users are scoped to their JWT claim; SuperAdmin uses adminMenuApi.
export const categoryApi = tenantCollection("category")
export const productApi = {
  ...tenantCollection("product"),
  image: (n, imagePath) =>
    request(`product/${id(n)}/image`, { method: "PATCH", body: { imagePath } }),
}
export const subscriptionApi = {
  list: (businessId) => request(`subscription/business/${id(businessId)}`),
  active: (businessId) =>
    request(`subscription/business/${id(businessId)}/active`),
  create: (businessId, body) =>
    request(`subscription/business/${id(businessId)}`, {
      method: "POST",
      body,
    }),
  remove: (businessId, n) =>
    request(`subscription/business/${id(businessId)}/${id(n)}`, {
      method: "DELETE",
    }),
}
export const templateApi = {
  list: () => request("template"),
  upload: ({archive,files,paths}) => {const body=new FormData();if(archive)body.append("Archive",archive);else{files.forEach(f=>body.append("Files",f));body.append("Paths",JSON.stringify(paths))}return request("template/package",{method:"POST",body,timeoutMs:120000})},
  create: (body) => request("template", { method: "POST", body }),
  remove: (n) => request(`template/${id(n)}`, { method: "DELETE" }),
}
export const dashboardApi = { summary: () => request("dashboard/summary"), bootstrap: () => request("dashboard/bootstrap") }
export const menuApi = {
  get: (slug) => request(`menu/${encodeURIComponent(slug)}`, { auth: false }),
}
export const uploadApi = {
  image: (file, folder = "general", businessId = null) => {
    const form = new FormData()
    form.append("File", file)
    form.append("Folder", folder)
    if (businessId != null) form.append("BusinessId", String(id(businessId)))
    return request("upload/image", { method: "POST", body: form })
  },
}
export function adminMenuApi(name, businessId) {
  if (!["category", "product"].includes(name)) throw Error("Geçersiz menü kaynağı.")
  const scope = '?businessId=' + id(businessId)
  return {
    list: () => request(name + scope),
    create: (body) => request(name + scope, { method: 'POST', body }),
    update: (n, body) => request(name + '/' + id(n) + scope, { method: 'PUT', body }),
    remove: (n) => request(name + '/' + id(n) + scope, { method: 'DELETE' }),
    image: (n, imagePath) => request(name + '/' + id(n) + '/image' + scope,
      { method: 'PATCH', body: { imagePath } }),
  }
}
export const logApi = {
  list: (businessId) => request(businessId == null ? 'log' : `log?businessId=${id(businessId)}`),
}
export const pendingBackendCapabilities = [
  "subscriptionUpdate",
  "businessRestore",
  "qrListGenerateToggle",
]
