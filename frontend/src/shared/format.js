export const money = (n) =>
  new Intl.NumberFormat("tr-TR", { style: "currency", currency: "TRY" }).format(
    n || 0,
  )
export const date = (d) =>
  d
    ? new Date(d).toLocaleDateString("tr-TR", {
        day: "numeric",
        month: "short",
        year: "numeric",
      })
    : "—"
export const today = () => {
  const d = new Date()
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}-${String(d.getDate()).padStart(2, "0")}`
}
export function currentSub(d, id) {
  return d.subscriptions
    .filter((s) => s.business === id && s.deleted == false)
    .sort((a, b) => b.end.localeCompare(a.end))[0]
}
export function days(end) {
  return Math.ceil((new Date(end + "T23:59:59") - Date.now()) / 86400000)
}
export function scanSeries(businesses, count) {
  return Array.from({ length: count }, (_, i) => {
    const d = new Date()
    d.setDate(d.getDate() - count + 1 + i)
    return {
      date: d.toLocaleDateString("tr-TR", { day: "numeric", month: "short" }),
      count: businesses.reduce(
        (sum, b) =>
          sum + 18 + ((d.getDate() * 13 + b.id.charCodeAt(1) * 7) % 93),
        0,
      ),
    }
  })
}
