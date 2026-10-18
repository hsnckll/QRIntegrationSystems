import React, { useState } from "react"
import QRCode from "qrcode"
import { Icon, Button, Badge, Field, Thumb } from "../../../shared/components"
import { money, date, today, currentSub, days, photoData } from "../repository"

export default function Empty({ text }) {
  return (
    <div className="empty">
      <Icon name="box" size={30} />
      <h3>{text}</h3>
      <p>Yeni bir kayıt ekleyebilir veya filtrenizi değiştirebilirsiniz.</p>
    </div>
  )
}
