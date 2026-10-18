import React, { useState } from "react"
import QRCode from "qrcode"
import { Icon, Button, Badge, Field, Thumb } from "../../../shared/components"
import { money, date, today, currentSub, days, photoData } from "../repository"

import Empty from "./Empty"
export default function Subscriptions({ d, businesses, onEdit }) {
  const rows = d.subscriptions
    .filter((s) => !s.deleted && businesses.some((b) => b.id === s.business))
    .sort((a, b) => a.end.localeCompare(b.end))
  return (
    <section className="surface responsive-table">
      <table>
        <thead>
          <tr>
            <th>İşletme</th>
            <th>Başlangıç / bitiş</th>
            <th>Kalan süre</th>
            <th>Ödenen tutar</th>
            <th />
          </tr>
        </thead>
        <tbody>
          {rows.map((s) => (
            <tr key={s.id}>
              <td>
                <strong>
                  {businesses.find((b) => b.id === s.business)?.name}
                </strong>
                <small>{s.method || "Ödeme yöntemi belirtilmedi"}</small>
              </td>
              <td data-label="Tarihler">
                {date(s.start)}
                <small>{date(s.end)}</small>
              </td>
              <td data-label="Kalan süre">
                <span className={days(s.end) < 14 ? "amber-text" : ""}>
                  {s.start > today()
                    ? "Henüz başlamadı"
                    : days(s.end) < 0
                      ? "Süresi doldu"
                      : `${days(s.end)} gün`}
                </span>
              </td>
              <td data-label="Ödenen tutar">
                {s.amount === "" ? "—" : money(s.amount)}
              </td>
              <td>
                <Button onClick={() => onEdit(s)}>Düzenle / uzat</Button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
      {!rows.length && <Empty text="Abonelik bulunamadı" />}
    </section>
  )
}
