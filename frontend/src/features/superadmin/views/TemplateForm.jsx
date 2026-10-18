import React, {useRef,useState} from "react"
import {Button} from "../../../shared/components"
export default function TemplateForm({onSave,onClose}) {
  const folder=useRef(null),zip=useRef(null)
  const [selection,setSelection]=useState(null),[error,setError]=useState(''),[busy,setBusy]=useState(false)
  function choose(files,isZip){
    setError('');setSelection(null)
    if(!files.length)return
    if(isZip){if(!files[0].name.toLowerCase().endsWith('.zip')){setError('Hazırlanmış web paketini ZIP olarak seçin.');return}setSelection({archive:files[0]});return}
    const root=files[0].webkitRelativePath.split('/')[0]
    const paths=Array.from(files,f=>f.webkitRelativePath.slice(root.length+1))
    if(!paths.includes('template.json') || !paths.includes('index.html')){setError('Klasör kökünde index.html ve template.json bulunmalı. React kaynak klasörü yerine dist klasörünü seçin.');return}
    setSelection({files:Array.from(files),paths})
  }
  return <form onSubmit={async e=>{e.preventDefault();if(!selection || busy)return;setBusy(true);setError('');try{await onSave(selection)}catch(err){setError(err.message || 'Paket yüklenemedi.');setBusy(false)}}}>
    <div className="form-body"><p>React projenizin derlenmiş <strong>dist</strong> klasörünü veya bu klasörün içeriğini taşıyan ZIP dosyasını yükleyin.</p><p className="muted small">Paket kökü: index.html, template.json ve assets/. Şablon adı ve kodu manifestten alınır. En fazla 32 MB ve 1000 dosya.</p>
    <input ref={folder} type="file" webkitdirectory="" multiple hidden disabled={busy} onChange={e=>choose(e.target.files,false)}/><input ref={zip} type="file" accept=".zip" hidden disabled={busy} onChange={e=>choose(e.target.files,true)}/>
    <div style={{display:'flex',gap:12,flexWrap:'wrap',margin:'24px 0'}}><Button type="button" disabled={busy} onClick={()=>folder.current.click()}>Klasör seç</Button><Button type="button" disabled={busy} onClick={()=>zip.current.click()}>ZIP seç</Button></div>
    {selection && <p role="status">{selection.archive ? selection.archive.name : `${selection.files.length} dosya seçildi`}</p>}
    <p>Yüklenen şablon doğrudan işletmelere atanabilir. Önizlemesini burada kontrol edebilirsiniz; ayrıca etkinleştirme gerekmez.</p>{error && <p role="alert" className="error">{error}</p>}</div>
    <div className="modal-actions"><Button type="button" disabled={busy} onClick={onClose}>İptal</Button><Button type="submit" kind="primary" disabled={busy || !selection}>{busy?'Paket doğrulanıyor…':'Paketi yükle ve önizle'}</Button></div>
  </form>
}
