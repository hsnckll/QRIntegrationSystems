import React, { useState } from "react"
import QRCode from "qrcode"
import { Icon, Button, Badge, Field, Thumb } from "../../../shared/components"
import { money, date, today, currentSub, days, photoData } from "../repository"

import Search from "./Search"
import Empty from "./Empty"
import "./menu-editor.css"
export default function MenuEditor({ d, b, setModal, save, move }) {
  const [category, setCategory] = useState("all"),
    [query, setQuery] = useState("")
  const categories = d.categories.filter((c) => c.business === b.id),
    products = d.products.filter(
      (p) =>
        p.business === b.id &&
        (category === "all" || p.category === category) &&
        p.name.toLocaleLowerCase("tr").includes(query.toLocaleLowerCase("tr")),
    )
  return (
    <div className="admin-editor">
      <section className="surface category-manager" aria-label="Kategori yönetimi">
        <div className="section-top">
          <div className="category-manager-heading"><h2>Kategoriler</h2><span>{categories.length}</span></div>
          <button
            type="button"
            className="button category-create-button"
            title="Kategori ekle"
            onClick={() => setModal({ type: "category", business: b.id })}
          >
            <Icon name="plus" /><span>Kategori ekle</span>
          </button>
        </div>
        <button
          type="button"
          className={`category category-all ${category === "all" ? "current" : ""}`}
          aria-pressed={category === "all"}
          onClick={() => setCategory("all")}
        >
          <strong>Tüm ürünler</strong><span className="category-product-count">{d.products.filter(p => p.business === b.id).length}</span>
        </button>
        <div className="category-manager-list">
        {categories.map((c, i) => (
          <div className={`managed-category ${category === c.id ? "current" : ""}`} key={c.id}>
            <button
              type="button"
              className={`category ${category === c.id ? "current" : ""}`}
              aria-pressed={category === c.id}
              onClick={() => setCategory(c.id)}
            >
              <span className="category-details"><strong>{c.name}</strong><span className="category-product-count">{d.products.filter(p => p.business === b.id && p.category === c.id).length} ürün</span></span>
              <Badge active={c.active} />
            </button>
            <div className="category-tools" role="group" aria-label={`${c.name} kategori işlemleri`}>
              <button
                type="button"
                title={`${c.name} yukarı taşı`}
                aria-label={`${c.name} kategorisini yukarı taşı`}
                disabled={i === 0}
                onClick={() => move("categories", c, -1)}
              >
                <Icon name="up" /><span className="tool-label">Yukarı</span>
              </button>
              <button
                type="button"
                title={`${c.name} aşağı taşı`}
                aria-label={`${c.name} kategorisini aşağı taşı`}
                disabled={i === categories.length - 1}
                onClick={() => move("categories", c, 1)}
              >
                <Icon name="down" /><span className="tool-label">Aşağı</span>
              </button>
              <button
                type="button"
                title={`${c.name} düzenle`}
                aria-label={`${c.name} kategorisini düzenle`}
                onClick={() =>
                  setModal({ type: "category", item: c, business: b.id })
                }
              >
                <Icon name="edit" /><span className="tool-label">Düzenle</span>
              </button>
              <button
                type="button"
                title={`${c.name} sil`}
                aria-label={`${c.name} kategorisini sil`}
                onClick={() => setModal({ type: "deleteCategory", item: c })}
              >
                <Icon name="trash" /><span className="tool-label">Sil</span>
              </button>
            </div>
          </div>
        ))}
        {!categories.length && <p className="category-manager-empty">Menünüzü düzenlemek için ilk kategorinizi ekleyin.</p>}
        </div>
      </section>
      <section className="surface">
        <div className="section-top">
          <h2>
            Ürünler <span className="muted">({products.length})</span>
          </h2>
          <Button
            kind="primary"
            icon="plus"
            disabled={!categories.length}
            onClick={() =>
              setModal({ type: "product", business: b.id, category })
            }
          >
            Ürün ekle
          </Button>
        </div>
        <div className="list-toolbar">
          <Search value={query} onChange={setQuery} placeholder="Ürün ara…" />
        </div>
        {products.map((p) => (
          <div className="managed-product" key={p.id}>
            <Thumb product={p} />
            <div className="managed-product-info">
              <strong>{p.name}</strong>
              <p>{p.description}</p>
              <span>{money(p.price)}</span>
            </div>
            <div className="managed-product-tools">
              <button
                title={`${p.name} durumunu değiştir`}
                onClick={() =>
                  save(
                    "products",
                    { ...p, active: !p.active },
                    b.id,
                    "Ürün durumu güncellendi",
                  )
                }
              >
                <Badge active={p.active} />
              </button>
              <button
                className="icon-button"
                title={`${p.name} düzenle`}
                onClick={() =>
                  setModal({ type: "product", item: p, business: b.id })
                }
              >
                <Icon name="edit" /><span className="tool-label">Düzenle</span>
              </button>
              <button
                className="icon-button"
                title={`${p.name} sil`}
                onClick={() => setModal({ type: "deleteProduct", item: p })}
              >
                <Icon name="trash" /><span className="tool-label">Sil</span>
              </button>
              <button
                className="icon-button"
                title={`${p.name} yukarı taşı`}
                disabled={
                  d.products.filter((x) => x.category === p.category)[0]?.id ===
                  p.id
                }
                onClick={() => move("products", p, -1)}
              >
                <Icon name="up" /><span className="tool-label">Yukarı</span>
              </button>
              <button
                className="icon-button"
                title={`${p.name} aşağı taşı`}
                disabled={
                  d.products.filter((x) => x.category === p.category).at(-1)
                    ?.id === p.id
                }
                onClick={() => move("products", p, 1)}
              >
                <Icon name="down" /><span className="tool-label">Aşağı</span>
              </button>
            </div>
          </div>
        ))}
        {!products.length && <Empty text="Bu görünümde ürün yok" />}
      </section>
    </div>
  )
}
