import React, { useState } from "react"
import QRCode from "qrcode"
import { Icon, Button, Badge, Field, Thumb } from "../../../shared/components"
import { money, date, today, currentSub, days, photoData } from "../repository"

import Empty from "./Empty"
import { logSummary } from "./LogChanges"
export default function Logs({ logs, byId, onInspect, title = "Tüm değişiklikler" }) {
  return (
    <section className="surface">
      <div className="section-top">
        <h2>{title}</h2>
        <span className="muted small">{logs.length} kayıt</span>
      </div>
      {logs.map((l) => (
        <button className="log-row" key={l.id} onClick={() => onInspect(l)}>
          <span className="log-icon">
            <Icon name="edit" size={19} />
          </span>
          <span>
            <strong>{l.action}</strong>
            <span className="log-summary">{logSummary(l)}</span>
            <small>
              {byId(l.business)?.name || "Yönetici hesabı"} · {l.actor}
            </small>
          </span>
          <time>
            {new Date(l.at).toLocaleString("tr-TR", {
              day: "numeric",
              month: "short",
              hour: "2-digit",
              minute: "2-digit",
            })}
          </time>
          <Icon name="chevron" size={16} />
        </button>
      ))}
      {!logs.length && <Empty text="Henüz işlem kaydı yok" />}
    </section>
  )
}
