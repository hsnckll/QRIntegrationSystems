import React from "react"
import { createRoot } from "react-dom/client"
import Login from "../features/auth/Login"
import ErrorBoundary from "../shared/ErrorBoundary"
import { getSubdomainSlug } from "../shared/session"
import "../features/auth/style.css"
import "../shared/styles/branding.css"
import "../shared/styles/refinements.css"

if (Boolean(getSubdomainSlug()) == true) {
  import("./menu.jsx")
} else {
  createRoot(document.getElementById("root")).render(
    <ErrorBoundary>
      <Login />
    </ErrorBoundary>,
  )
}
