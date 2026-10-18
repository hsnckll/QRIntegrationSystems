import React, { useEffect, useState } from "react"
import { createRoot } from "react-dom/client"
import { menuApi } from "../services/api/endpoints"
import MenuRenderer from "../templates/MenuRenderer"
import ErrorBoundary from "../shared/ErrorBoundary"

import "../shared/styles/dashboard.css"

function MenuApp() {
  const [menu, setMenu] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState("")

  useEffect(() => {
    async function fetchMenu() {
      const params = new URLSearchParams(location.search)

      const preview = params.get("previewTemplate")
      if (Boolean(preview) == true) {
         setMenu({ templateName: preview, businessName: "Uyumluluk önizlemesi", logoPath:null, bannerPath:null, categories:[{id:1,name:"Önizleme kategorisi",sortOrder:0,products:[{id:1,name:"Uyumluluk test ürünü",description:"Bu içerik yalnız önizlemede kullanılır; işletmeye kaydedilmez.",price:125.5,imagePath:null,sortOrder:0}]}] })
         setLoading(false)
         return
      }

      const rootDomain = import.meta.env.VITE_ROOT_DOMAIN || "okutye.com"
      const hostname = location.hostname
      const sub = hostname.endsWith("."+rootDomain) ? hostname.slice(0,-rootDomain.length-1) : ""
      const slug = params.get("slug") || (sub && !sub.includes(".") && sub!=="www" ? sub : "")

      try {
        if(!slug)throw new Error("İşletme menü adresi bulunamadı.")
        setLoading(true)
        const data = await menuApi.get(slug)
        setMenu(data)
        document.title = `${data.businessName || "QR Menü"} · Menü`
      } catch (err) {
        setError(err.message || "Menü yüklenemedi veya işletme aktif değil.")
      } finally {
        setLoading(false)
      }
    }

    fetchMenu()
  }, [])

  if (loading == true) {
    return (
      <div
        style={{
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
          minHeight: "100vh",
          background: "#1c1c1e",
          color: "#fff",
          fontFamily: "sans-serif",
        }}
      >
        <div style={{ textAlign: "center" }}>
          <div
            style={{
              width: "36px",
              height: "36px",
              border: "3px solid #dc2626",
              borderTopColor: "transparent",
              borderRadius: "50%",
              animation: "spin 1s linear infinite",
              margin: "0 auto 16px",
            }}
          />
          <style>{`@keyframes spin { to { transform: rotate(360deg); } }`}</style>
          <p style={{ fontSize: "14px", color: "#9ca3af" }}>Menü yükleniyor…</p>
        </div>
      </div>
    )
  }

  if (error !== "" || Boolean(menu) == false) {
    return (
      <div
        style={{
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
          minHeight: "100vh",
          background: "#1c1c1e",
          color: "#fff",
          fontFamily: "sans-serif",
          padding: "20px",
        }}
      >
        <div
          style={{
            maxWidth: "400px",
            textAlign: "center",
            padding: "32px",
            background: "#252527",
            borderRadius: "14px",
            border: "1px solid #3f3f46",
          }}
        >
          <h2 style={{ fontSize: "20px", marginBottom: "8px", color: "#f87171" }}>
            Menüye Ulaşılamadı
          </h2>
          <p style={{ fontSize: "14px", color: "#a1a1aa", marginBottom: "20px" }}>
            {error || "Bu işletme bulunamadı veya menüsü şu an yayında değil."}
          </p>
          <button
            onClick={() => location.reload()}
            style={{
              padding: "10px 20px",
              background: "#dc2626",
              color: "#fff",
              border: "none",
              borderRadius: "8px",
              cursor: "pointer",
              fontWeight: "600",
            }}
          >
            Yeniden Dene
          </button>
        </div>
      </div>
    )
  }

  return (
    <MenuRenderer
      templateName={menu.templateName}
      menuData={menu}
      embedded={false}
      onReady={()=>{if(new URLSearchParams(location.search).has("previewTemplate") && parent!==window)parent.postMessage({type:"okutye:preview-ready"},location.origin)}}
    />
  )
}

createRoot(document.getElementById("root")).render(
  <ErrorBoundary>
    <MenuApp />
  </ErrorBoundary>,
)
