import React, { useEffect, useRef, useState } from "react"
import { request } from "../services/api/http"

export function publicMenu(data, menuData) {
  const source = menuData || {
    businessName: data?.business?.name || "Şablon önizlemesi",
    logoPath: data?.business?.logo || null,
    bannerPath: data?.business?.banner || null,
    templateName: data?.business?.templateName || "",
    categories: (data?.categories || []).filter(c=>c.active).map(c=>({id:c.id,name:c.name,sortOrder:c.sortOrder || 0,products:(data?.products || []).filter(p=>p.active && p.category===c.id).map(p=>({id:p.id,name:p.name,description:p.description || null,price:p.price,imagePath:p.image || null,sortOrder:p.sortOrder || 0}))}))
  }
  const image = value => {
    if (!value) return null
    try { const u = new URL(value, location.origin); return ["http:","https:"].includes(u.protocol) ? u.href : null } catch { return null }
  }
  return {...source,logoPath:image(source.logoPath),bannerPath:image(source.bannerPath),categories:(source.categories || []).map(c=>({...c,products:(c.products || []).map(p=>({...p,imagePath:image(p.imagePath)}))}))}
}
export default function MenuRenderer({templateName="",data,menuData,embedded=false,onReady}) {
  const frame=useRef(null),port=useRef(null),latest=useRef(null),readyCallback=useRef(onReady),handshake=useRef(null)
  const [url,setUrl]=useState(""),[error,setError]=useState(""),[connected,setConnected]=useState(false),[retry,setRetry]=useState(0)
  latest.current=publicMenu(data,menuData);readyCallback.current=onReady
  useEffect(()=>{
    let cancelled=false
    setUrl("");setError("");setConnected(false);clearInterval(handshake.current);port.current?.close();port.current=null
    if(!templateName){setError("Bu işletmeye henüz bir şablon atanmadı.");return}
    request(`template/runtime/${encodeURIComponent(templateName)}`,{auth:Boolean(data) || new URLSearchParams(location.search).has("previewTemplate")}).then(result=>{
      if(cancelled)return
      const entry=new URL(result.entryUrl,location.origin)
      if(entry.origin!==location.origin || !entry.pathname.startsWith('/api/template/assets/'))throw Error('Geçersiz şablon adresi.')
      setUrl(entry.href)
    }).catch(err=>{if(!cancelled)setError(err.message || "Şablon yüklenemedi.")})
    return ()=>{cancelled=true;clearInterval(handshake.current);port.current?.close();port.current=null}
  },[templateName,retry])
  useEffect(()=>{if(connected)port.current?.postMessage({type:'okutye:menu',protocolVersion:1,menu:latest.current})},[data,menuData,connected])
  useEffect(()=>{if(!url || connected)return;const timer=setTimeout(()=>{clearInterval(handshake.current);port.current?.close();setError("Şablon menü bağlantısına yanıt vermedi. Paket v1 bağlantısını kontrol edin.")},12000);return ()=>clearTimeout(timer)},[url,connected])
  function initialize(){
    port.current?.close()
    const channel=new MessageChannel();port.current=channel.port1
    channel.port1.onmessage=e=>{
      if(e.data?.type!=='okutye:ready' || e.data.protocolVersion!==1)return
      clearInterval(handshake.current);setError("");setConnected(true)
      channel.port1.postMessage({type:'okutye:menu',protocolVersion:1,menu:latest.current})
      readyCallback.current?.()
    }
    frame.current?.contentWindow?.postMessage({type:'okutye:init',protocolVersion:1},'*',[channel.port2])
  }
  return <div className={`package-menu ${embedded?'embedded':''}`} style={{width:'100%',height:embedded?'100%':'100dvh',minHeight:embedded?480:undefined,position:'relative',background:'#f6f8fb'}}>
    {error ? <div role="alert" style={{padding:24,textAlign:'center'}}><h2>Menü görüntülenemiyor</h2><p>{error}</p>{templateName && <button className="button" onClick={()=>setRetry(n=>n+1)}>Yeniden dene</button>}</div> : !connected && <p role="status" style={{padding:20}}>Menü yükleniyor…</p>}
    {url && !error && <iframe ref={frame} src={url} onLoad={()=>{clearInterval(handshake.current);initialize();handshake.current=setInterval(initialize,500)}} title="İşletme menüsü" sandbox="allow-scripts" referrerPolicy="no-referrer" style={{border:0,width:'100%',height:'100%',display:'block',position:connected?'relative':'absolute',inset:0,visibility:connected?'visible':'hidden'}}/>}
  </div>
}
