import PlatformBrand from "../../shared/PlatformBrand"
import React from "react"
const paths = {
  arrow: "M5 12h14m-5-5 5 5-5 5",
  back: "M19 12H5m5-5-5 5 5 5",
  mail: "M3 5h18v14H3zm0 0 9 8 9-8",
  lock: "M5 10h14v11H5zM8 10V6a4 4 0 0 1 8 0v4",
  eye: "M2 12s4-7 10-7 10 7 10 7-4 7-10 7S2 12 2 12m13 0a3 3 0 1 1-6 0 3 3 0 0 1 6 0",
  store: "M4 10v11h16V10M3 4h18l1 6H2zM9 21v-7h6v7",
  shield: "m12 2 9 4v6c0 5-9 10-9 10S3 17 3 12V6zm-4 10 3 3 5-6",
  check: "m5 12 4 4L19 6",
  help: "M9 9a3 3 0 1 1 5 2c-2 1-2 2-2 3M12 17h.01M22 12a10 10 0 1 1-20 0 10 10 0 0 1 20 0",
  chart: "M4 3v17h17M8 16v-5M13 16V7M18 16V4",
  menu: "M4 6h16M4 12h16M4 18h16",
}
export function Icon({ name, size = 20 }) {
  return (
    <svg
      width={size}
      height={size}
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.7"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
    >
      <path d={paths[name] || paths.help} />
    </svg>
  )
}
export function Brand() { return <PlatformBrand /> }
export function Button({ children, busy = false, disabled = false, ...props }) {
  return (
    <button className="primary" disabled={busy || disabled} {...props}>
      {busy ? (
        <>
          <span className="spinner" /> Lütfen bekleyin…
        </>
      ) : (
        children
      )}
    </button>
  )
}
export function ErrorMessage({ message }) {
  return message ? (
    <div className="error" role="alert">
      <Icon name="help" size={18} />
      <span>{message}</span>
    </div>
  ) : null
}
export function EmailField({ value, onChange, disabled }) {
  return (
    <label className="field">
      <span>E-posta adresi</span>
      <span className="input-wrap">
        <Icon name="mail" />
        <input
          name="email"
          type="email"
          autoComplete="email"
          required
          placeholder="ornek@isletmeniz.com"
          value={value}
          disabled={disabled}
          onChange={(e) => onChange(e.target.value)}
          maxLength={254}
        />
      </span>
    </label>
  )
}
export function Back({ onClick, children = "Giriş seçenekleri" }) {
  return (
    <button type="button" className="back" onClick={onClick}>
      <Icon name="back" size={17} />
      {children}
    </button>
  )
}
