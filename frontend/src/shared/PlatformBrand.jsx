import React from "react"
import logo from "../assets/branding/okut-ye-logo.webp"

export default function PlatformBrand({ subtitle = "İşletmenizin dijital menüsü" }) {
  return (
    <span className="platform-brand">
      <img
        src={logo}
        alt="Okut Ye"
        width="2172"
        height="724"
        loading="eager"
        fetchPriority="high"
        decoding="async"
      />
      <span>{subtitle}</span>
    </span>
  )
}
