import React from "react"
import PlatformBrand from "./PlatformBrand"
import "./styles/loading-dashboard.css"

export default function LoadingDashboard({ title = "Yönetim merkezi", navigationCount = 6 }) {
  return (
    <div className="app panel-loading" aria-busy="true" aria-label={`${title} yükleniyor`}>
      <aside className="sidebar" aria-hidden="true">
        <PlatformBrand subtitle={title} />
        <div className="skeleton skeleton-profile" />
        <div className="skeleton-nav">
          {Array.from({ length: navigationCount }, (_, i) => <div className="skeleton" key={i} />)}
        </div>
      </aside>
      <div className="workspace">
        <header className="topbar">
          <strong>{title}</strong>
          <span className="muted small" role="status">Panel hazırlanıyor…</span>
        </header>
        <main>
          <div className="skeleton skeleton-heading" aria-hidden="true" />
          <div className="loading-metrics" aria-hidden="true">
            {Array.from({ length: 4 }, (_, i) => (
              <div className="surface skeleton-card" key={i}><div className="skeleton" /><div className="skeleton" /></div>
            ))}
          </div>
          <div className="surface skeleton-content" aria-hidden="true">
            {Array.from({ length: 5 }, (_, i) => <div className="skeleton" key={i} />)}
          </div>
        </main>
      </div>
    </div>
  )
}
