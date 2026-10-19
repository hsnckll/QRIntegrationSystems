import LogChanges from "./views/LogChanges"
import PlatformBrand from "../../shared/PlatformBrand"
import React, { useState, useEffect, useMemo, useRef } from "react"
import {
  Icon,
  Button,
  Badge,
  Field,
  Modal,
  ProductForm,
  CategoryForm,
} from "../../shared/components"
import {
  loadAdminData,
  loadBusinessLogs,
  loadBusinessMenu,
  saveMenuItem,
  deleteMenuItem,
  createBusiness,
  updateBusiness,
  toggleBusiness,
  deleteBusiness,
  saveDesign,
  createSubscription,
  replaceSubscription,
  createTemplate,
  deleteTemplate,
  money,
  date,
  today,
  currentSub,
  days,

} from "./repository"
import LoadingDashboard from "./LoadingDashboard"
import { logout } from "../../shared/session"
import usePanelRoute from "../../shared/usePanelRoute"
import {
  safeJson,
  Initial,
  Metric,
  Search,
  BusinessList,
  Empty,
  Chart,
  BusinessForm,
  MenuEditor,
  DesignEditor,
  Subscriptions,
  SubscriptionForm,
  QRPanel,
  QRForm,
  Logs,
  Profile,

  TemplateCardPreview,
  TemplateForm,
} from "./views"
const nav = [
  ["home", "Genel bakış", "chart"],
  ["businesses", "İşletmeler", "box"],
  ["subscriptions", "Abonelikler", "calendar"],
  ["qrs", "QR kodları", "box"],
  ["templates", "Menü şablonları", "eye"],
  ["logs", "Değişiklik geçmişi", "edit"],
]
const emptyData = {
  profile: { name: "Sistem Yöneticisi", email: "" },
  templates: [],
  businesses: [],
  subscriptions: [],
  qrs: [],
  logs: [],
  categories: [],
  products: [],
  dashboard: {
    totalScans: 0,
    todayScans: 0,
    monthScans: 0,
    totalRevenue: 0,
    monthRevenue: 0,
    todayRevenue: 0,
    revenueSeries: [],
    series: [],
  },
}

