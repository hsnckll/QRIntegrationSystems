import React, { useState } from "react"
import QRCode from "qrcode"
import { Icon, Button, Badge, Field, Thumb } from "../../../shared/components"
import { money, date, today, currentSub, days, photoData } from "../repository"

export default function Metric({ icon, label, value, note, tone = "blue" }) {
  return (
    <div>
      <div className="metric-label">
        <span>{label}</span>
        <span className={`overview-icon ${tone}`}>
          <Icon name={icon} />
        </span>
      </div>
      <strong>{value}</strong>
      <small>{note}</small>
    </div>
  )
}
