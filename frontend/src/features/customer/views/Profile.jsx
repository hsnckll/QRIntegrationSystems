import React, { useState, useEffect } from "react"
import { Icon, Button, Field } from "../../../shared/components"

export default function Profile({ data, commit, setDirty }) {
  const [draft, setDraft] = useState(data.business)
  const [saving, setSaving] = useState(false)
  const dirty = JSON.stringify(draft) !== JSON.stringify(data.business)

  useEffect(() => {
    setDraft(data.business)
  }, [data.business])

  useEffect(() => {
    setDirty(dirty)
  }, [dirty, setDirty])

  return (
    <div className="profile-layout">
      <section className="surface">
        <div className="section-header">
          <span className="section-symbol">
            <Icon name="user" />
          </span>
          <div>
            <h2>İletişim ve işletme bilgileri</h2>
            <p>Müşterilerinizin size ulaşabileceği bilgiler.</p>
          </div>
        </div>
        <form
          onSubmit={async (e) => {
            e.preventDefault()
            if (saving) return
            setSaving(true)
            try { await commit({ ...data, business: draft }, "İşletme bilgileri kaydedildi") }
            finally { setSaving(false) }
          }}
        >
          <div className="form-body">
            <div className="form-grid">
              {[
                ["owner", "İşletme sahibi"],
                ["name", "İşletme adı"],
                ["phone", "Telefon"],
                ["email", "E-posta"],
              ].map(([key, label]) => (
                <Field
                  key={key}
                  label={label}
                  type={
                    key === "email" ? "email" : key === "phone" ? "tel" : "text"
                  }
                  required
                  disabled={saving}
                  value={draft[key] || ""}
                  onChange={(e) =>
                    setDraft({ ...draft, [key]: e.target.value })
                  }
                />
              ))}
            </div>
            <p className="muted">E-posta adresinizi değiştirirseniz yeni adresinizle yeniden giriş yapmanız gerekir. Sonraki giriş kodları yeni adresinize gönderilir.</p>
            <Field label="Adres">
              <textarea
                rows={3}
                value={draft.address || ""}
                onChange={(e) =>
                  setDraft({ ...draft, address: e.target.value })
                }
              />
            </Field>
            <Field
              label="Menü adresiniz"
              value={`/menu/${draft.slug || ""}`}
              readOnly
            />
            <p className="muted small">
              Menü adresi sabittir; bilgileriniz değişse de QR kodunuz aynı
              kalır.
            </p>
          </div>
          <div className="modal-actions">
            <Button
              type="button"
              disabled={saving || dirty == false}
              onClick={() => setDraft(data.business)}
            >
              Vazgeç
            </Button>
            <Button kind="primary" type="submit" disabled={saving || dirty == false}>
              {saving ? "Kaydediliyor…" : "Değişiklikleri kaydet"}
            </Button>
          </div>
        </form>
      </section>
      <aside className="side-explainer">
        <Icon name="help" size={26} />
        <h3>Güncel bilgiler, kolay iletişim.</h3>
        <p>
          Telefon numaranızı ve adresinizi kontrol ederek müşterilerinizin size
          kolayca ulaşmasını sağlayın.
        </p>
        <hr />
        <p>
          Logo ve banner işletmenizin mevcut tasarımının bir parçasıdır. Bu
          panelden değiştirilemez.
        </p>
      </aside>
    </div>
  )
}
