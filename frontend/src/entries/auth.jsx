import React from "react"
import { createRoot } from "react-dom/client"
import Login from "../features/auth/Login"
import ErrorBoundary from "../shared/ErrorBoundary"
import "../features/auth/style.css"
createRoot(document.getElementById("root")).render(
  <ErrorBoundary>
    <Login />
  </ErrorBoundary>,
)
import "../shared/styles/branding.css"

import "../shared/styles/refinements.css"
