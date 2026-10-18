import React, { useState, useEffect, useRef, useMemo } from "react"

const paths = {
  edit: "M4 7h16M4 12h10M4 17h7 M17 16l3-3 2 2-3 3-3 1z",
  user: "M16 7a4 4 0 1 1-8 0 4 4 0 0 1 8 0M4 21v-3a8 8 0 0 1 16 0v3",
  eye: "M2 12s4-7 10-7 10 7 10 7-4 7-10 7S2 12 2 12M15 12a3 3 0 1 1-6 0 3 3 0 0 1 6 0",
  calendar: "M5 5h14v16H5zM8 2v6M16 2v6M5 10h14",
  chart: "M4 3v17h17M8 16v-5M13 16V7M18 16V4",
  search: "M16 10a6 6 0 1 1-12 0 6 6 0 0 1 12 0M15 15l6 6",
  plus: "M12 5v14M5 12h14",
  arrow: "M5 12h14M14 7l5 5-5 5",
  chevron: "m9 5 7 7-7 7",
  close: "m6 6 12 12M6 18 18 6",
  menu: "M4 6h16M4 12h16M4 18h16",
  check: "m5 12 4 4L19 6",
  box: "m12 3 9 5v9l-9 5-9-5V8zM3 8l9 5 9-5M12 13v9",
  trash: "M3 6h18M9 6V3h6v3M6 6l1 15h10l1-15M10 10v7M14 10v7",
  upload: "M12 16V3m-5 5 5-5 5 5M4 15v6h16v-6",
  up: "m6 14 6-6 6 6",
  down: "m6 10 6 6 6-6",
  link: "M10 7l2-2a5 5 0 0 1 7 7l-2 2M14 17l-2 2a5 5 0 0 1-7-7l2-2M8 16l8-8",
  help: "M9 9a3 3 0 1 1 5 2c-2 1-2 2-2 3M12 17h.01M22 12a10 10 0 1 1-20 0 10 10 0 0 1 20 0",
  creditCard: "M3 7a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2v10a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V7z M3 10h18 M7 15h3",
}
function Icon({ name, size = 20 }) {
  return (
    <svg
      width={size}
      height={size}
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.65"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
    >
      <path d={paths[name] || paths.box} />
    </svg>
  )
}
const nav = [
  ["edit", "Düzenleme"],
  ["eye", "Önizleme"],
  ["user", "Bilgilerim"],
  ["calendar", "Abonelik"],
  ["chart", "İstatistikler"],
]
function Button({ children, icon, kind = "", ...props }) {
  return (
    <button className={`button ${kind}`} {...props}>
      {icon && <Icon name={icon} size={18} />} {children}
    </button>
  )
}
function Badge({ active, children }) {
  return (
    <span className={`badge ${active ? "green" : "gray"}`}>
      <i />
      {children || (active ? "Aktif" : "Pasif")}
    </span>
  )
}
function Field({ label, children, ...props }) {
  return (
    <label className="field">
      <span>{label}</span>
      {children ? (
        React.cloneElement(children, { "aria-label": label })
      ) : (
        <input aria-label={label} {...props} />
      )}
    </label>
  )
}
function Modal({ title, children, onClose, className = "" }) {
  const ref = useRef()
  useEffect(() => {
    const el = ref.current
    el.showModal()
    return () => el.close()
  }, [])
  return (
    <dialog
      className={className}
      aria-label={title}
      ref={ref}
      onCancel={(e) => {
        e.preventDefault()
        onClose()
      }}
      onClick={(e) => {
        if (e.target === ref.current) onClose()
      }}
    >
      <header>
        <div>
          <h2>{title}</h2>
        </div>
        <button className="icon-button" aria-label="Kapat" onClick={onClose}>
          <Icon name="close" />
        </button>
      </header>
      {children}
    </dialog>
  )
}

