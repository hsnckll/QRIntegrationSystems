import React, { useState, useEffect } from "react"
import { Button, Badge, Thumb } from "../../shared/components"
import { money } from "./repository"

export default function ProductWorkbench({ products, categories, onEdit, onDelete, onSave, onRefresh, onMove, canMove }) {
  const [selected,setSelected]=useState([]), [priceEdit,setPriceEdit]=useState(null), [price,setPrice]=useState(""), [busy,setBusy]=useState(false), [message,setMessage]=useState("")
  useEffect(()=>{setSelected(ids=>ids.filter(id=>products.some(p=>p.id===id)))},[products])
  async function apply(items) {
    if(busy)return
    setBusy(true);setMessage("Kaydediliyor…")
    const results=await Promise.allSettled(items.map(onSave))
    const failed=results.filter(r=>r.status==='rejected').length
    try { await onRefresh(); setMessage(failed ? `${items.length-failed} ürün kaydedildi. ${failed} ürün kaydedilemedi; yeniden deneyin.` : "Değişiklikler kaydedildi."); if(!failed){setSelected([]);setPriceEdit(null)} }
    catch {setMessage("Değişikliklerden sonra liste yenilenemedi. Sayfayı yenileyin.")}
    finally{setBusy(false)}
  }
  return <div className="product-workbench">
    {products.length>0 && <div className="bulk-toolbar">
      <label><input type="checkbox" disabled={busy} checked={selected.length===products.length} onChange={e=>setSelected(e.target.checked?products.map(p=>p.id):[])} /> Görünen ürünleri seç</label>
      {selected.length>0 && <><span>{selected.length} seçili</span><Button disabled={busy} onClick={()=>apply(products.filter(p=>selected.includes(p.id)).map(p=>({...p,active:true})))}>Aktif yap</Button><Button disabled={busy} onClick={()=>apply(products.filter(p=>selected.includes(p.id)).map(p=>({...p,active:false})))}>Pasif yap</Button><Button disabled={busy} onClick={()=>setSelected([])}>Seçimi temizle</Button></>}
    </div>}
    {message && <p className="save-feedback" role="status">{message}</p>}
    {products.map((p,i)=><article className="workbench-row" key={p.id}>
      <label className="product-selection"><input type="checkbox" aria-label={`${p.name} seç`} disabled={busy} checked={selected.includes(p.id)} onChange={e=>setSelected(ids=>e.target.checked?[...ids,p.id]:ids.filter(id=>id!==p.id))} /></label>
      <Thumb product={p}/><div className="workbench-info"><strong>{p.name}</strong><p>{p.description || "Açıklama eklenmedi"}</p><small>{categories.find(c=>c.id===p.category)?.name}</small></div>
      <div className="workbench-price">{priceEdit===p.id ? <form onSubmit={e=>{e.preventDefault();if(price!==''&&Number(price)>=0)apply([{...p,price:Number(price)}])}}><input aria-label={`${p.name} fiyatı`} type="number" min="0" step="0.01" required value={price} disabled={busy} onChange={e=>setPrice(e.target.value)} autoFocus/><Button type="submit" disabled={busy}>Kaydet</Button><Button type="button" disabled={busy} onClick={()=>setPriceEdit(null)}>Vazgeç</Button></form> : <button title={`${p.name} fiyatını düzenle`} disabled={busy} onClick={()=>{setPriceEdit(p.id);setPrice(String(p.price));setMessage("")}}><strong>{money(p.price)}</strong><small>Fiyatı düzenle</small></button>}</div>
      <button className="workbench-status" disabled={busy} aria-label={`${p.name} ${p.active?'pasif':'aktif'} yap`} onClick={()=>apply([{...p,active:!p.active}])}><Badge active={p.active}/></button>
      <div className="workbench-actions"><Button disabled={busy} onClick={()=>onEdit(p)}>Düzenle</Button><Button disabled={busy} onClick={()=>onDelete(p)}>Sil</Button>{canMove && <><Button disabled={busy||i===0} aria-label={`${p.name} yukarı taşı`} onClick={()=>onMove(p,-1)}>↑</Button><Button disabled={busy||i===products.length-1} aria-label={`${p.name} aşağı taşı`} onClick={()=>onMove(p,1)}>↓</Button></>}</div>
    </article>)}
  </div>
}
