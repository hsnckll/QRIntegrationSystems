import React, { useState } from "react"
import QRCode from "qrcode"
import { Icon, Button, Badge, Field, Thumb } from "../../../shared/components"
import { money, date, today, currentSub, days, photoData } from "../repository"

export default function BusinessForm({
  item,
  templates,
  businesses,
  onSave,
  inline,
}) {
  const [x, setX] = useState(
    item || {
      id: crypto.randomUUID(),
      name: "",
      slug: "",
      owner: "",
      email: "",
      phone: "",
      address: "",
      template: "",
      active: true,
      deleted: false,
      logo: "",
      banner: "",
      createdAt: today(),
      subscriptionPlan: "3",
      subscriptionPrice: "2000",
      subscriptionMethod: "Havale",
      subscriptionNote: "İlk açılış paketi",
    },
  )
  const [error, setError] = useState("")
  const set = (k, v) => setX((prev) => ({ ...prev, [k]: v }))
  const dirty = JSON.stringify(item) !== JSON.stringify(x)

  return (
    <section className={inline == true ? "surface form-surface" : ""}>
      <form
        onSubmit={(e) => {
          e.preventDefault()
          if (businesses.some((b) => b.id !== x.id && b.slug === x.slug)) {
            setError("Bu menü adresi başka bir işletmeye ait.")
            return
          }
          if (Boolean(x.name.trim()) == false || Boolean(x.owner.trim()) == false) {
            setError("İşletme ve yetkili adı gerekli.")
            return
          }
          setError("")
          onSave(x)
        }}
      >
        <div className="form-body">
          <div className="form-grid">
            {[
              ["name", "İşletme adı"],
              ["owner", "İşletme sahibi"],
              ["email", "E-posta"],
              ["phone", "Telefon"],
            ].map(([k, label]) => (
              <Field
                key={k}
                label={label}
                value={x[k]}
                required={k !== "phone"}
                type={k === "email" ? "email" : "text"}
                disabled={item != null && k === "email"}
                onChange={(e) => set(k, e.target.value)}
              />
            ))}
          </div>
          <Field label="Adres">
            <textarea
              value={x.address}
              onChange={(e) => set("address", e.target.value)}
            />
          </Field>
          <Field
            label="Menü adresi (slug)"
            required
            pattern="[a-z0-9]+(?:-[a-z0-9]+)*"
            value={x.slug}
            disabled={item != null}
            onChange={(e) => set("slug", e.target.value)}
            placeholder="ornek-restoran"
          />
          <p className="muted small">
            {item != null
              ? "E-posta ve menü adresi güvenlik gereği oluşturulduktan sonra değiştirilemez."
              : "Küçük harf, rakam ve tire kullanın. E-posta, işletme sahibinin giriş kodunu alacağı adrestir."}
          </p>
          {Boolean(item) == false && (
            <>
              <Field label="Menü şablonu (İsteğe bağlı)">
                <select
                  value={x.template || ""}
                  onChange={(e) => set("template", e.target.value)}
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

              <div
                style={{
                  marginTop: "18px",
                  marginBottom: "8px",
                  borderTop: "1px solid var(--border-color, #e5e7eb)",
                  paddingTop: "14px",
                }}
              >
                <strong
                  style={{
                    display: "block",
                    marginBottom: "10px",
                    fontSize: "14px",
                  }}
                >
                  Abonelik Başlangıç Paketi
                </strong>
                <div className="form-grid">
                  <Field label="Başlangıç paketi">
                    <select
                      value={x.subscriptionPlan || "3"}
                      onChange={(e) => {
                        const val = e.target.value
                        let price = "1800"
                        let method = "Havale"
                        if (val === "3") {
                          price = "2000"
                          method = "Havale"
                        } else if (val === "6") {
                          price = "4000"
                          method = "Havale"
                        } else if (val === "12") {
                          price = "7500"
                          method = "Kredi kartı"
                        }
                        setX((prev) => ({
                          ...prev,
                          subscriptionPlan: val,
                          subscriptionPrice: price,
                          subscriptionMethod: method,
                        }))
                      }}
                    >
                      <option value="3">3 Aylık Paket</option>
                      <option value="6">6 Aylık Paket</option>
                      <option value="12">1 Yıllık Paket (Tavsiye edilen)</option>
                    </select>
                  </Field>

                  <Field
                    label="Ödenen tutar (TL)"
                    type="number"
                    min="0"
                    value={x.subscriptionPrice ?? "0"}
                    onChange={(e) => set("subscriptionPrice", e.target.value)}
                  />
                </div>

                <div className="form-grid">
                  <Field label="Ödeme yöntemi">
                    <select
                      value={x.subscriptionMethod || "Havale"}
                      onChange={(e) => set("subscriptionMethod", e.target.value)}
                    >
                      <option value="Havale">Havale</option>
                      <option value="Kredi kartı">Kredi kartı</option>
                      <option value="Nakit">Nakit</option>
                      <option value="Ücretsiz / Deneme">Ücretsiz / Deneme</option>
                      <option value="Diğer">Diğer</option>
                    </select>
                  </Field>

                  <Field
                    label="Abonelik notu"
                    value={x.subscriptionNote || ""}
                    onChange={(e) => set("subscriptionNote", e.target.value)}
                    placeholder="Örn: İlk açılış paketi"
                  />
                </div>
              </div>
            </>
          )}
          {Boolean(error) == true && <p className="error">{error}</p>}
        </div>
        <div className="modal-actions">
          {inline == true && (
            <Button
              type="button"
              disabled={dirty == false}
              onClick={() => setX(item)}
            >
              Vazgeç
            </Button>
          )}
          <Button
            kind="primary"
            type="submit"
            disabled={inline == true && dirty == false}
          >
            {Boolean(item) == true ? "Bilgileri kaydet" : "İşletmeyi oluştur"}
          </Button>
        </div>
      </form>
    </section>
  )
}