import { photoData } from "./media"
function Thumb({ product }) {
  return product.image ? (
    <img
      className="thumb"
      src={product.image}
      alt={product.name}
      loading="lazy"
    />
  ) : (
    <div className={`thumb placeholder ${product.category}`}>
      <Icon name="box" size={24} />
    </div>
  )
}
function CategoryForm({ item, onClose, onSave }) {
  const [name, setName] = useState(item?.name || ""),
    [active, setActive] = useState(item?.active ?? true)
  return (
    <Modal
      title={item ? "Kategoriyi düzenle" : "Yeni kategori"}
      onClose={onClose}
    >
      <form
        onSubmit={(e) => {
          e.preventDefault()
          if (name.trim())
            onSave({
              id: item?.id || crypto.randomUUID(),
              name: name.trim(),
              active,
            })
        }}
      >
        <div className="modal-body">
          <Field
            label="Kategori adı"
            value={name}
            onChange={(e) => setName(e.target.value)}
            required
            maxLength={80}
            autoFocus
            placeholder="Örn. Başlangıçlar"
          />
          <label className="check-field">
            <input
              type="checkbox"
              checked={active}
              onChange={(e) => setActive(e.target.checked)}
            />
            Kategori menüde görünsün
          </label>
        </div>
        <div className="modal-actions">
          <Button onClick={onClose} type="button">
            Vazgeç
          </Button>
          <Button kind="primary" type="submit">
            Kategoriyi kaydet
          </Button>
        </div>
      </form>
    </Modal>
  )
}
function ProductForm({ item, categories, selected, onClose, onSave }) {
  const [p, setP] = useState(
      item || {
        id: crypto.randomUUID(),
        name: "",
        description: "",
        price: "",
        category: selected === "all" ? categories[0]?.id : selected,
        active: true,
        image: "",
      },
    ),
    [error, setError] = useState(""),
    [busy, setBusy] = useState(false)
  const set = (key, value) => setP((x) => ({ ...x, [key]: value }))
  return (
    <Modal className="product-drawer" title={item ? "Ürünü düzenle" : "Yeni ürün"} onClose={onClose}>
      <form
        onSubmit={(e) => {
          e.preventDefault()
          if (Boolean(p.name.trim()) == false) {
            setError("Ürün adı gerekli.")
            return
          }
          if (
            Number.isFinite(Number(p.price)) == false ||
            p.price === "" ||
            Number(p.price) < 0
          ) {
            setError("Geçerli bir fiyat girin.")
            return
          }
          onSave({ ...p, name: p.name.trim(), price: Number(p.price) })
        }}
      >
        <div className="modal-body">
          <div className="photo-picker">
            {p.image ? (
              <img src={p.image} alt="Ürün fotoğrafı önizlemesi" />
            ) : (
              <span className="photo-empty">
                <Icon name="upload" size={26} />
              </span>
            )}
            <div>
              <label className="button">
                <Icon name="upload" size={16} />
                {busy ? "Fotoğraf hazırlanıyor…" : "Fotoğraf seç"}
                <input
                  type="file"
                  aria-label="Ürün fotoğrafı"
                  accept="image/png,image/jpeg,image/webp"
                  disabled={busy}
                  onChange={async (e) => {
                    const file = e.target.files[0]
                    if (Boolean(file) == false) return
                    setBusy(true)
                    setError("")
                    try {
                      set("image", await photoData(file))
                    } catch (err) {
                      setError(err.message)
                    } finally {
                      setBusy(false)
                      e.target.value = ""
                    }
                  }}
                />
              </label>
              <small>JPEG, PNG, WebP · En fazla 8 MB</small>
              {p.image && (
                <button
                  type="button"
                  className="text-button"
                  onClick={() => set("image", "")}
                >
                  Fotoğrafı kaldır
                </button>
              )}
            </div>
          </div>
          <Field
            label="Ürün adı"
            value={p.name}
            onChange={(e) => set("name", e.target.value)}
            required
            maxLength={150}
            placeholder="Örn. Hatay Usulü Tavuk Dürüm"
          />
          <Field label="Açıklama">
            <textarea
              value={p.description}
              onChange={(e) => set("description", e.target.value)}
              rows={3}
              maxLength={500}
              placeholder="İçeriği ve sunumu hakkında kısa bir açıklama"
            />
          </Field>
          <div className="form-grid">
            <Field
              label="Fiyat (TL)"
              type="number"
              min="0"
              step="0.01"
              value={p.price}
              onChange={(e) => set("price", e.target.value)}
              required
            />
            <Field label="Kategori">
              <select
                value={p.category}
                onChange={(e) => set("category", e.target.value)}
              >
                {categories.map((c) => (
                  <option key={c.id} value={c.id}>
                    {c.name}
                  </option>
                ))}
              </select>
            </Field>
          </div>
          <label className="check-field">
            <input
              type="checkbox"
              checked={p.active}
              onChange={(e) => set("active", e.target.checked)}
            />
            Ürün menüde görünsün
          </label>
          {error && (
            <p className="error" role="alert">
              {error}
            </p>
          )}
        </div>
        <div className="modal-actions">
          <Button type="button" onClick={onClose}>
            Vazgeç
          </Button>
          <Button kind="primary" type="submit" disabled={busy}>
            Ürünü kaydet
          </Button>
        </div>
      </form>
    </Modal>
  )
}

export { Icon, Button, Badge, Field, Modal, ProductForm, CategoryForm, Thumb }
