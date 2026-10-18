import { useEffect, useRef, useState } from "react"
import { panelHash, readPanelRoute } from "./panel-route"

export default function usePanelRoute({ pages, defaultPage, tabs = [], beforeChange = () => true }) {
  const normalize = value => readPanelRoute(panelHash(value), pages, defaultPage, tabs)
  const [route, setRoute] = useState(() => readPanelRoute(location.hash, pages, defaultPage, tabs))
  const current = useRef(route), guard = useRef(beforeChange), index = useRef(history.state?.qrPanelIndex || 0)
  const reverting = useRef(false), pending = useRef(false), restored = useRef(null)
  guard.current = beforeChange
  useEffect(() => {
    const initial = readPanelRoute(location.hash, pages, defaultPage, tabs)
    current.current = initial; setRoute(initial)
    history.replaceState({ ...history.state, qrPanelIndex: index.current }, "", location.pathname + location.search + panelHash(current.current))
    const changed = async () => {
      const next = readPanelRoute(location.hash, pages, defaultPage, tabs)
      if (reverting.current) { if (panelHash(next) === panelHash(current.current)) { reverting.current = false; restored.current?.(); restored.current = null } return }
      if (panelHash(next) === panelHash(current.current)) return
      const nextIndex = history.state?.qrPanelIndex ?? index.current
      if (pending.current) return
      const decision = guard.current()
      if (decision instanceof Promise) {
        pending.current = true
        const delta = index.current - nextIndex
        const restoration = delta ? new Promise(resolve => { restored.current = resolve; reverting.current = true; history.go(delta) }) : Promise.resolve()
        if (!delta) history.replaceState({ ...history.state, qrPanelIndex: index.current }, "", location.pathname + location.search + panelHash(current.current))
        try {
          const accepted = await decision
          await restoration
          if (accepted) {
            reverting.current = true
            if (delta) {
              current.current = next; index.current = nextIndex; setRoute(next); history.go(-delta)
            } else {
              current.current = next; setRoute(next)
              history.replaceState({ ...history.state, qrPanelIndex: index.current }, "", location.pathname + location.search + panelHash(next))
              reverting.current = false
            }
          }
        } finally { pending.current = false }
        return
      }
      if (!decision) {
        const delta = index.current - nextIndex
        if (delta) { reverting.current = true; history.go(delta) }
        else history.replaceState({ ...history.state, qrPanelIndex: index.current }, "", location.pathname + location.search + panelHash(current.current))
        return
      }
      index.current = nextIndex; current.current = next; setRoute(next)
    }
    window.addEventListener("popstate", changed); window.addEventListener("hashchange", changed)
    return () => { window.removeEventListener("popstate", changed); window.removeEventListener("hashchange", changed) }
  }, [])
  async function navigate(patch, { replace = false } = {}) {
    const next = normalize({ ...current.current, ...patch })
    if (panelHash(next) === panelHash(current.current) || pending.current) return false
    pending.current = true
    let accepted
    try { accepted = await guard.current() } finally { pending.current = false }
    if (!accepted) return false
    if (!replace) index.current++
    history[replace ? "replaceState" : "pushState"]({ ...history.state, qrPanelIndex: index.current }, "", location.pathname + location.search + panelHash(next))
    current.current = next; setRoute(next); return true
  }
  return [route, navigate]
}
