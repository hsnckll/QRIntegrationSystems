export function readPanelRoute(hash, pages, defaultPage, tabs = []) {
  const params = new URLSearchParams(hash.replace(/^#/, ""))
  const page = pages.includes(params.get("page")) ? params.get("page") : defaultPage
  const value = params.get("business")
  const business = page === "businesses" && value && /^[1-9]\d*$/.test(value) && Number.isSafeInteger(Number(value)) ? Number(value) : null
  const tab = business && tabs.includes(params.get("tab")) ? params.get("tab") : "info"
  return { page, business, tab }
}

export function panelHash(route) {
  const params = new URLSearchParams({ page: route.page })
  if (route.business) { params.set("business", String(route.business)); params.set("tab", route.tab || "info") }
  return "#" + params.toString()
}
