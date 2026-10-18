import React, { useState } from "react"
import QRCode from "qrcode"
import { Icon, Button, Badge, Field, Thumb } from "../../../shared/components"
import { money, date, today, currentSub, days, photoData } from "../repository"

export default function DesignEditor({ b, templates, onSave }) {
  const [x, setX] = useState(b),
    [busy, setBusy] = useState(false),
    [error, setError] = useState("")
  return (
    <section className="surface form-surface">
      <div className="form-body">
        <h2>İşletmeye özel görünüm</h2>
        <p className="muted">
          Logo ve tasarım yalnızca SuperAdmin tarafından değiştirilir.
        </p>
        <div className="asset-fields">
          <div className="asset-field">
            <label>İşletme logosu</label>
            <div className="asset-preview logo">
              {Boolean(x.logo) == true ? (
                <img src={x.logo} alt="İşletme logosu" />
              ) : (
                <Icon name="eye" size={30} />
              )}
            </div>
            <input
              aria-label="İşletme logosu"
              type="file"
              accept="image/png,image/jpeg,image/webp"
              disabled={busy}
              onChange={async (e) => {
                const file = e.target.files[0]
                if (Boolean(file) == false) return
                setBusy(true)
                setError("")
                try {
                  const image = await photoData(file)
                  setX((prev) => ({ ...prev, logo: image }))
                } catch (err) {
                  setError(err.message)
                } finally {
                  setBusy(false)
                }
              }}
            />
            {Boolean(x.logo) == true && (
              <button
                className="text-button"
                onClick={() => setX({ ...x, logo: "" })}
              >
                Görseli kaldır
              </button>
            )}
          </div>
        </div>
        <Field label="Menü şablonu">
          <select
            value={x.template || ""}
            onChange={(e) => setX((prev) => ({ ...prev, template: e.target.value }))}
            aria-label="Menü şablonu"
          >
            <option value="">Şablon seçilmedi — menü yayında değil</option>
            {templates.filter(t=>t.entryUrl).map(t => (
              <option key={t.id} value={t.id}>
                {t.name}
              </option>
            ))}
          </select>
        </Field>
        <p className="muted small">İşletme menüsü seçtiğiniz şablon tasarımı ile yayınlanacaktır.</p>
        {Boolean(error) == true && <p className="error">{error}</p>}
        <p className="muted small">
          JPEG, PNG, WebP · En fazla 8 MB. Görseller kaydettiğinizde sunucuya
          yüklenir.
        </p>
      </div>
      <div className="modal-actions">
        <Button
          kind="primary"
          disabled={busy == true || JSON.stringify(x) === JSON.stringify(b)}
          onClick={() => onSave(x)}
        >
          {busy == true ? "Görsel hazırlanıyor…" : "Tasarımı kaydet"}
        </Button>
      </div>
    </section>
  )
}
