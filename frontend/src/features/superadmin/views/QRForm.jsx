import React, { useState } from "react"
import QRCode from "qrcode"
import { Icon, Button, Badge, Field, Thumb } from "../../../shared/components"
import { money, date, today, currentSub, days, photoData } from "../repository"

export default function QRForm({ businesses, business, onSave }) {
  const [id, setId] = useState(business || businesses[0]?.id),
    [label, setLabel] = useState("Masa QR"),
    [busy, setBusy] = useState(false),
    [error, setError] = useState("")
  return (
    <form
      onSubmit={async (e) => {
        e.preventDefault()
        setBusy(true)
        setError("")
        try {
          const qrId = crypto.randomUUID(),
            url = new URL(`/menu.html?qr=${qrId}`, location.origin).href,
            image = await QRCode.toDataURL(url, {
              width: 768,
              margin: 4,
              errorCorrectionLevel: "M",
            })
          onSave({
            id: qrId,
            business: id,
            label: label.trim(),
            url,
            image,
            active: true,
            createdAt: new Date().toISOString(),
          })
        } catch {
          setError("QR oluşturulamadı. Tekrar deneyin.")
        } finally {
          setBusy(false)
        }
      }}
    >
      <div className="modal-body">
        <Field label="İşletme">
          <select
            required
            value={id}
            disabled={!!business}
            onChange={(e) => setId(e.target.value)}
          >
            {businesses.map((b) => (
              <option value={b.id} key={b.id}>
                {b.name}
              </option>
            ))}
          </select>
        </Field>
        <Field
          label="QR adı"
          value={label}
          required
          pattern=".*\S.*"
          onChange={(e) => setLabel(e.target.value)}
          maxLength={80}
        />
        <p className="muted small">
          Kod, seçilen işletmeye atanır. Menü bağlantısını baskıdan önce kontrol edin.
        </p>
        {error && <p className="error">{error}</p>}
      </div>
      <div className="modal-actions">
        <Button kind="primary" disabled={busy || !id} type="submit">
          {busy ? "Üretiliyor…" : "Oluştur ve işletmeye ata"}
        </Button>
      </div>
    </form>
  )
}
