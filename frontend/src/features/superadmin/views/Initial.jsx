import React, { useState } from "react"
import QRCode from "qrcode"
import { Icon, Button, Badge, Field, Thumb } from "../../../shared/components"
import { money, date, today, currentSub, days, photoData } from "../repository"

export default function Initial({ b }) {
  return b.logo ? (
    <img className="business-avatar" src={b.logo} alt="" />
  ) : (
    <span
      className="business-avatar"
      style={{
        background: ["#edf2ff", "#eaf5f5", "#f4efe9"][
          String(b.id).charCodeAt(0) % 3
        ],
      }}
    >
      {b.name
        .split(" ")
        .map((s) => s[0])
        .slice(0, 2)
        .join("")}
    </span>
  )
}
