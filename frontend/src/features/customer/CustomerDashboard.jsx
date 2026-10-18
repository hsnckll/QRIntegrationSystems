import ProductWorkbench from "./ProductWorkbench"
import PlatformBrand from "../../shared/PlatformBrand"
import LoadingDashboard from "../../shared/LoadingDashboard"
import { buildPublicMenuUrl, getRootDomain } from "../../shared/session"
import React, { useState, useEffect } from "react"
import {
  Icon,
  Button,
  Badge,
  Modal,
  ProductForm,
  CategoryForm,
  Thumb,
} from "../../shared/components"
import {
  loadCustomerData,
  createCategory,
  updateCategory,
  deleteCategory,
  createProduct,
  updateProduct,
  deleteProduct,
  toggleProduct,
  updateBusinessProfile,
  money,
} from "./repository"
import { Profile, Subscription, Statistics } from "./views"
import MenuRenderer from "../../templates/MenuRenderer"
import { logout } from "../../shared/session"
import useLeaveConfirmation from "../../shared/useLeaveConfirmation"
import usePanelRoute from "../../shared/usePanelRoute"

const nav = [
  ["edit", "Düzenleme"],
  ["eye", "Önizleme"],
  ["user", "Bilgilerim"],
  ["calendar", "Abonelik"],
  ["chart", "İstatistikler"],
]

const emptyCustomerData = {
  business: {
    id: 0,
    name: "İşletme",
    slug: "",
    owner: "",
    phone: "",
    email: "",
    address: "",
    logo: "",
    banner: "",
    active: true,
    template: 1,
    qrUrl: "",
    qrImage: "",
  },
  categories: [],
  products: [],
  subscription: {
    start: "",
    end: "",
  },
}

