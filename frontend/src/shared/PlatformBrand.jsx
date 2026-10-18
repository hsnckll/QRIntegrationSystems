import React from "react"
import logo from "../assets/branding/okut-ye-logo.png"

export default function PlatformBrand({ subtitle = "İşletmenizin dijital menüsü" }) {
  return <span className="platform-brand"><img src={logo} alt="Okut Ye" width="2172" height="724" /><span>{subtitle}</span></span>
}