export default function SuperAdminDashboard() {
  const [d, setD] = useState(emptyData),
    [loading, setLoading] = useState(true),
    [loadError, setLoadError] = useState(""),
    [mobile, setMobile] = useState(false),
    [modal, setModal] = useState(null),
    [toast, setToast] = useState(""),
    [search, setSearch] = useState(""),
    [status, setStatus] = useState("all"),
    [period, setPeriod] = useState(6),
    [templateLimit, setTemplateLimit] = useState(9),
    [modalSearch, setModalSearch] = useState("")
  const [route, navigateRoute] = usePanelRoute({ pages: ["home", "businesses", "subscriptions", "qrs", "templates", "logs", "profile"], defaultPage: "home", tabs: ["info", "menu", "design", "subscription", "qr", "history"] })
  const { page, business: detail, tab } = route
  useEffect(() => { setMobile(false); setSearch(""); setStatus("all") }, [page, detail])

  const templateObserverRef = useRef(null)
  const [syncing, setSyncing] = useState(false)
  const [businessHistory, setBusinessHistory] = useState({ business: null, logs: [], error: "", loading: false })
  const [menuStatus, setMenuStatus] = useState({ business: null, loading: false, error: "" })

  useEffect(() => {
    if (page !== "businesses" || tab !== "menu" || detail == null) return
    let cancelled = false
    setMenuStatus({ business: detail, loading: true, error: "" })
    loadBusinessMenu(detail).then(menu => {
      if (cancelled) return
      setD(previous => ({ ...previous, categories: menu.categories, products: menu.products }))
      setMenuStatus({ business: detail, loading: false, error: "" })
    }).catch(err => {
      if (!cancelled) setMenuStatus({ business: detail, loading: false, error: err.message || "Menü yüklenemedi." })
    })
    return () => { cancelled = true }
  }, [page, tab, detail, d.businesses])

  useEffect(() => {
    if (page !== "businesses" || tab !== "history" || detail == null) return
    let cancelled = false
    setBusinessHistory({ business: detail, logs: [], error: "", loading: true })
    loadBusinessLogs(detail).then(logs => {
      if (!cancelled) setBusinessHistory({ business: detail, logs, error: "", loading: false })
    }).catch(err => {
      if (!cancelled) setBusinessHistory({ business: detail, logs: [], error: err.message || "Geçmiş yüklenemedi.", loading: false })
    })
    return () => { cancelled = true }
  }, [page, tab, detail, d.logs])

  async function refreshData(isInitial = false) {
    try {
      if (isInitial == true) {
        setLoading(true)
      } else {
        setSyncing(true)
      }
      const data = await loadAdminData()
      setD(data)
      setLoadError("")
    } catch (err) {
      setLoadError(err.message || "Veriler sunucudan alınamadı.")
    } finally {
      if (isInitial == true) {
        setLoading(false)
      } else {
        setSyncing(false)
      }
    }
  }

  useEffect(() => {
    refreshData(true)
  }, [])

  useEffect(() => {
    if (Boolean(toast) == true) {
      const t = setTimeout(() => setToast(""), 4500)
      return () => clearTimeout(t)
    }
  }, [toast])

  useEffect(() => {
    if (page !== "templates") return
    if (templateLimit >= d.templates.length) return

    const observer = new IntersectionObserver(
      (entries) => {
        if (entries[0]?.isIntersecting == true) {
          setTemplateLimit((prev) => Math.min(prev + 9, d.templates.length))
        }
      },
      { threshold: 0.1, rootMargin: "150px" },
    )

    const el = templateObserverRef.current
    if (Boolean(el) == true) {
      observer.observe(el)
    }

    return () => {
      if (Boolean(el) == true) {
        observer.unobserve(el)
      }
    }
  }, [page, templateLimit, d.templates.length])

  const businesses = d.businesses.filter((b) => b.deleted == false),
    b = d.businesses.find((b) => b.id === detail),
    byId = (id) => d.businesses.find((b) => b.id === id),
    revenueSeries = useMemo(() => {
      const list = d.dashboard?.revenueSeries || []
      if (period === 6) {
        return list.slice(-6)
      }
      return list.slice(-12)
    }, [d.dashboard, period]),
    filteredTemplates = useMemo(() => {
      const q = String(search || "").trim().toLocaleLowerCase("tr")
      if (q === "") return d.templates
      return d.templates.filter((t) => {
        const name = String(t.name || "").toLocaleLowerCase("tr")
        const folder = String(t.folder || t.folderName || "").toLocaleLowerCase("tr")
        const idStr = String(t.id || "")
        return (
          name.includes(q) == true ||
          folder.includes(q) == true ||
          idStr === q
        )
      })
    }, [d.templates, search]),
    filteredModalBusinesses = useMemo(() => {
      if (Boolean(modal) == false || modal.type !== "templateBusinesses") return []
      const list = modal.list || []
      const q = String(modalSearch || "").trim().toLocaleLowerCase("tr")
      if (q === "") return list
      return list.filter((item) => {
        const name = String(item.name || "").toLocaleLowerCase("tr")
        const email = String(item.email || "").toLocaleLowerCase("tr")
        const phone = String(item.phone || "").toLocaleLowerCase("tr")
        const city = String(item.city || "").toLocaleLowerCase("tr")
        return (
          name.includes(q) == true ||
          email.includes(q) == true ||
          phone.includes(q) == true ||
          city.includes(q) == true
        )
      })
    }, [modal, modalSearch])

  function go(next) {
    navigateRoute({ page: next, business: null, tab: "info" })
    setSearch("")
    setStatus("all")
    setMobile(false)
    if (next === "templates") {
      setTemplateLimit(9)
    }
  }

  function openBusiness(id, nextTab = "info") {
    navigateRoute({ page: "businesses", business: id, tab: nextTab })
    setSearch("")
    setStatus("all")
  }

  async function save(collection, item, business, action) {
    try {
      if (collection === "businesses") {
        const old = d.businesses.find((x) => x.id === item.id)
        if (old == null) {
          await createBusiness(item)
          setToast("İşletme ve başlangıç aboneliği başarıyla oluşturuldu.")
        } else if (action === "İşletme durumu güncellendi") {
          await toggleBusiness(item.id)
          setToast("İşletme durumu güncellendi.")
        } else if (action === "İşletme silindi") {
          await deleteBusiness(item.id)
          setToast("İşletme silindi.")
        } else if (action === "İşletme tasarımı güncellendi") {
          await saveDesign(old, item)
          setToast("İşletme tasarımı güncellendi.")
        } else {
          await updateBusiness(item)
          setToast("İşletme bilgileri güncellendi.")
        }
      } else if (collection === "categories" || collection === "products") {
        await saveMenuItem(collection, item, d[collection].find(x => x.id === item.id))
        setToast(action + ".")
      } else if (collection === "subscriptions") {
        const old = d.subscriptions.find((x) => x.id === item.id)
        if (old == null) {
          await createSubscription(item)
          setToast("Abonelik başarıyla tanımlandı.")
        } else {
          await replaceSubscription(old, item)
          setToast("Abonelik güncellendi.")
        }
      }
      setModal(null)
      await refreshData()
      return true
    } catch (err) {
      setToast(err.message || "İşlem kaydedilemedi.")
      return false
    }
  }
  async function move(collection, item, direction) {
    const siblings = d[collection].filter(x => x.business === item.business &&
      (collection !== "products" || x.category === item.category))
    const index = siblings.findIndex(x => x.id === item.id)
    const target = index + direction
    if (index < 0 || target < 0 || target >= siblings.length) return
    ;[siblings[index], siblings[target]] = [siblings[target], siblings[index]]
    try {
      for (const [sortOrder, sibling] of siblings.entries())
        await saveMenuItem(collection, { ...sibling, sortOrder }, sibling)
      await refreshData()
      setToast("Menü sıralaması güncellendi.")
    } catch (err) {
      setToast(err.message || "Sıralama kaydedilemedi.")
      await refreshData()
    }
  }

  if (loading == true) return <LoadingDashboard />

  if (loadError !== "" && d.businesses.length === 0) {
    return (
      <div className="app" style={{ display: "flex", alignItems: "center", justifyContent: "center", minHeight: "100vh" }}>
        <div className="surface" style={{ maxWidth: "480px", textAlign: "center", padding: "32px", margin: "20px" }}>
          <h2 style={{ fontSize: "18px", color: "#e11d48", marginBottom: "12px" }}>
            API Bağlantı Hatası
          </h2>
          <p style={{ fontSize: "13px", color: "#64748b", marginBottom: "20px" }}>
            {loadError}
          </p>
          <Button kind="primary" onClick={refreshData}>
            Yeniden Dene
          </Button>
        </div>
      </div>
    )
  }

  const title = detail
    ? b?.name
    : page === "profile"
      ? "Yönetici bilgilerim"
      : nav.find((n) => n[0] === page)?.[1]
  return (
    <div className="app">
      {mobile && (
        <button
          className="scrim"
          aria-label="Menüyü kapat"
          onClick={() => setMobile(false)}
        />
      )}
      <aside className={`sidebar ${mobile ? "opened" : ""}`}>
        <a
          href="#"
          className="brand"
          onClick={(e) => {
            e.preventDefault()
            go("home")
          }}
        >
          <PlatformBrand subtitle="Yönetim merkezi" />
        </a>
        <button
          className={`profile-shortcut ${page === "profile" ? "selected" : ""}`}
          onClick={() => go("profile")}
        >
          <span className="avatar">SA</span>
          <span className="profile-text">
            <strong>{d.profile.name}</strong>
            <span>Hesap ayarları</span>
          </span>
          <Icon name="chevron" size={16} />
        </button>
        <span className="nav-label">Yönetim alanı</span>
        <nav>
          {nav.map(([key, label, icon]) => (
            <button
              key={key}
              className={page === key ? "selected" : ""}
              aria-current={page === key ? "page" : undefined}
              onClick={() => go(key)}
            >
              <Icon name={icon} />
              {label}
              {key === "businesses" && (
                <span className="nav-count">{businesses.length}</span>
              )}
            </button>
          ))}
        </nav>
        <div className="sidebar-bottom">
          <div className="logout-area">
            <button
              className="logout-button"
              onClick={() => {
                setModal(null)
                logout()
              }}
            >
              <Icon name="arrow" />
              Çıkış yap
            </button>
          </div>
        </div>
      </aside>
      <div className="workspace">
        <header className="topbar">
          <div className="breadcrumb">
            <button
              className="icon-button mobile-toggle"
              aria-label="Gezinme menüsü"
              onClick={() => setMobile(mobile == false)}
            >
              <Icon name="menu" />
            </button>
            <span>Yönetim merkezi</span>
            <Icon name="chevron" size={14} />
            <strong>{detail ? "İşletme detayı" : title}</strong>
          </div>
          <div className="top-right">
            {syncing == true && (
              <span className="muted small" style={{ marginRight: "6px" }}>
                Kaydediliyor...
              </span>
            )}


          </div>
        </header>
        <main style={{ paddingBottom: "48px" }}>
          {loadError && <p className="error" role="alert">Veriler yenilenemedi: {loadError}</p>}
          <div className="page-heading">
            <div>
              {detail && (
                <button className="back-link" onClick={() => go("businesses")}>
                  ← İşletmelere dön
                </button>
              )}
              <h1>
                {page === "home" ? "Her işletme, tek bir merkezde." : title}
              </h1>
              <p>
                {detail
                  ? "İşletme bilgilerini, menüsünü ve erişimini yönetin."
                  : {
                      home: "İşletmelerinizin bugününe birlikte bakalım.",
                      businesses:
                        "Yeni işletmeler ekleyin, mevcut işletmelerinizi yönetin.",
                      subscriptions:
                        "Abonelik sürelerini takip edin ve yenileyin.",
                      qrs: "İşletmeye özel QR kodlarını üretin ve yönetin.",
                      templates: "İşletmelere atanabilen menü tasarımları.",
                      logs: "Kimin, ne zaman, neyi değiştirdiğini görün.",
                      profile:
                        "Yönetici hesabınızın bilgilerini görüntüleyin.",
                    }[page]}
              </p>
            </div>
            {page === "businesses" && Boolean(detail) == false && (
              <Button
                kind="primary"
                icon="plus"
                onClick={() => setModal({ type: "business" })}
              >
                İşletme ekle
              </Button>
            )}
          </div>
          {page === "home" && (
            <>
              {businesses.some(x => { const s = currentSub(d, x.id); return s && days(s.end) < 0 }) && (
                <section className="surface subscription-policy-notice" role="status">
                  <strong>Abonelik süresi dolan işletmeler var</strong>
                  <p>Menüler açık kalır. Yenileme gerektiren kayıtları abonelikler bölümünden kontrol edin.</p>
                  <Button onClick={() => go("subscriptions")}>Abonelikleri kontrol et</Button>
                </section>
              )}
              <div className="admin-metrics">
                <Metric
                  icon="box"
                  label="Toplam işletme"
                  value={businesses.length}
                  note="Kayıtlı işletmeler"
                />
                <Metric
                  icon="check"
                  label="Aktif işletme"
                  value={businesses.filter((x) => x.active == true).length}
                  note={`${businesses.filter((x) => x.active == false).length} işletme pasif`}
                  tone="green-bg"
                />
                <Metric
                  icon="calendar"
                  label="Yakında bitecek"
                  value={
                    businesses.filter((x) => {
                      const s = currentSub(d, x.id)
                      return (
                        Boolean(s) == true &&
                        days(s.end) >= 0 &&
                        days(s.end) <= 14
                      )
                    }).length
                  }
                  note="Önümüzdeki 14 gün"
                  tone="amber"
                />
                <Metric
                  icon="creditCard"
                  label="Toplam ciro"
                  value={money(d.dashboard?.totalRevenue || 0)}
                  note={`Bu ay: ${money(d.dashboard?.monthRevenue || 0)}`}
                  tone="purple"
                />
              </div>
              <section className="surface setup-tasks">
                <div className="section-top"><div><h2>Kurulum kontrolü</h2><p>Şablon veya QR kodu eksik işletmeler</p></div><Button onClick={()=>go("businesses")}>İşletmeleri yönet</Button></div>
                {businesses.filter(x=>!x.template || !d.qrs.some(q=>q.business===x.id)).length===0 ? <p className="setup-complete">Tüm işletmelerin şablon ve QR kodu tanımlı.</p> : businesses.filter(x=>!x.template || !d.qrs.some(q=>q.business===x.id)).map(x=><button className="setup-task" key={x.id} onClick={()=>openBusiness(x.id,!x.template ? "design" : "qr")}><strong>{x.name}</strong><span>{[!x.template && "Şablon ataması",!d.qrs.some(q=>q.business===x.id) && "QR kodu"].filter(Boolean).join(" ve ")} gerekli</span><Icon name="arrow" size={18}/></button>)}
              </section>
              <Logs logs={d.logs.slice(0,5)} byId={byId} title="Son değişiklikler" onInspect={log=>setModal({type:"log",item:log})}/>
              <div className="dashboard-grid">
                <section className="surface dashboard-chart">
                  <div className="section-top">
                    <div>
                      <h2>Aylık Ciro ve Gelir Dağılımı</h2>
                      <p>Abonelik paketlerinden elde edilen gelir</p>
                    </div>
                    <select
                      aria-label="Grafik dönemi"
                      value={period}
                      onChange={(e) => setPeriod(Number(e.target.value))}
                    >
                      <option value={6}>Son 6 ay</option>
                      <option value={12}>Son 12 ay</option>
                    </select>
                  </div>
                  <Chart series={revenueSeries} />
                  <p className="chart-disclaimer">
                    Veritabanındaki işletme abonelik ödemelerinden dinamik olarak hesaplanmaktadır.
                  </p>
                </section>
                <section className="surface renewal">
                  <div className="section-top">
                    <div>
                      <h2>Takip gerektirenler</h2>
                      <p>Abonelik bitişleri</p>
                    </div>
                    <Icon name="calendar" />
                  </div>
                  <div className="renewal-body">
                    {(() => {
                      const expiring = businesses
                        .map((x) => {
                          const s = currentSub(d, x.id)
                          return {
                            item: x,
                            sub: s,
                            remaining: Boolean(s) == true ? days(s.end) : null,
                          }
                        })
                        .filter((x) => Boolean(x.sub) == true && x.remaining <= 14)
                        .sort((a, b) => a.remaining - b.remaining)

                      if (expiring.length === 0) {
                        return (
                          <div
                            style={{
                              padding: "36px 16px",
                              textAlign: "center",
                              margin: "auto 0",
                            }}
                          >
                            <div
                              style={{
                                display: "inline-flex",
                                alignItems: "center",
                                justifyContent: "center",
                                width: "40px",
                                height: "40px",
                                borderRadius: "50%",
                                background: "#f0fdf4",
                                color: "#16a34a",
                                marginBottom: "10px",
                              }}
                            >
                              <Icon name="check" size={20} />
                            </div>
                            <p
                              style={{
                                fontWeight: "600",
                                fontSize: "13px",
                                color: "#1e293b",
                                margin: "0 0 4px",
                              }}
                            >
                              Takip gerektiren durum yok
                            </p>
                            <p
                              style={{
                                fontSize: "12px",
                                color: "#64748b",
                                margin: 0,
                              }}
                            >
                              Tüm işletmelerin abonelikleri güncel.
                            </p>
                          </div>
                        )
                      }

                      return expiring.map(({ item: x, sub: s, remaining }) => (
                        <button
                          className="renewal-row"
                          key={x.id}
                          onClick={() => openBusiness(x.id, "subscription")}
                        >
                          <Initial b={x} />
                          <span>
                            <strong>{x.name}</strong>
                            <small>{date(s.end)}</small>
                          </span>
                          <span
                            className={
                              remaining < 0
                                ? "red-text"
                                : "amber-text"
                            }
                            style={{
                              fontWeight: remaining === 0 ? "600" : "normal",
                            }}
                          >
                            {remaining < 0
                              ? "Sona erdi"
                              : remaining === 0
                                ? "Bugün sona eriyor"
                                : `${remaining} gün kaldı`}
                          </span>
                        </button>
                      ))
                    })()}
                  </div>
                  <div className="renewal-footer">
                    <Button onClick={() => go("subscriptions")}>
                      Abonelikleri görüntüle <Icon name="arrow" size={16} />
                    </Button>
                  </div>
                </section>
              </div>
              <section className="surface">
                <div className="section-top">
                  <div>
                    <h2>İşletmeleriniz</h2>
                    <p>Menüler, tasarımlar ve durumlar</p>
                  </div>
                  <Button onClick={() => go("businesses")}>Tümünü gör</Button>
                </div>
                <BusinessList
                  list={businesses.slice(0, 4)}
                  d={d}
                  onOpen={openBusiness}
                />
              </section>
            </>
          )}
          {page === "businesses" && Boolean(detail) == false && (
            <section className="surface">
              <div className="list-toolbar">
                <Search
                  value={search}
                  onChange={setSearch}
                  placeholder="İşletme adı, yetkili veya e-posta ara…"
                />
                <select
                  aria-label="İşletme durumu"
                  value={status}
                  onChange={(e) => setStatus(e.target.value)}
                >
                  <option value="all">Tüm işletmeler</option>
                  <option value="active">Aktif</option>
                  <option value="passive">Pasif</option>
                  <option value="deleted">Silinenler</option>
                </select>
              </div>
              <BusinessList
                list={d.businesses.filter(
                  (x) =>
                    (status === "deleted" ? x.deleted == true : x.deleted == false) &&
                    (status !== "active" || x.active == true) &&
                    (status !== "passive" || x.active == false) &&
                    [x.name, x.owner, x.email]
                      .join(" ")
                      .toLocaleLowerCase("tr")
                      .includes(search.toLocaleLowerCase("tr")),
                )}
                d={d}
                onOpen={openBusiness}
              />
            </section>
          )}
          {Boolean(detail) == true && Boolean(b) == true && (
            <>
              <div className="detail-summary">
                <Initial b={b} />
                <div>
                  <h2>{b.name}</h2>
                  <p>
                    {b.address} · /{b.slug}
                  </p>
                </div>
                <Badge active={b.active == true && b.deleted == false}>
                  {b.deleted == true ? "Silindi" : b.active == true ? "Aktif" : "Pasif"}
                </Badge>
                {b.deleted == true ? (
                  <Button
                    onClick={() =>
                      save(
                        "businesses",
                        { ...b, deleted: false, active: false },
                        b.id,
                        "İşletme geri yüklendi",
                      )
                    }
                  >
                    Geri yükle
                  </Button>
                ) : (
                  <div className="detail-actions">
                    <Button
                      onClick={() =>
                        save(
                          "businesses",
                          { ...b, active: b.active == false },
                          b.id,
                          "İşletme durumu güncellendi",
                        )
                      }
                    >
                      {b.active == true ? "Pasif yap" : "Aktif yap"}
                    </Button>
                    <Button
                      kind="danger-outline"
                      icon="trash"
                      onClick={() =>
                        setModal({ type: "deleteBusiness", item: b })
                      }
                    >
                      İşletmeyi sil
                    </Button>
                  </div>
                )}
              </div>
              {b.deleted == false && (
                <>
                  <div className="detail-tabs">
                    {[
                      ["info", "Bilgiler"],
                      ["menu", "Menü düzenleme"],
                      ["design", "Görseller & şablon"],
                      ["subscription", "Abonelik"],
                      ["qr", "QR kodları"],
                      ["history", "Geçmiş"],
                    ].map(([key, label]) => (
                      <button
                        className={tab === key ? "active" : ""}
                        onClick={() => navigateRoute({ tab: key })}
                        key={key}
                      >
                        {label}
                      </button>
                    ))}
                  </div>
                  {tab === "info" && (
                    <BusinessForm
                      key={b.id}
                      item={b}
                      templates={d.templates}
                      businesses={d.businesses}
                      inline
                      onSave={(x) =>
                        save(
                          "businesses",
                          x,
                          b.id,
                          "İşletme bilgileri güncellendi",
                        )
                      }
                    />
                  )}
                  {tab === "menu" && (
                    menuStatus.business !== b.id || menuStatus.loading ? (
                      <p className="muted" role="status">İşletmenin menüsü yükleniyor…</p>
                    ) : menuStatus.error ? (
                      <div role="alert"><p className="error">{menuStatus.error}</p><Button onClick={() => refreshData()}>Yeniden dene</Button></div>
                    ) : <MenuEditor
                      key={b.id}
                      d={d}
                      b={b}
                      setModal={setModal}
                      save={save}
                      move={move}
                    />
                  )}
                  {tab === "design" && (
                    <DesignEditor
                      key={b.id}
                      b={b}
                      templates={d.templates}
                      onSave={(x) =>
                        save(
                          "businesses",
                          x,
                          b.id,
                          "İşletme tasarımı güncellendi",
                        )
                      }
                    />
                  )}
                  {tab === "subscription" && (
                    <>
                      <div className="section-top">
                        <div>
                          <h2>Abonelik kayıtları</h2>
                          <p>İşletmenin mevcut ve geçmiş abonelikleri</p>
                        </div>
                        <Button
                          kind="primary"
                          icon="plus"
                          onClick={() =>
                            setModal({ type: "subscription", business: b.id })
                          }
                        >
                          Abonelik ekle
                        </Button>
                      </div>
                      <Subscriptions
                        d={d}
                        businesses={[b]}
                        onEdit={(s) =>
                          setModal({
                            type: "subscription",
                            item: s,
                            business: b.id,
                          })
                        }
                      />
                    </>
                  )}
                  {tab === "qr" && (
                    <QRPanel
                      d={d}
                      businesses={[b]}
                      readOnly={true}
                      onCreate={(id) => setModal({ type: "qr", business: id })}
                      onToggle={(q) =>
                        save(
                          "qrs",
                          { ...q, active: q.active == false },
                          q.business,
                          "QR durumu güncellendi",
                        )
                      }
                      onError={setToast}
                    />
                  )}
                  {tab === "history" && (
                    businessHistory.business !== b.id || businessHistory.loading ? (
                      <p className="muted" role="status">İşletmenin geçmişi yükleniyor…</p>
                    ) : businessHistory.error ? (
                      <p className="error" role="alert">{businessHistory.error}</p>
                    ) : (
                      <Logs
                        title="Bu işletmenin geçmişi"
                        logs={businessHistory.logs}
                        byId={byId}
                        onInspect={(l) => setModal({ type: "log", item: l })}
                      />
                    )
                  )}
                </>
              )}
            </>
          )}
          {page === "subscriptions" && (
            <>
              <div className="list-toolbar standalone">
                <Search
                  value={search}
                  onChange={setSearch}
                  placeholder="İşletme ara…"
                />
              </div>
              <Subscriptions
                d={d}
                businesses={businesses.filter((b) =>
                  b.name
                    .toLocaleLowerCase("tr")
                    .includes(search.toLocaleLowerCase("tr")),
                )}
                onEdit={(s) =>
                  setModal({
                    type: "subscription",
                    item: s,
                    business: s.business,
                  })
                }
              />
            </>
          )}
          {page === "qrs" && (
            <QRPanel
              d={d}
              businesses={businesses}
              readOnly={true}
              onCreate={(id) => setModal({ type: "qr", business: id })}
              onToggle={(q) =>
                save(
                  "qrs",
                  { ...q, active: q.active == false },
                  q.business,
                  "QR durumu güncellendi",
                )
              }
              onError={setToast}
            />
          )}
          {page === "templates" && (
            <>
              <div
                className="list-toolbar standalone"
                style={{
                  display: "flex",
                  justifyContent: "space-between",
                  alignItems: "center",
                  flexWrap: "wrap",
                  gap: "14px",
                }}
              >
                <div style={{ flex: 1, minWidth: "260px", maxWidth: "480px" }}>
                  <Search
                    value={search}
                    onChange={setSearch}
                    placeholder="Şablon adı veya klasör adı ile ara…"
                  />
                </div>
                <Button
                  onClick={() => setModal({ type: "templateCreate" })}
                  style={{ display: "inline-flex", alignItems: "center", gap: "7px" }}
                >
                  <Icon name="check" size={16} />
                  Yeni Şablon Ekle
                </Button>
              </div>
              {filteredTemplates.length === 0 ? (
                <div
                  className="surface"
                  style={{
                    textAlign: "center",
                    padding: "48px 24px",
                    borderRadius: "12px",
                    border: "1px dashed var(--line)",
                    color: "var(--muted)",
                  }}
                >
                  <Icon name="search" size={32} />
                  <p style={{ marginTop: "12px", fontSize: "14px", fontWeight: "600", color: "var(--navy)" }}>
                    {search.trim() !== ""
                      ? `"${search}" aramasına uygun şablon bulunamadı.`
                      : "Henüz sisteme kayıtlı şablon bulunmuyor."}
                  </p>
                  {search.trim() !== "" && (
                    <Button
                      onClick={() => setSearch("")}
                      style={{ marginTop: "14px", fontSize: "12px" }}
                    >
                      Aramayı Temizle
                    </Button>
                  )}
                </div>
              ) : (
                <div className="template-grid">
                  {filteredTemplates.slice(0, templateLimit).map((t, i) => {
                    const usedBusinesses = businesses.filter(
                      (b) =>
                        b.template === t.id ||
                        b.template === t.folder ||
                        String(b.template) === String(t.id) ||
                        String(b.template) === String(t.folder),
                    )
                    return (
                      <section className="surface template-card" key={t.id || i}>
                        <TemplateCardPreview
                          template={t}
                          onPreview={(tmpl) =>
                            setModal({ type: "templatePreview", template: tmpl })
                          }
                        />
                        <div className="template-copy">
                          <h2>{t.name}</h2>
                          <button
                            type="button"
                            className="template-usage-pill"
                            onClick={() => {
                              setModalSearch("")
                              setModal({
                                type: "templateBusinesses",
                                template: t,
                                list: usedBusinesses,
                              })
                            }}
                            style={{
                              display: "inline-flex",
                              alignItems: "center",
                              gap: "6px",
                              padding: "4px 10px",
                              borderRadius: "16px",
                              fontSize: "11.5px",
                              fontWeight: "500",
                              cursor: "pointer",
                              border:
                                usedBusinesses.length > 0
                                  ? "1px solid #bfdbfe"
                                  : "1px solid #e2e8f0",
                              background:
                                usedBusinesses.length > 0
                                  ? "#eff6ff"
                                  : "#f8fafc",
                              color:
                                usedBusinesses.length > 0
                                  ? "#1e40af"
                                  : "#64748b",
                              transition: "all 0.15s ease",
                              marginTop: "6px",
                              width: "fit-content",
                            }}
                            onMouseEnter={(e) => {
                              e.currentTarget.style.background =
                                usedBusinesses.length > 0 ? "#dbeafe" : "#f1f5f9"
                              e.currentTarget.style.borderColor =
                                usedBusinesses.length > 0 ? "#93c5fd" : "#cbd5e1"
                            }}
                            onMouseLeave={(e) => {
                              e.currentTarget.style.background =
                                usedBusinesses.length > 0 ? "#eff6ff" : "#f8fafc"
                              e.currentTarget.style.borderColor =
                                usedBusinesses.length > 0 ? "#bfdbfe" : "#e2e8f0"
                            }}
                            title="Bu şablonu kullanan işletmeleri görüntüle"
                          >
                            <span
                              style={{
                                display: "inline-flex",
                                alignItems: "center",
                                justifyContent: "center",
                                minWidth: "18px",
                                height: "18px",
                                padding: "0 4px",
                                borderRadius: "10px",
                                fontSize: "10.5px",
                                fontWeight: "700",
                                background:
                                  usedBusinesses.length > 0 ? "#2563eb" : "#94a3b8",
                                color: "#ffffff",
                              }}
                            >
                              {usedBusinesses.length}
                            </span>
                            <span>
                              {usedBusinesses.length > 0
                                ? "işletmede kullanılıyor"
                                : "henüz atanmamış"}
                            </span>
                            <Icon name="arrow" size={11} />
                          </button>
                          <div
                            style={{
                              marginTop: "12px",
                              paddingTop: "10px",
                              borderTop: "1px solid #f1f4f9",
                              display: "flex",
                              alignItems: "center",
                              justifyContent: "space-between",
                              gap: "8px",
                            }}
                          >
                            <code
                              style={{
                                margin: 0,
                                fontSize: "10.5px",
                                color: "#64748b",
                                background: "#f1f5f9",
                                padding: "3px 8px",
                                borderRadius: "4px",
                                border: "1px solid #e2e8f0",
                                letterSpacing: "0.2px",
                              }}
                            >
                              {t.folder || t.id}
                            </code>
                            <button
                              type="button"
                              title={
                                usedBusinesses.length > 0
                                  ? `Bu şablon ${usedBusinesses.length} işletmede aktif kullanıldığı için silinemez.`
                                  : "Şablonu sil"
                              }
                              onClick={() => {
                                if (usedBusinesses.length > 0) {
                                  setToast(
                                    `"${t.name}" şablonu ${usedBusinesses.length} işletmede aktif olarak kullanılıyor. Silmeden önce ilgili işletmelerin şablonunu değiştirin.`,
                                  )
                                } else {
                                  setModal({ type: "deleteTemplate", item: t })
                                }
                              }}
                              style={{
                                width: "auto",
                                padding: "4px 10px",
                                fontSize: "11px",
                                height: "26px",
                                display: "inline-flex",
                                alignItems: "center",
                                gap: "4px",
                                borderRadius: "6px",
                                border: "1px solid #fecaca",
                                background: "#fff",
                                color: "#dc2626",
                                fontWeight: "550",
                                opacity: usedBusinesses.length > 0 ? 0.45 : 1,
                                cursor: usedBusinesses.length > 0 ? "not-allowed" : "pointer",
                                transition: "all 0.15s ease",
                              }}
                              onMouseEnter={(e) => {
                                if (usedBusinesses.length === 0) {
                                  e.currentTarget.style.background = "#fef2f2"
                                  e.currentTarget.style.borderColor = "#f87171"
                                }
                              }}
                              onMouseLeave={(e) => {
                                if (usedBusinesses.length === 0) {
                                  e.currentTarget.style.background = "#fff"
                                  e.currentTarget.style.borderColor = "#fecaca"
                                }
                              }}
                            >
                              <Icon name="trash" size={12} />
                              <span>Sil</span>
                            </button>
                          </div>
                        </div>
                      </section>
                    )
                  })}
                </div>
              )}
              {templateLimit < filteredTemplates.length ? (
                <div
                  ref={templateObserverRef}
                  className="template-infinite-loader"
                >
                  <div className="template-infinite-spinner" />
                  <span>Daha fazla şablon yükleniyor…</span>
                </div>
              ) : (
                Boolean(filteredTemplates.length > 9) == true && (
                  <div
                    style={{
                      textAlign: "center",
                      padding: "24px 0",
                      color: "#8a9ab1",
                      fontSize: "12px",
                    }}
                  >
                    Tüm şablonlar görüntülendi ({filteredTemplates.length})
                  </div>
                )
              )}
            </>
          )}
          {page === "logs" && (
            <>
              <div className="list-toolbar standalone">
                <Search
                  value={search}
                  onChange={setSearch}
                  placeholder="İşlem, yönetici veya işletme ara…"
                />
              </div>
              <Logs
                logs={d.logs.filter((l) =>
                  [l.action, l.actor, byId(l.business)?.name]
                    .join(" ")
                    .toLocaleLowerCase("tr")
                    .includes(search.toLocaleLowerCase("tr")),
                )}
                byId={byId}
                onInspect={(l) => setModal({ type: "log", item: l })}
              />
            </>
          )}
          {page === "profile" && (
            <Profile
              profile={d.profile}
              onSave={(p) =>
                commit(
                  { ...d, profile: p },
                  "Yönetici bilgileri güncellendi",
                  null,
                  "SuperAdmins",
                  d.profile,
                  p,
                )
              }
            />
          )}
        </main>
      </div>
      {modal?.type === "business" && (
        <Modal title="Yeni işletme" onClose={() => setModal(null)}>
          <BusinessForm
            templates={d.templates}
            businesses={d.businesses}
            onSave={(x) => save("businesses", x, x.id, "İşletme oluşturuldu")}
          />
        </Modal>
      )}
      {modal?.type === "category" && (
        <CategoryForm
          item={modal.item}
          onClose={() => setModal(null)}
          onSave={(x) =>
            save(
              "categories",
              { ...x, business: modal.business },
              modal.business,
              modal.item ? "Kategori güncellendi" : "Kategori eklendi",
            )
          }
        />
      )}
      {modal?.type === "product" && (
        <ProductForm
          item={modal.item}
          categories={d.categories.filter((c) => c.business === modal.business)}
          selected={modal.category || "all"}
          onClose={() => setModal(null)}
          onSave={(x) =>
            save(
              "products",
              { ...x, business: modal.business },
              modal.business,
              modal.item ? "Ürün güncellendi" : "Ürün eklendi",
            )
          }
        />
      )}
      {modal?.type === "subscription" && (
        <Modal
          title={modal.item ? "Aboneliği düzenle" : "Abonelik tanımla"}
          onClose={() => setModal(null)}
        >
          <SubscriptionForm
            item={modal.item}
            business={modal.business}
            businesses={businesses}
            onSave={(x) =>
              save(
                "subscriptions",
                x,
                x.business,
                modal.item ? "Abonelik güncellendi" : "Abonelik tanımlandı",
              )
            }
          />
        </Modal>
      )}
      {modal?.type === "qr" && (
        <Modal title="İşletmeye QR tanımla" onClose={() => setModal(null)}>
          <QRForm
            businesses={businesses}
            business={modal.business}
            onSave={(q) => save("qrs", q, q.business, "QR kodu oluşturuldu")}
          />
        </Modal>
      )}
      {modal?.type?.startsWith("delete") && (
        <Modal title="Silme işlemini onaylayın" onClose={() => setModal(null)}>
          <div className="modal-body">
            <p>
              <strong>{modal.item.name}</strong>{" "}
              {modal.type === "deleteBusiness"
                ? "silinen işletmeler listesine taşınacak. Menü ve QR erişimi kapanacak; işletmeyi daha sonra geri yükleyebilirsiniz."
                : modal.type === "deleteCategory"
                  ? `ve bu kategoriye ait ${d.products.filter((p) => p.category === modal.item.id).length} ürün kaldırılacak.`
                  : modal.type === "deleteTemplate"
                    ? "şablonu sistemden tamamen silinecek. Bu işlem geri alınamaz."
                    : "menüden kaldırılacak."}
            </p>
          </div>
          <div className="modal-actions">
            <Button onClick={() => setModal(null)}>Vazgeç</Button>
            <Button
              kind="danger"
              onClick={async () => {
                if (modal.type === "deleteBusiness") {
                  save(
                    "businesses",
                    { ...modal.item, deleted: true, active: false },
                    modal.item.id,
                    "İşletme silindi",
                  )
                  navigateRoute({ business: null, tab: "info" }, { replace: true })
                } else if (modal.type === "deleteTemplate") {
                  try {
                    await deleteTemplate(modal.item.id)
                    setD((prev) => ({
                      ...prev,
                      templates: prev.templates.filter(
                        (t) => Boolean(t.id == modal.item.id) == false,
                      ),
                    }))
                    setModal(null)
                    await refreshData()
                    setToast("Şablon başarıyla silindi.")
                  } catch (err) {
                    setToast(err.message || "Şablon silinemedi.")
                  }
                } else {
                  const collection =
                    modal.type === "deleteCategory" ? "categories" : "products"
                  try {
                    await deleteMenuItem(collection, modal.item)
                    setModal(null)
                    await refreshData()
                    setToast(collection === 'categories' ? 'Kategori ve bağlı ürünler silindi.' : 'Ürün silindi.')
                  } catch (err) { setToast(err.message || 'Silme işlemi tamamlanamadı.') }
                }
              }}
            >
              Evet, sil
            </Button>
          </div>
        </Modal>
      )}
      {modal?.type === "log" && (
        <Modal title={modal.item.action} onClose={() => setModal(null)}>
          <div className="modal-body">
            <p className="muted small">
              {modal.item.actor} ·{" "}
              {new Date(modal.item.at).toLocaleString("tr-TR")}
            </p>
            <LogChanges log={modal.item} />
          </div>
        </Modal>
      )}
      {Boolean(modal) == true &&
        modal.type === "templatePreview" &&
        Boolean(modal.template) == true && (
          <Modal
            title={`${modal.template.name} · Menü önizlemesi`}
            onClose={() => setModal(null)}
          >
            <div className="template-modal-body">
              <div className="template-modal-top">
                <div className="template-modal-badge">
                  <span className="live-dot" />
                  <strong>{modal.template.name}</strong>
                  <span className="muted small">
                    ({modal.template.folder || modal.template.id})
                  </span>
                </div>
                <a
                  href={`/menu.html?previewTemplate=${encodeURIComponent(modal.template.folder || modal.template.id || modal.template.name)}`}
                  target="_blank"
                  rel="noreferrer"
                  className="button template-external-btn"
                >
                  <Icon name="arrow" size={14} />
                  Yeni sekmede tam ekran aç
                </a>
              </div>
              <div className="phone template-modal-phone">
                <div className="phone-bar">
                  <span>9:41</span>
                  <span>● ▰</span>
                </div>
                <iframe
                  title={modal.template.name}
                  src={`/menu.html?previewTemplate=${encodeURIComponent(modal.template.folder || modal.template.id || modal.template.name)}`}
                  className="template-modal-iframe"
                />
              </div>
            </div>
          </Modal>
        )}
      {Boolean(modal) == true && modal.type === "templateCreate" && (
        <Modal title="Menü Şablonu Yükle veya Güncelle" onClose={() => setModal(null)}>
          <TemplateForm
            onClose={() => setModal(null)}
            onSave={async (tmpl) => {
              const created = await createTemplate(tmpl)
              const updating = d.templates.some(t => t.id === created.id)
              setD((prev) => ({
                ...prev,
                templates: prev.templates.some(t => t.id === created.id)
                  ? prev.templates.map(t => t.id === created.id ? created : t)
                  : [...prev.templates, created],
              }))
              setModal(null)
              setModal({type:"templatePreview",template:created});
              setToast(`"${created.name}" ${updating ? "güncellendi. İşletme atamaları korundu." : "yüklendi. İşletmelere atanabilir."}`)
            }}
          />
        </Modal>
      )}
      {Boolean(modal) == true && modal.type === "templateBusinesses" && (
        <Modal
          title={`${modal.template?.name || "Şablon"} · Kullanan İşletmeler`}
          onClose={() => {
            setModal(null)
            setModalSearch("")
          }}
        >
          <div className="modal-body" style={{ maxHeight: "60vh", overflowY: "auto", padding: "24px 28px" }}>
            {Boolean(modal.list) == true && modal.list.length > 0 ? (
              <div style={{ display: "grid", gap: "14px" }}>
                <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", gap: "12px", flexWrap: "wrap" }}>
                  <p className="muted small" style={{ margin: 0 }}>
                    Bu şablon toplam <strong>{modal.list.length}</strong> işletmeye atanmış:
                  </p>
                  {modalSearch.trim() !== "" && (
                    <span className="muted small" style={{ fontSize: "11px", color: "var(--navy)", fontWeight: "500" }}>
                      {filteredModalBusinesses.length} sonuç bulundu
                    </span>
                  )}
                </div>

                <div>
                  <Search
                    value={modalSearch}
                    onChange={setModalSearch}
                    placeholder="İşletme adı, yetkili veya telefon ara…"
                  />
                </div>

                {filteredModalBusinesses.length > 0 ? (
                  filteredModalBusinesses.map((b) => (
                    <div
                      key={b.id}
                      className="surface"
                      style={{
                        display: "flex",
                        alignItems: "center",
                        justifyContent: "space-between",
                        padding: "14px 18px",
                        borderRadius: "10px",
                        border: "1px solid var(--line)",
                        background: "#fff",
                      }}
                    >
                      <div>
                        <strong style={{ fontSize: "14px", display: "block", color: "var(--navy)" }}>{b.name}</strong>
                        <span className="muted small" style={{ display: "block", marginTop: "3px" }}>
                          {b.email || b.phone || "İletişim bilgisi yok"}
                        </span>
                      </div>
                      <Button
                        onClick={() => {
                          setModal(null)
                          setModalSearch("")
                          openBusiness(b.id)
                        }}
                        style={{ fontSize: "12px", padding: "7px 14px", height: "auto" }}
                      >
                        İşletmeyi Aç <Icon name="arrow" size={14} />
                      </Button>
                    </div>
                  ))
                ) : (
                  <div style={{ textAlign: "center", padding: "28px 12px", color: "var(--muted)" }}>
                    <Icon name="search" size={28} />
                    <p style={{ marginTop: "10px", fontSize: "13px" }}>
                      "{modalSearch}" aramasına uygun işletme bulunamadı.
                    </p>
                    <Button
                      onClick={() => setModalSearch("")}
                      style={{ marginTop: "12px", fontSize: "11px" }}
                    >
                      Aramayı Temizle
                    </Button>
                  </div>
                )}
              </div>
            ) : (
              <div style={{ textAlign: "center", padding: "36px 12px", color: "var(--muted)" }}>
                <Icon name="help" size={32} />
                <p style={{ marginTop: "12px", fontSize: "13px" }}>
                  Bu şablon henüz hiçbir işletmeye atanmamış.
                </p>
              </div>
            )}
          </div>
          <div
            className="modal-actions"
            style={{
              padding: "16px 28px",
              borderTop: "1px solid var(--line)",
              background: "#fafbfd",
              display: "flex",
              justifyContent: "flex-end",
            }}
          >
            <Button
              onClick={() => {
                setModal(null)
                setModalSearch("")
              }}
            >
              Kapat
            </Button>
          </div>
        </Modal>
      )}
      {toast && (
        <div className="toast" role="status">
          <Icon name="help" size={18} />
          {toast}
          <button aria-label="Bildirimi kapat" onClick={() => setToast("")}>
            <Icon name="close" size={16} />
          </button>
        </div>
      )}
    </div>
  )
}
