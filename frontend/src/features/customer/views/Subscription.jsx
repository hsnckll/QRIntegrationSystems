import React, { useState } from "react"
import { Icon, Badge, Button, Modal } from "../../../shared/components"
import { date } from "../repository"

function RenewalRequest({ business }) {
  const [open, setOpen] = useState(false), [feedback, setFeedback] = useState("")
  const message = `Merhaba, ${business?.name || "işletmem"} için abonelik yenileme bilgisi almak istiyorum. İşletme adresi: ${business?.slug || "Belirtilmedi"}`
  const support = import.meta.env.VITE_SUPPORT_URL
  const safeSupport = support && /^(https:\/\/|mailto:|tel:)/.test(support) ? support : null
  return <div className="renewal-action"><Button kind="primary" onClick={() => { setFeedback(""); setOpen(true) }}>Yenileme talebi hazırla</Button>
    {open && <Modal title="Aboneliğinizi yenileyin" onClose={() => setOpen(false)}><div className="modal-body"><p>Talep metnini kopyalayıp işletmenizin sistem yöneticisine iletin.</p><textarea className="renewal-message" aria-label="Yenileme talebi" readOnly value={message} rows={4} />{feedback && <p role="status">{feedback}</p>}</div><div className="modal-actions"><Button onClick={() => setOpen(false)}>Kapat</Button>{safeSupport && <a className="button" href={safeSupport} target="_blank" rel="noopener noreferrer">İletişime geç</a>}<Button kind="primary" onClick={async () => { try { await navigator.clipboard.writeText(message); setFeedback("Talep metni kopyalandı. Sistem yöneticinize iletebilirsiniz.") } catch { setFeedback("Metni seçerek kopyalayabilirsiniz.") } }}>Talep metnini kopyala</Button></div></Modal>}
  </div>
}
export default function Subscription({ subscription, business }) {
  if (!subscription?.start || !subscription?.end) return <section className="surface subscription"><h2>Abonelik bilgisi bulunmuyor</h2><p>Menünüz açık kalır. Abonelik bilgisi için sistem yöneticisine başvurun.</p><RenewalRequest business={business} /></section>
  const now = Date.now(),
    start = +new Date(subscription?.start || now),
    end = +new Date(subscription?.end || now),
    remaining = Math.max(0, Math.ceil((end - now) / 86400000)),
    future = now < start,
    expired = now >= end
  const progress = Math.max(
    0,
    Math.min(100, ((now - start) / (end - start)) * 100),
  )
  return (
    <div className="subscription-layout">
      <section className="surface subscription">
        <div className="subscription-top">
          <span className="section-symbol">
            <Icon name="calendar" size={28} />
          </span>
          <Badge active={expired == false && future == false}>
            {future == true
              ? "Henüz başlamadı"
              : expired == true
                ? "Süresi doldu"
                : remaining <= 14
                  ? "Yakında bitiyor"
                  : "Aktif abonelik"}
          </Badge>
        </div>
        <h2>Abonelik süreniz</h2>
        <p className="muted">Mevcut aboneliğinizin süre bilgileri</p>
        <div className="days">
          <strong>
            {future == true ? Math.ceil((start - now) / 86400000) : remaining}
          </strong>
          <span>
            {future == true
              ? "gün sonra başlayacak"
              : expired == true
                ? "gün kaldı · Aboneliğiniz sona erdi"
                : "gün kaldı"}
          </span>
        </div>
        <div
          className="progress"
          role="progressbar"
          aria-label="Geçen abonelik süresi"
          aria-valuemin={0}
          aria-valuemax={100}
          aria-valuenow={Math.round(progress)}
        >
          <span style={{ width: `${progress}%` }} />
        </div>
        <div className="date-pair">
          <div>
            <span>Başlangıç tarihi</span>
            <strong>{date(subscription?.start)}</strong>
          </div>
          <div>
            <span>Bitiş tarihi</span>
            <strong>{date(subscription?.end)}</strong>
          </div>
        </div>
        {remaining <= 14 && future == false && (
          <p className="subscription-alert">
            {expired == true
              ? "Abonelik süreniz doldu. Menünüz açık kalır; yenileme için sistem yöneticisine başvurun."
              : "Aboneliğinizin bitiş tarihi yaklaşıyor."}
          </p>
        )}
        <RenewalRequest business={business} />
      </section>
      <aside className="side-explainer">
        <Icon name="help" size={26} />
        <h3>Abonelik bilgileri</h3>
        <p>
          Aboneliğinizin başlangıç ve bitiş tarihlerini bu sayfadan takip
          edebilirsiniz.
        </p>
        <p>Kalan süre her gün otomatik olarak hesaplanır.</p>
      </aside>
    </div>
  )
}
