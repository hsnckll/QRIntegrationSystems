import { useEffect, useRef, useState } from "react"
import { Modal, Button } from "./components"

export default function useLeaveConfirmation() {
  const [action, setAction] = useState(null)
  const pending = useRef(null)
  useEffect(() => () => pending.current?.resolve(false), [])
  function confirmLeave(label = "Değişiklikleri bırak") {
    if (pending.current) return pending.current.promise
    const promise = new Promise(resolve => { pending.current = { resolve } })
    pending.current.promise = promise
    setAction(label)
    return promise
  }
  function finish(accepted) {
    pending.current?.resolve(accepted)
    pending.current = null
    setAction(null)
  }
  return { confirmLeave, leaveDialog: action && <Modal title="Kaydedilmemiş değişiklikler" onClose={() => finish(false)}>
    <div className="modal-body"><p>İşletme bilgilerindeki değişiklikler henüz kaydedilmedi. Ayrılırsanız bu değişiklikler kaybolacak.</p></div>
    <div className="modal-actions"><Button autoFocus onClick={() => finish(false)}>Düzenlemeye devam et</Button><Button kind="danger" onClick={() => finish(true)}>{action}</Button></div>
  </Modal> }
}
