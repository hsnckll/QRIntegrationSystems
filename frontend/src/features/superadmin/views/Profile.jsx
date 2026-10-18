import React, { useState } from "react"
import QRCode from "qrcode"
import { Icon, Button, Badge, Field, Thumb } from "../../../shared/components"
import { money, date, today, currentSub, days, photoData } from "../repository"

export default function Profile({ profile, onSave }) {
  const [x, setX] = useState(profile)
  return (
    <section className="surface form-surface">
      <form
        onSubmit={(e) => {
          e.preventDefault()
          onSave(x)
        }}
      >
        <div className="form-body">
          <Field
            label="Ad soyad"
            required
            value={x.name}
            onChange={(e) => setX({ ...x, name: e.target.value })}
          />
          <Field
            label="E-posta"
            type="email"
            required
            value={x.email}
            onChange={(e) => setX({ ...x, email: e.target.value })}
          />
        </div>
        <div className="modal-actions">
          <Button
            kind="primary"
            disabled={JSON.stringify(x) === JSON.stringify(profile)}
          >
            Bilgileri kaydet
          </Button>
        </div>
      </form>
    </section>
  )
}