export default function CustomerDashboard() {
  const [data, setData] = useState(emptyCustomerData),
    [loading, setLoading] = useState(true),
    [loadError, setLoadError] = useState(""),
    [syncing, setSyncing] = useState(false),
    [mobile, setMobile] = useState(false),
    [selected, setSelected] = useState("all"),
    [query, setQuery] = useState(""),
    [filter, setFilter] = useState("all"),
    [modal, setModal] = useState(null),
    [toast, setToast] = useState(""),
    [profileDirty, setProfileDirty] = useState(false),
    [manageCategories, setManageCategories] = useState(false),
    [showPreview, setShowPreview] = useState(false)
  const { confirmLeave, leaveDialog } = useLeaveConfirmation()
  const [route, navigateRoute] = usePanelRoute({
    pages: nav.map(([key]) => key), defaultPage: "edit",
    beforeChange: () => !profileDirty || confirmLeave(),
  })
  const page = route.page
  useEffect(() => { setProfileDirty(false); setMobile(false); setModal(null) }, [page])

  async function refreshData(isInitial = false, propagateError = false) {
    try {
      if (isInitial == true) {
        setLoading(true)
      } else {
        setSyncing(true)
      }
      const remote = await loadCustomerData()
      setData(remote)
      setLoadError("")
    } catch (err) {
      setLoadError(err.message || "İşletme verileri sunucudan alınamadı.")
      if (propagateError) throw err
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
      const t = setTimeout(() => setToast(""), 4200)
      return () => clearTimeout(t)
    }
  }, [toast])

  async function navigate(next) {
    if (!await navigateRoute({ page: next })) return
    setProfileDirty(false)
    setMobile(false)
  }

  async function signOut() {
    if (
      profileDirty == true &&
      !await confirmLeave("Çıkış yap")
    )
      return
    setProfileDirty(false)
    setMobile(false)
    setModal(null)
    setToast("")
    logout()
  }

  useEffect(() => {
    const f = (e) => {
      if (profileDirty == true) {
        e.preventDefault()
        e.returnValue = ""
      }
    }
    window.addEventListener("beforeunload", f)
    return () => window.removeEventListener("beforeunload", f)
  }, [profileDirty])

  async function move(collection, id, delta) {
    const list = [...data[collection]]
    const matching =
      collection === "products"
        ? list.filter(
            (x) => x.category === list.find((x) => x.id === id)?.category,
          )
        : list
    const index = matching.findIndex((x) => x.id === id)
    if (Boolean(matching[index + delta]) == false) return
    const a = list.findIndex((x) => x.id === id)
    const b = list.findIndex((x) => x.id === matching[index + delta].id)
    ;[list[a], list[b]] = [list[b], list[a]]

    setData({ ...data, [collection]: list })

    try {
      if (collection === "categories") {
        await Promise.all([
          updateCategory({ ...list[a], sortOrder: a }),
          updateCategory({ ...list[b], sortOrder: b }),
        ])
      } else {
        await Promise.all([
          updateProduct({ ...list[a], sortOrder: a }),
          updateProduct({ ...list[b], sortOrder: b }),
        ])
      }
      setToast("Sıralama güncellendi.")
    } catch {
      setToast("Sıralama kaydedilemedi.")
    }
  }

  async function handleToggleProduct(prod) {
    try {
      await toggleProduct(prod.id)
      setToast("Ürün görünürlüğü güncellendi.")
      await refreshData(false)
    } catch (err) {
      setToast(err.message || "Ürün durumu güncellenemedi.")
    }
  }

  async function handleSaveProfile(nextData) {
    try {
      await updateBusinessProfile(nextData.business)
      setToast("İşletme bilgileri güncellendi.")
      setProfileDirty(false)
      await refreshData(false)
      return true
    } catch (err) {
      setToast(err.message || "İşletme bilgileri kaydedilemedi.")
      return false
    }
  }

  if (loading == true) {
    return <LoadingDashboard title="İşletme paneli" navigationCount={nav.length} />
  }

  if (loadError !== "" && data.categories.length === 0 && data.products.length === 0) {
    return (
      <div
        className="app"
        style={{
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
          minHeight: "100vh",
        }}
      >
        <div
          className="surface"
          style={{
            maxWidth: "480px",
            textAlign: "center",
            padding: "32px",
            margin: "20px",
          }}
        >
          <h2
            style={{
              fontSize: "18px",
              color: "#e11d48",
              marginBottom: "12px",
            }}
          >
            Bağlantı Hatası
          </h2>
          <p
            style={{
              fontSize: "13px",
              color: "#64748b",
              marginBottom: "20px",
            }}
          >
            {loadError}
          </p>
          <Button kind="primary" onClick={() => refreshData(true)}>
            Yeniden Dene
          </Button>
        </div>
      </div>
    )
  }

  const products = data.products.filter(
    (p) =>
      (selected === "all" || p.category === selected) &&
      p.name.toLocaleLowerCase("tr").includes(query.toLocaleLowerCase("tr")) &&
      (filter === "all" || p.active === (filter === "active")),
  )
  const activeCount = data.products.filter(
    (p) =>
      p.active == true &&
      data.categories.find((c) => c.id === p.category)?.active == true,
  ).length

  const ownerName = data.business?.owner || data.business?.name || "İşletme"
  const ownerInitials =
    ownerName
      .split(" ")
      .filter((w) => Boolean(w) == true)
      .map((x) => x[0])
      .slice(0, 2)
      .join("")
      .toUpperCase() || "İP"

  return (
    <div className="app">
      {mobile == true && (
        <button
          className="scrim"
          aria-label="Menüyü kapat"
          onClick={() => setMobile(false)}
        />
      )}
      <aside className={`sidebar ${mobile == true ? "opened" : ""}`}>
        <a
          className="brand"
          href="#"
          onClick={(e) => {
            e.preventDefault()
            navigate("edit")
          }}
        >
          <PlatformBrand subtitle="İşletme paneli" />
        </a>

        <button
          className={`profile-shortcut ${page === "user" ? "selected" : ""}`}
          aria-label="Bilgilerim"
          aria-current={page === "user" ? "page" : undefined}
          onClick={() => navigate("user")}
        >
          <span className="avatar">{ownerInitials}</span>
          <span className="profile-text">
            <strong>{ownerName}</strong>
            <span>Bilgilerimi güncelle</span>
          </span>
          <Icon name="chevron" size={16} />
        </button>
        <span className="nav-label">Çalışma alanı</span>
        <nav>
          {nav
            .filter(([key]) => key !== "user")
            .map(([key, label]) => (
              <button
                key={key}
                className={page === key ? "selected" : ""}
                onClick={() => navigate(key)}
                aria-current={page === key ? "page" : undefined}
              >
                <Icon name={key} />
                {label}
                {page === key && <span className="nav-dot" />}
              </button>
            ))}
        </nav>
        <div className="sidebar-bottom">
          <div className="quiet-note">
            <Icon name="help" />
            <p>
              Menünüz her zaman elinizin altında.
              <span>Değişiklikleri önizlemede kontrol edebilirsiniz.</span>
            </p>
          </div>
          <div className="logout-area">
            <button className="logout-button" onClick={signOut}>
              <svg
                width="21"
                height="21"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="1.7"
                strokeLinecap="round"
                strokeLinejoin="round"
                aria-hidden="true"
              >
                <path d="M9 4H4v16h5M9 12h12m-4-4 4 4-4 4" />
              </svg>
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
              aria-expanded={mobile}
              onClick={() => setMobile(mobile == false)}
            >
              <Icon name="menu" />
            </button>
            <span>İşletme paneli</span>
            <Icon name="chevron" size={14} />
            <strong>{nav.find((n) => n[0] === page)?.[1]}</strong>
          </div>
          <div className="top-right">
            {syncing && <span className="muted small" role="status">Güncelleniyor…</span>}
            <button className="account-link" onClick={() => navigate("user")} aria-label="İşletme hesap bilgileri">{data.business.name}</button>
          </div>
        </header>
        <main style={{ paddingBottom: "48px" }}>
          <div className="page-heading">
            <div>
              <h1>
                {
                  {
                    edit: "Menünüzü düzenleyin",
                    eye: "Müşterinizin gözünden",
                    user: "İşletme bilgileriniz",
                    calendar: "Aboneliğiniz",
                    chart: "Menünüze gösterilen ilgi",
                  }[page]
                }
              </h1>
              <p>
                {
                  {
                    edit: "Kategorilerinizi ve ürünlerinizi tek bir yerden yönetin.",
                    eye: "Mevcut menünüzün müşterilerinize nasıl göründüğünü inceleyin.",
                    user: "İletişim bilgilerinizi güncel tutun.",
                    calendar:
                      "Abonelik tarihlerinizi ve kalan sürenizi takip edin.",
                    chart:
                      "QR okutmalarını takip edin, yoğun günlerinizi keşfedin.",
                  }[page]
                }
              </p>
            </div>
            {page === "edit" && (
              <Button icon="eye" onClick={() => window.innerWidth < 1400 ? navigate("eye") : setShowPreview(!showPreview)}>
                {showPreview ? "Önizlemeyi kapat" : "Menüyü önizle"}
              </Button>
            )}
          </div>
          {page === "edit" && (
            <>
              <div className="overview">
                <div>
                  <span className="overview-icon blue">
                    <Icon name="box" />
                  </span>
                  <div>
                    <strong>
                      {data.products.length}
                      <small>ürün</small>
                    </strong>
                    <span>Menünüzdeki tüm ürünler</span>
                  </div>
                </div>
                <div>
                  <span className="overview-icon green-bg">
                    <Icon name="check" />
                  </span>
                  <div>
                    <strong>
                      {activeCount}
                      <small>yayında</small>
                    </strong>
                    <span>Müşterilerinize görünen</span>
                  </div>
                </div>
                <div>
                  <span className="overview-icon purple">
                    <Icon name="edit" />
                  </span>
                  <div>
                    <strong>
                      {data.categories.length}
                      <small>kategori</small>
                    </strong>
                    <span>Menünüzün bölümleri</span>
                  </div>
                </div>
                <div className="overview-tip">
                  <span className="live-dot" />
                  <span>
                    Menünüz güncel<strong>Düzenleyin, önizlemede görün.</strong>
                  </span>
                </div>
              </div>
              <div className={`editor ${showPreview ? "with-preview" : ""}`}>
                <section
                  className={`category-panel ${
                    manageCategories == true ? "expanded" : ""
                  }`}
                >
                  <div className="mobile-category-controls">
                    <select
                      aria-label="Kategori seç"
                      value={selected}
                      onChange={(e) => setSelected(e.target.value === "all" ? "all" : Number(e.target.value))}
                    >
                      <option value="all">Tüm ürünler</option>
                      {data.categories.map((c) => (
                        <option value={c.id} key={c.id}>
                          {c.name}
                        </option>
                      ))}
                    </select>
                    <button
                      className="button"
                      aria-expanded={manageCategories}
                      onClick={() => setManageCategories(manageCategories == false)}
                    >
                      <Icon name="edit" size={16} />
                      {manageCategories == true ? "Kapat" : "Kategorileri yönet"}
                    </button>
                  </div>
                  <div className="panel-label">
                    <h2>Kategoriler</h2>
                    <span>{data.categories.length}</span>
                  </div>
                  <button
                    className={`category all ${
                      selected === "all" ? "current" : ""
                    }`}
                    onClick={() => setSelected("all")}
                  >
                    <Icon name="box" size={18} />
                    <strong>Tüm ürünler</strong>
                    <span>{data.products.length}</span>
                  </button>
                  <div className="category-list">
                    {data.categories.map((c, i) => (
                      <div
                        className={`category-wrap ${
                          selected === c.id ? "current" : ""
                        }`}
                        key={c.id}
                      >
                        <button
                          className="category"
                          onClick={() => setSelected(c.id)}
                        >
                          <span className="category-marker">
                            {["◈", "◉", "◇", "▤"][i % 4]}
                          </span>
                          <strong>
                            {c.name}
                            {c.active == false && <small>Pasif</small>}
                          </strong>
                          <span>
                            {
                              data.products.filter((p) => p.category === c.id)
                                .length
                            }
                          </span>
                        </button>
                        <div className="category-tools">
                          <button
                            title={`${c.name} yukarı taşı`}
                            disabled={i === 0}
                            onClick={() => move("categories", c.id, -1)}
                          >
                            <Icon name="up" size={14} /><span className="tool-label">Yukarı</span>
                          </button>
                          <button
                            title={`${c.name} aşağı taşı`}
                            disabled={i === data.categories.length - 1}
                            onClick={() => move("categories", c.id, 1)}
                          >
                            <Icon name="down" size={14} /><span className="tool-label">Aşağı</span>
                          </button>
                          <button
                            title={`${c.name} düzenle`}
                            onClick={() =>
                              setModal({ type: "category", item: c })
                            }
                          >
                            <Icon name="edit" size={14} /><span className="tool-label">Düzenle</span>
                          </button>
                          <button
                            title={`${c.name} sil`}
                            onClick={() =>
                              setModal({ type: "deleteCategory", item: c })
                            }
                          >
                            <Icon name="trash" size={14} /><span className="tool-label">Sil</span>
                          </button>
                        </div>
                      </div>
                    ))}
                  </div>
                  <Button
                    kind="category-add"
                    icon="plus"
                    onClick={() => setModal({ type: "category" })}
                  >
                    Kategori ekle
                  </Button>
                  <p className="category-hint">
                    Pasif kategoriler müşteri menüsünde görünmez.
                  </p>
                </section>
                <section className="product-panel">
                  <div className="product-header">
                    <div>
                      <h2>
                        {selected === "all"
                          ? "Tüm ürünler"
                          : data.categories.find((c) => c.id === selected)
                              ?.name}
                        <span>{products.length}</span>
                      </h2>
                      <p>
                        Ürün bilgilerinizi düzenleyin ve görünürlüğünü yönetin.
                      </p>
                    </div>
                    <Button
                      kind="primary"
                      icon="plus"
                      disabled={data.categories.length === 0}
                      onClick={() => setModal({ type: "product" })}
                    >
                      Ürün ekle
                    </Button>
                  </div>
                  <div className="toolbar">
                    <label className="search">
                      <Icon name="search" size={19} />
                      <input
                        aria-label="Ürün ara"
                        placeholder="Ürün adıyla ara…"
                        value={query}
                        onChange={(e) => setQuery(e.target.value)}
                      />
                      {Boolean(query) == true && (
                        <button
                          aria-label="Aramayı temizle"
                          onClick={() => setQuery("")}
                        >
                          <Icon name="close" size={16} />
                        </button>
                      )}
                    </label>
                    <select
                      aria-label="Ürün durumu"
                      value={filter}
                      onChange={(e) => setFilter(e.target.value)}
                    >
                      <option value="all">Tüm durumlar</option>
                      <option value="active">Aktif ürünler</option>
                      <option value="inactive">Pasif ürünler</option>
                    </select>
                  </div>
                  <ProductWorkbench products={products} categories={data.categories} onEdit={p=>setModal({type:'product',item:p})} onDelete={p=>setModal({type:'deleteProduct',item:p})} onSave={p=>updateProduct(p,data.products.find(x=>x.id===p.id))} onRefresh={()=>refreshData(false,true)} onMove={(p,direction)=>move('products',p.id,direction)} canMove={selected!=='all' && query==='' && filter==='all'} />
                  {products.length === 0 && (
                    <div className="empty">
                      <Icon name="search" size={32} />
                      <h3>
                        {query !== "" || filter !== "all"
                          ? "Eşleşen ürün yok"
                          : "Henüz ürün eklenmedi"}
                      </h3>
                      <p>
                        {query !== "" || filter !== "all"
                          ? "Aramanızı veya durum filtresini değiştirin."
                          : "Bir kategori seçip ilk ürününüzü ekleyin."}
                      </p>
                      <Button kind="primary" onClick={() => { if(query || filter!=="all"){setQuery("");setFilter("all")}else{setModal({type:data.categories.length ? "product" : "category"})} }}>{query || filter!=="all" ? "Filtreleri temizle" : data.categories.length ? "İlk ürünü ekle" : "İlk kategoriyi ekle"}</Button>
                    </div>
                  )}
                  <div className="table-footer">
                    <span>{products.length} ürün gösteriliyor</span>
                    <span>Fiyata tıklayarak düzenleyebilirsiniz</span>
                  </div>
                </section>
                {showPreview && <aside className="menu-live-preview"><div className="section-top"><h2>Müşteri görünümü</h2><button className="button" onClick={()=>setShowPreview(false)} aria-label="Önizlemeyi kapat">×</button></div><p>Kaydedilen menü gösterilir.</p><div className="live-preview-screen"><MenuRenderer templateName={data.business?.templateName || ""} data={data} embedded={true}/></div></aside>}
              </div>
            </>
          )}
          {page === "user" && (
            <Profile
              data={data}
              commit={handleSaveProfile}
              setDirty={setProfileDirty}
            />
          )}
          {page === "eye" && (
            <div className="preview-layout">
              <div className="preview-copy">
                <span className="section-symbol">
                  <Icon name="eye" size={28} />
                </span>
                <h2>Menünüz hazır.</h2>
                <p>
                  Müşterileriniz yalnızca aktif kategorileri ve ürünleri görür.
                  Düzenlemeleriniz burada anında görünür.
                </p>
                <div className="preview-address">
                  <Icon name="link" size={17} />
                  <span>
                    {Boolean(data.business.slug) == true
                      ? `https://${data.business.slug}.${getRootDomain()}`
                      : data.business.qrUrl || ""}
                  </span>
                </div>
                <a
                  className="button primary"
                  href={buildPublicMenuUrl(data.business.slug, data.business.qrUrl)}
                  target="_blank"
                  rel="noreferrer"
                >
                  Menüyü yeni sekmede aç <Icon name="arrow" size={17} />
                </a>
                <div className="note">
                  <Icon name="help" />
                  <p>
                    Menünüzün müşteri görünümünü inceleyin. Kaydedilen değişiklikler menü yeniden açıldığında görüntülenir.
                  </p>
                </div>
              </div>
              <div className="phone">
                <div className="phone-bar">
                  <span>9:41</span>
                  <span>● ▰</span>
                </div>
                <MenuRenderer
                  templateName={data.business?.templateName || ""}
                  data={data}
                  embedded={true}
                />
              </div>
            </div>
          )}
          {page === "calendar" && (
            <Subscription subscription={data.subscription} business={data.business} />
          )}
          {page === "chart" && <Statistics />}
        </main>
      </div>
      {leaveDialog}
      {modal?.type === "product" && (
        <ProductForm
          item={modal.item}
          categories={data.categories}
          selected={selected}
          onClose={() => setModal(null)}
          onSave={async (p) => {
            try {
              if (modal.item) {
                await updateProduct(p, modal.item)
                setToast("Ürün başarıyla güncellendi.")
              } else {
                await createProduct(p)
                setToast("Ürün başarıyla eklendi.")
              }
              setModal(null)
              await refreshData(false)
            } catch (err) {
              setToast(err.message || "Ürün kaydedilemedi.")
            }
          }}
        />
      )}
      {modal?.type === "category" && (
        <CategoryForm
          item={modal.item}
          onClose={() => setModal(null)}
          onSave={async (c) => {
            try {
              if (modal.item) {
                await updateCategory({ ...modal.item, ...c })
                setToast("Kategori başarıyla güncellendi.")
              } else {
                await createCategory(c)
                setToast("Kategori başarıyla eklendi.")
              }
              setModal(null)
              await refreshData(false)
            } catch (err) {
              setToast(err.message || "Kategori kaydedilemedi.")
            }
          }}
        />
      )}
      {modal?.type.startsWith("delete") && (
        <Modal
          title={
            modal.type === "deleteCategory"
              ? "Kategori silinsin mi?"
              : "Ürün silinsin mi?"
          }
          onClose={() => setModal(null)}
        >
          <div className="modal-body">
            <p>
              <strong>{modal.item.name}</strong>{" "}
              {modal.type === "deleteCategory"
                ? `ve içindeki ${data.products.filter((p) => p.category === modal.item.id).length} ürün silinecek.`
                : "menünüzden silinecek."}{" "}
              Bu işlem geri alınamaz.
            </p>
          </div>
          <div className="modal-actions">
            <Button onClick={() => setModal(null)}>Vazgeç</Button>
            <Button
              kind="danger"
              icon="trash"
              onClick={async () => {
                try {
                  if (modal.type === "deleteCategory") {
                    await deleteCategory(modal.item.id)
                    if (selected === modal.item.id) setSelected("all")
                    setToast("Kategori başarıyla silindi.")
                  } else {
                    await deleteProduct(modal.item.id)
                    setToast("Ürün başarıyla silindi.")
                  }
                  setModal(null)
                  await refreshData(false)
                } catch (err) {
                  setToast(err.message || "Silme işlemi tamamlanamadı.")
                }
              }}
            >
              Evet, sil
            </Button>
          </div>
        </Modal>
      )}
      {Boolean(toast) == true && (
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
