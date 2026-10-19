import React, { useState } from "react"
import { Badge, Icon } from "../../shared/components"
import "./category-manager.css"

export default function CategoryManager({ categories, products, selected, onSelect, onCreate, onEdit, onDelete, onMove }) {
  const [expanded, setExpanded] = useState(false)
  const counts = new Map()
  products.forEach(product => counts.set(product.category, (counts.get(product.category) || 0) + 1))

  return <section className={`business-category-manager ${expanded ? "expanded" : ""}`} aria-label="Kategori yönetimi">
    <div className="business-category-quick">
      <select aria-label="Kategori seç" value={selected} onChange={event => onSelect(event.target.value === "all" ? "all" : Number(event.target.value))}>
        <option value="all">Tüm ürünler ({products.length})</option>
        {categories.map(category => <option key={category.id} value={category.id}>{category.name}</option>)}
      </select>
      <button type="button" className="button" aria-expanded={expanded} aria-controls="business-category-content" onClick={() => setExpanded(value => !value)}>
        <Icon name={expanded ? "close" : "edit"} size={16} />{expanded ? "Kategorileri gizle" : "Kategorileri yönet"}
      </button>
    </div>
    <div className="business-category-content" id="business-category-content">
      <header className="business-category-header">
        <div className="business-category-heading"><h2>Kategoriler</h2><span>{categories.length}</span></div>
        <button type="button" className="button business-category-create" onClick={onCreate}><Icon name="plus" size={20} />Kategori ekle</button>
      </header>
      <button type="button" className={`business-category-all ${selected === "all" ? "current" : ""}`} aria-pressed={selected === "all"} onClick={() => onSelect("all")}>
        <strong>Tüm ürünler</strong><span>{products.length}</span>
      </button>
      <div className="business-category-list">
        {categories.map((category, index) => <div className={`business-category-card ${selected === category.id ? "current" : ""}`} key={category.id}>
          <button type="button" className="business-category-select" aria-pressed={selected === category.id} onClick={() => onSelect(category.id)}>
            <span className="business-category-info"><strong>{category.name}</strong><span>{counts.get(category.id) || 0} ürün</span></span>
            <Badge active={category.active} />
          </button>
          <div className="business-category-actions" role="group" aria-label={`${category.name} kategori işlemleri`}>
            <button type="button" title={`${category.name} yukarı taşı`} aria-label={`${category.name} kategorisini yukarı taşı`} disabled={index === 0} onClick={() => onMove(category.id, -1)}><Icon name="up" size={16} /><span>Yukarı</span></button>
            <button type="button" title={`${category.name} aşağı taşı`} aria-label={`${category.name} kategorisini aşağı taşı`} disabled={index === categories.length - 1} onClick={() => onMove(category.id, 1)}><Icon name="down" size={16} /><span>Aşağı</span></button>
            <button type="button" title={`${category.name} düzenle`} aria-label={`${category.name} kategorisini düzenle`} onClick={() => onEdit(category)}><Icon name="edit" size={16} /><span>Düzenle</span></button>
            <button type="button" title={`${category.name} sil`} aria-label={`${category.name} kategorisini sil`} onClick={() => onDelete(category)}><Icon name="trash" size={16} /><span>Sil</span></button>
          </div>
        </div>)}
        {!categories.length && <p className="business-category-empty">Menünüzü düzenlemek için ilk kategorinizi ekleyin.</p>}
      </div>
    </div>
  </section>
}
