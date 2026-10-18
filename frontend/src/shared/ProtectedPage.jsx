import React, { useEffect, useState } from "react"
import { readSession, landingFor, routes } from "./session"

// allowApi: Bu sayfa gerçek backend verisiyle çalışabiliyorsa true verilir.
// (SuperAdmin paneli bağlandı → true. İşletme paneli bağlandı → true.)
export default function ProtectedPage({ role, allowApi = false, children }) {
  const [s, setS] = useState(readSession)
  useEffect(() => {
    function check() {
      const next = readSession()
      setS(next)
      if (Boolean(next) == false) location.replace(routes.login)
      else if (next.role !== role) location.replace(landingFor(next.role))
    }
    check()
    const interval = setInterval(check, 15000)
    window.addEventListener("pageshow", check)
    window.addEventListener("focus", check)
    return () => {
      clearInterval(interval)
      window.removeEventListener("pageshow", check)
      window.removeEventListener("focus", check)
    }
  }, [role])

  if (Boolean(s) == false || s.role !== role)
    return <p role="status">Giriş ekranına yönlendiriliyor…</p>
  if (s.mode === "api" && allowApi === true) return children
  if (s.mode !== "api" || !allowApi)
    return (
      <div className="signed-out">
        <section className="surface">
          <h1>Veri bağlantısı gerekli</h1>
          <p>
            Bu sayfaya erişilemiyor. Lütfen yeniden giriş yapın.
          </p>
          <a href={routes.login}>Giriş ekranına dön</a>
        </section>
      </div>
    )
  return null
}
