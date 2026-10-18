import React, { useState } from "react"
import { money } from "../repository"

export default function Chart({ series = [] }) {
  const [hovered, setHovered] = useState(null)

  const data = Array.isArray(series) == true && series.length > 0 ? series : []
  const maxVal = Math.max(...data.map((x) => Number(x.count) || 0), 0)
  // Ölçek tavanını belirle (minimum 1000 TL, yoksa maxVal'in bir üst yuvarlağı)
  const ceiling = maxVal > 0 ? Math.ceil((maxVal * 1.15) / 100) * 100 : 1000

  // SVG Ölçüleri
  const width = 640
  const height = 230
  const padLeft = 70
  const padRight = 35
  const padTop = 25
  const padBottom = 40

  const plotW = width - padLeft - padRight
  const plotH = height - padTop - padBottom

  const points = data.map((item, idx) => {
    const x =
      data.length > 1
        ? padLeft + (idx / (data.length - 1)) * plotW
        : padLeft + plotW / 2
    const val = Number(item.count) || 0
    const y = padTop + plotH - (val / ceiling) * plotH
    return { x, y, val, item, idx }
  })

  const linePath =
    points.length > 0
      ? points
          .map((p, i) => `${i === 0 ? "M" : "L"} ${p.x.toFixed(1)} ${p.y.toFixed(1)}`)
          .join(" ")
      : ""

  const areaPath =
    points.length > 0
      ? `${linePath} L ${points[points.length - 1].x.toFixed(1)} ${(padTop + plotH).toFixed(1)} L ${points[0].x.toFixed(1)} ${(padTop + plotH).toFixed(1)} Z`
      : ""

  return (
    <div style={{ position: "relative", width: "100%", padding: "12px 16px 20px" }}>
      <svg
        viewBox={`0 0 ${width} ${height}`}
        style={{ width: "100%", height: "auto", overflow: "visible", display: "block" }}
      >
        <defs>
          <linearGradient id="revenueGrad" x1="0" y1="0" x2="0" y2="1">
            <stop offset="0%" stopColor="#8b5cf6" stopOpacity="0.38" />
            <stop offset="100%" stopColor="#8b5cf6" stopOpacity="0.0" />
          </linearGradient>
        </defs>

        {/* Yatay Kılavuz Çizgileri & Y Ekseni Etiketleri */}
        {[1, 0.5, 0].map((ratio, i) => {
          const y = padTop + plotH * (1 - ratio)
          const labelVal = ceiling * ratio
          return (
            <g key={i}>
              <line
                x1={padLeft}
                y1={y}
                x2={padLeft + plotW}
                y2={y}
                stroke="#e2e8f0"
                strokeDasharray="4 4"
                strokeWidth="1"
              />
              <text
                x={padLeft - 10}
                y={y + 4}
                textAnchor="end"
                fontSize="11"
                fill="#94a3b8"
                fontWeight="500"
              >
                {money(labelVal)}
              </text>
            </g>
          )
        })}

        {/* Alan Dolgusu */}
        {areaPath !== "" && (
          <path d={areaPath} fill="url(#revenueGrad)" />
        )}

        {/* Çizgi */}
        {linePath !== "" && (
          <path
            d={linePath}
            fill="none"
            stroke="#8b5cf6"
            strokeWidth="3.2"
            strokeLinecap="round"
            strokeLinejoin="round"
          />
        )}

        {/* X Ekseni Etiketleri ve Veri Noktaları */}
        {points.map((p, i) => {
          const isHovered = hovered === i
          return (
            <g key={i}>
              {/* X Ekseni Ay Etiketi */}
              <text
                x={p.x}
                y={padTop + plotH + 24}
                textAnchor="middle"
                fontSize="11"
                fill={isHovered == true ? "#6d28d9" : "#64748b"}
                fontWeight={isHovered == true ? "600" : "500"}
              >
                {p.item.monthShort || p.item.date}
              </text>

              {/* Dikey Hover Kılavuz Çizgisi */}
              {isHovered == true && (
                <line
                  x1={p.x}
                  y1={padTop}
                  x2={p.x}
                  y2={padTop + plotH}
                  stroke="#c4b5fd"
                  strokeDasharray="3 3"
                  strokeWidth="1.5"
                />
              )}

              {/* Nokta Halkası */}
              <circle
                cx={p.x}
                cy={p.y}
                r={isHovered == true ? 6 : 4}
                fill={isHovered == true ? "#6d28d9" : "#8b5cf6"}
                stroke="#ffffff"
                strokeWidth="2.5"
                style={{ transition: "all 0.15s ease" }}
              />

              {/* Geniş Tıklama / Hover Alanı */}
              <circle
                cx={p.x}
                cy={p.y}
                r={22}
                fill="transparent"
                style={{ cursor: "pointer" }}
                onMouseEnter={() => setHovered(i)}
                onMouseLeave={() => setHovered(null)}
              />
            </g>
          )
        })}
      </svg>

      {/* Hover Tooltip (Kullanıcı fareyi noktanın üzerine getirdiğinde) */}
      {hovered != null && Boolean(points[hovered]) == true && (
        <div
          style={{
            position: "absolute",
            left: `${(points[hovered].x / width) * 100}%`,
            top: `${(points[hovered].y / height) * 100}%`,
            transform: "translate(-50%, -125%)",
            background: "#1e1b4b",
            color: "#ffffff",
            padding: "6px 12px",
            borderRadius: "6px",
            fontSize: "12px",
            fontWeight: "500",
            pointerEvents: "none",
            whiteSpace: "nowrap",
            boxShadow: "0 4px 12px rgba(0,0,0,0.18)",
            zIndex: 10,
          }}
        >
          <div style={{ fontSize: "10px", color: "#c4b5fd", marginBottom: "2px" }}>
            {points[hovered].item.date}
          </div>
          <div style={{ fontWeight: "700" }}>
            {money(points[hovered].val)} ciro
          </div>
        </div>
      )}
    </div>
  )
}
