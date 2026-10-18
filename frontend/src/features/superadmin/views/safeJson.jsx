import React, { useState } from "react"
import QRCode from "qrcode"
import { Icon, Button, Badge, Field, Thumb } from "../../../shared/components"
import { money, date, today, currentSub, days, photoData } from "../repository"

export default function safeJson(v) {
  return (
    JSON.stringify(
      v,
      (k, val) =>
        typeof val === "string" && val.startsWith("data:image")
          ? "[Görsel dosyası]"
          : val,
      2,
    ) || "—"
  )
}
