import React, { useState } from "react"
import QRCode from "qrcode"
import { Icon, Button, Badge, Field, Thumb } from "../../../shared/components"
import { money, date, today, currentSub, days, photoData } from "../repository"

import Initial from "./Initial"
import Empty from "./Empty"
export default function BusinessList({ list, d, onOpen }) {
  return (
    <div className="responsive-table">
      <table>
        <thead>
          <tr>
            <th>İşletme</th>
            <th>Şablon</th>
            <th>Abonelik bitişi</th>
            <th>Durum</th>
            <th />
          </tr>
        </thead>
        <tbody>
          {list.map((b) => {
            const s = currentSub(d, b.id)
            return (
              <tr key={b.id}>
                <td>
                  <div className="business-cell">
                    <Initial b={b} />
                    <div>
                      <strong>{b.name}</strong>
                      <small>
                        {b.owner} · {b.address}
                      </small>
                    </div>
                  </div>
                </td>
                <td data-label="Şablon">
                  {d.templates.find((t) => t.id === b.template)?.name || "—"}
                </td>
                <td data-label="Abonelik">
                  {s ? (
                    <>
                      <span>{date(s.end)}</span>
                      <small
                        className={days(s.end) <= 14 ? "amber-text" : "muted"}
                      >
                        {days(s.end) < 0
                          ? "Süresi doldu"
                          : `${days(s.end)} gün kaldı`}
                      </small>
                    </>
                  ) : (
                    "Tanımlanmadı"
                  )}
                </td>
                <td data-label="Durum">
                  <Badge active={b.active && !b.deleted}>
                    {b.deleted ? "Silindi" : b.active ? "Aktif" : "Pasif"}
                  </Badge>
                </td>
                <td>
                  <Button onClick={() => onOpen(b.id)}>
                    Yönet <Icon name="chevron" size={15} />
                  </Button>
                </td>
              </tr>
            )
          })}
        </tbody>
      </table>
      {!list.length && <Empty text="İşletme bulunamadı" />}
    </div>
  )
}
