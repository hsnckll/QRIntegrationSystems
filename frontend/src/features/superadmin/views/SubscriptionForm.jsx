import React, { useState } from "react"
import QRCode from "qrcode"
import { Icon, Button, Badge, Field, Thumb } from "../../../shared/components"
import { money, date, today, currentSub, days, photoData } from "../repository"

export default function SubscriptionForm({
  item,
  business,
  businesses,
  onSave,
}) {
  const [x, setX] = useState(
      item || {
        id: crypto.randomUUID(),
        business: business || businesses[0]?.id,
        start: today(),
        end: today(),
        amount: "",
        method: "Havale",
        note: "",
        deleted: false,
      },
    ),
    [error, setError] = useState("")
  const set = (key, val) => setX({ ...x, [key]: val })
  return (
    <form
      onSubmit={(e) => {
        e.preventDefault()
        if (x.end <= x.start) {
          setError("Bitiş tarihi başlangıçtan sonra olmalı.")
          return
        }
        onSave({ ...x, amount: x.amount === "" ? "" : Number(x.amount) })
      }}
    >
      <div className="modal-body">
        <Field label="İşletme">
          <select
            required
            disabled={!!item || !!business}
            value={x.business}
            onChange={(e) => set("business", e.target.value)}
          >
            {businesses.map((b) => (
              <option key={b.id} value={b.id}>
                {b.name}
              </option>
            ))}
          </select>
        </Field>
        <div className="form-grid">
          <Field
            label="Başlangıç tarihi"
            type="date"
            value={x.start}
            required
            onChange={(e) => set("start", e.target.value)}
          />
          <Field
            label="Bitiş tarihi"
            type="date"
            value={x.end}
            required
            onChange={(e) => set("end", e.target.value)}
          />
        </div>
        <div className="form-grid">
          <Field
            label="Ödenen tutar (TL)"
            type="number"
            min="0"
            step="0.01"
            value={x.amount}
            onChange={(e) => set("amount", e.target.value)}
          />
          <Field label="Ödeme yöntemi">
            <select
              value={x.method}
              onChange={(e) => set("method", e.target.value)}
            >
              <option>Havale</option>
              <option>Kredi kartı</option>
              <option>Nakit</option>
              <option>Diğer</option>
            </select>
          </Field>
        </div>
        <Field label="Not">
          <textarea
            value={x.note}
            onChange={(e) => set("note", e.target.value)}
          />
        </Field>
        {error && <p className="error">{error}</p>}
      </div>
      <div className="modal-actions">
        <Button kind="primary" type="submit" disabled={!businesses.length}>
          Aboneliği kaydet
        </Button>
      </div>
    </form>
  )
}
