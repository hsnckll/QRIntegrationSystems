import brandBanner from "../../assets/branding/okut-ye-banner.webp"
import { completeLogin, readSession, landingFor } from "../../shared/session"
import React, { useState, useEffect, useRef } from "react"
import { Brand, Icon } from "./components"
import Choice from "./pages/Choice"
import Admin from "./pages/Admin"
import Business from "./pages/Business"
const route = () =>
  ["admin", "business"].includes(location.hash.slice(1))
    ? location.hash.slice(1)
    : "choice"
export default function Login() {
  const [page, setPage] = useState(route)
  const mainRef = useRef(null)
  useEffect(() => {
    const redirect = () => { const session = readSession(); if (session) location.replace(landingFor(session.role)) }
    redirect(); window.addEventListener("pageshow", redirect)
    return () => window.removeEventListener("pageshow", redirect)
  }, [])

  useEffect(() => {
    const listener = () => {
      setPage(route())
    }
    window.addEventListener("hashchange", listener)
    return () => window.removeEventListener("hashchange", listener)
  }, [])

  useEffect(() => {
    document.title = `${
      page === "admin"
        ? "Yönetici girişi"
        : page === "business"
          ? "İşletme girişi"
          : "Giriş"
    } · Okut Ye`
    mainRef.current?.focus()
  }, [page])

  function navigate(next) {
    setPage(next)
    location.hash = next === "choice" ? "" : next
  }

  if (readSession()) return <p role="status">Panelinize yönlendiriliyor…</p>
  return (
    <div className="auth-app">
      <aside className="story-panel">
        <a
          href="#"
          aria-label="Giriş seçeneklerine dön"
          onClick={(e) => {
            e.preventDefault()
            navigate("choice")
          }}
        >
          <Brand />
        </a>
        <div className="story-content">
          <img
            className="brand-banner"
            src={brandBanner}
            alt="Okut Ye — Menü cebinde, afiyet masanda"
            width="2172"
            height="724"
            loading="eager"
            fetchPriority="high"
            decoding="async"
          />
          <span className="story-label">
            <i /> Menünüz, her an elinizin altında
          </span>
          <h2>
            İşletmenize odaklanın.
            <br />
            Menünüz bizde.
          </h2>
          <p>
            Ürünlerinizden menü tasarımınıza,
            <br />
            her şey tek bir yerde.
          </p>
          <div className="story-features">
            <span>
              <Icon name="check" size={15} />
              Kolay yönetim
            </span>
            <span>
              <Icon name="check" size={15} />
              Anında güncelleme
            </span>
          </div>
        </div>
        <div className="story-footer">
          İşletmeniz büyürken menünüz size ayak uydursun.
        </div>
      </aside>
      <div className="auth-main">
        <header>
          <a
            className="mobile-brand"
            href="#"
            onClick={(e) => {
              e.preventDefault()
              navigate("choice")
            }}
          >
            <Brand />
          </a>

        </header>
        <main ref={mainRef} tabIndex={-1}>
          {page === "admin" ? (
            <Admin navigate={navigate} onSuccess={completeLogin} />
          ) : page === "business" ? (
            <Business navigate={navigate} onSuccess={completeLogin} />
          ) : (
            <Choice navigate={navigate} />
          )}
        </main>
        <footer>
          <span>© {new Date().getFullYear()} Okut Ye</span>

        </footer>
      </div>
    </div>
  )
}
