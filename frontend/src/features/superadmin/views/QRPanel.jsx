import React, { useState } from "react"
import { Icon, Button, Badge } from "../../../shared/components"
import { buildPublicMenuUrl, getRootDomain } from "../../../shared/session"
import { date } from "../repository"
import Empty from "./Empty"

export default function QRPanel({
  d,
  businesses,
  onCreate,
  onToggle,
  onError,
  readOnly = false,
}) {
  const [id, setId] = useState("all")
  // <select> değeri her zaman metin döner; id'ler sayı olabileceği için metin olarak karşılaştırıyoruz
  const valid =
    id === "all" || businesses.some((b) => String(b.id) === String(id)) == true
      ? id
      : "all"
  const rows = d.qrs.filter(
    (q) =>
      businesses.some((b) => b.id === q.business) == true &&
      (valid === "all" || String(q.business) === String(valid)),
  )

  const isLocal = window.location.hostname === "localhost" || window.location.hostname === "127.0.0.1"

  return (
    <>
      <div className="list-toolbar standalone">
        <select
          aria-label="QR işletme filtresi"
          value={valid}
          onChange={(e) => setId(e.target.value)}
        >
          <option value="all">Tüm işletmeler</option>
          {businesses.map((b) => (
            <option value={b.id} key={b.id}>
              {b.name}
            </option>
          ))}
        </select>
        {readOnly === false && (
          <Button
            kind="primary"
            icon="plus"
            disabled={businesses.length === 0}
            onClick={() =>
              onCreate(
                businesses.length === 1
                  ? businesses[0].id
                  : valid === "all"
                    ? undefined
                    : valid,
              )
            }
          >
            QR kodu oluştur
          </Button>
        )}
      </div>
      <div className="info-strip">
        <Icon name="help" />
        <p>
          {readOnly === true
            ? "İşletmenizin QR kodunu indirip masalara yerleştirebilirsiniz. Baskıdan önce menü bağlantısını kontrol edin."
            : "QR kodlarını indirmeden önce hedef menü bağlantısını kontrol edin."}
        </p>
      </div>
      <div className="qr-grid">
        {rows.map((q) => {
          const b = businesses.find((item) => item.id === q.business)
          const menuUrl = buildPublicMenuUrl(b?.slug, q.url)
          const displayUrl =
            Boolean(b?.slug) == true &&
            (Boolean(q.url) == false || q.url.includes("/menu.html") == true)
              ? `https://${encodeURIComponent(b.slug.trim().toLowerCase())}.${getRootDomain()}`
              : q.url

          return (
            <section className="surface qr-card" key={q.id}>
              <div className="section-top">
                <h2>{b?.name || "İşletme"}</h2>
                <Badge active={q.active} />
              </div>
              <img src={q.image} alt={`${q.label} QR kodu`} />
              <h3>{q.label}</h3>
              <a
                className="qr-url"
                href={menuUrl}
                target="_blank"
                rel="noreferrer"
                title={isLocal == true ? "Yerel menüyü görüntüle" : "Menü adresini aç"}
              >
                {displayUrl}
              </a>
              {isLocal == true && (
                <span style={{ fontSize: "11px", color: "#64748b", display: "block", marginTop: "2px", fontWeight: "500" }}>
                  (Yerel ortam: Tıklandığında test menüsü açılır)
                </span>
              )}
              <small style={{ marginTop: "4px" }}>{date(q.createdAt)} tarihinde oluşturuldu</small>
              <div className="qr-actions">
                <a
                  className="button"
                  href={menuUrl}
                  target="_blank"
                  rel="noreferrer"
                  style={{ textDecoration: "none", display: "inline-flex", alignItems: "center", gap: "6px" }}
                  title="Menüyü tarayıcıda canlı görüntüleyin"
                >
                  <Icon name="eye" size={14} />
                  Menüyü Aç
                </a>
                <a
                  className="button primary"
                  href={q.image}
                  download={`${q.label.replace(/[^a-zA-Z0-9-]/g, "-")}.png`}
                >
                  PNG indir
                </a>
                <Button
                  onClick={async () => {
                    try {
                      await navigator.clipboard.writeText(displayUrl)
                      onError("QR bağlantısı kopyalandı")
                    } catch {
                      onError(
                        "Bağlantı kopyalanamadı; yukarıdaki adresi seçip kopyalayın.",
                      )
                    }
                  }}
                >
                  Bağlantıyı kopyala
                </Button>
                {readOnly === false && (
                  <Button onClick={() => onToggle(q)}>
                    {q.active === true ? "Pasif yap" : "Aktif yap"}
                  </Button>
                )}
              </div>
            </section>
          )
        })}
      </div>
      {rows.length === 0 && (
        <section className="surface">
          <Empty text="Henüz QR kodu tanımlanmadı" />
        </section>
      )}
    </>
  )
}
