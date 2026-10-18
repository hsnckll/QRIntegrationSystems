import React from "react"
import { createRoot } from "react-dom/client"
import CustomerDashboard from "../features/customer/CustomerDashboard"
import ProtectedPage from "../shared/ProtectedPage"
import ErrorBoundary from "../shared/ErrorBoundary"
import "../shared/styles/dashboard.css"
createRoot(document.getElementById("root")).render(
  <ErrorBoundary>
    <ProtectedPage role="Business" allowApi={true}>
      <CustomerDashboard />
    </ProtectedPage>
  </ErrorBoundary>,
)

import "../shared/styles/branding.css"

import "../shared/styles/refinements.css"
