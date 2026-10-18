import React from "react"
import safeJson from "./safeJson"

const labels = {Name:"Ad",name:"Ad",Price:"Fiyat",price:"Fiyat",Description:"Açıklama",IsActive:"Menüde görünür",IsDeleted:"Silindi",SortOrder:"Sıra",CategoryId:"Kategori kimliği",TemplateId:"Şablon kimliği",Phone:"Telefon",Address:"Adres",OwnerName:"Yetkili",Email:"E-posta",Slug:"Menü adresi",AmountPaid:"Ödenen tutar",PaymentMethod:"Ödeme yöntemi",Note:"Not",StartDate:"Başlangıç",EndDate:"Bitiş",ImagePath:"Ürün görseli",LogoPath:"Logo",BannerPath:"Banner",TargetUrl:"QR bağlantısı"}
const ignored = new Set(["Id","BusinessId","CreatedAt","UpdatedAt","DeletedAt"])
export function changesFor(log) {
  const old = log.old && typeof log.old === "object" ? log.old : {}
  const next = log.next && typeof log.next === "object" ? log.next : {}
  return [...new Set([...Object.keys(old),...Object.keys(next)])]
    .filter(key => !ignored.has(key) && JSON.stringify(old[key]) !== JSON.stringify(next[key]))
    .map(key => ({ key, label: labels[key] || key, old: old[key], next: next[key] }))
}
function valueText(value,key) {
  if (value == null || value === "") return "—"
  if (typeof value === "boolean") return value ? "Evet" : "Hayır"
  if (/price|amountpaid/i.test(key)) return Number(value).toLocaleString("tr-TR",{style:"currency",currency:"TRY"})
  if (/image|logo|banner/i.test(key)) return "Görsel eklendi"
  if (/date/i.test(key) && !Number.isNaN(Date.parse(value))) return new Date(value).toLocaleDateString("tr-TR")
  return typeof value === "object" ? JSON.stringify(value) : String(value)
}
export function logSummary(log) {
  const changes = changesFor(log)
  const change = changes.find(c => /price/i.test(c.key)) || changes.find(c => /isactive/i.test(c.key)) || changes[0]
  if (!change) return "İşlem ayrıntılarını görüntüle"
  return `${change.label}: ${valueText(change.old,change.key)} → ${valueText(change.next,change.key)}`
}
export default function LogChanges({ log }) {
  const changes = changesFor(log)
  return <>
    <div className="readable-changes">
      <div className="change-head"><span>Değişen alan</span><span>Önceki değer</span><span>Yeni değer</span></div>
      {changes.length ? changes.map(c => <div className="change-row" key={c.key}><strong>{c.label}</strong><span><small>Önceki</small>{valueText(c.old,c.key)}</span><span><small>Yeni</small>{valueText(c.next,c.key)}</span></div>) : <p>Bu işlem için alan değişikliği bulunmuyor.</p>}
    </div>
    <details className="technical-log"><summary>Teknik kayıt ayrıntıları</summary><div className="diff"><div><h3>Önceki değerler</h3><pre>{safeJson(log.old)}</pre></div><div><h3>Yeni değerler</h3><pre>{safeJson(log.next)}</pre></div></div></details>
  </>
}
