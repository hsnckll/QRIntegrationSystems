import React from "react"
import { createRoot } from "react-dom/client"
import SuperAdminDashboard from "../features/superadmin/SuperAdminDashboard"
import ProtectedPage from "../shared/ProtectedPage"
import ErrorBoundary from "../shared/ErrorBoundary"
import "../shared/styles/dashboard.css"
import "../features/superadmin/style.css"
createRoot(document.getElementById("root")).render(
  <ErrorBoundary>
    <ProtectedPage role="SuperAdmin" allowApi={true}>
      <SuperAdminDashboard />
    </ProtectedPage>
  </ErrorBoundary>,
)

import "../shared/styles/branding.css"

import "../shared/styles/refinements.css"
