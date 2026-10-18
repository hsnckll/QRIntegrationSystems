import React, { useState, useMemo, useEffect } from "react"
import { Icon, Field } from "../../../shared/components"
import { getBusinessStatistics } from "../repository"

export default function Statistics() {
  const [period, setPeriod] = useState("7")
  const [from, setFrom] = useState("")
  const [to, setTo] = useState("")
  const [stats, setStats] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)

  useEffect(() => {
    let active = true
    async function fetchStats() {
      try {
        setLoading(true)
        setError(null)
        const res = await getBusinessStatistics()
        if (active == true) {
          setStats(res)
        }
      } catch (err) {
        if (active == true) {
          setError(err.message || "İstatistik verileri alınamadı.")
        }
      } finally {
        if (active == true) {
          setLoading(false)
        }
      }
    }
    fetchStats()
    return () => {
      active = false
    }
  }, [])

  const events = useMemo(() => {
    if (Boolean(stats) == false || Array.isArray(stats.scans) == false) {
      return []
    }
    return stats.scans.map((iso) => new Date(iso))
  }, [stats])

  const now = new Date()
  const start = new Date(now)
  start.setHours(0, 0, 0, 0)
  if (period === "30") start.setDate(start.getDate() - 29)
  else if (period === "7") start.setDate(start.getDate() - 6)
  else if (period === "custom" && Boolean(from) == true)
    start.setTime(new Date(from + "T00:00:00").getTime())
  const end = period === "custom" && Boolean(to) == true ? new Date(to + "T23:59:59") : now
  const invalid =
    period === "custom" &&
    (Boolean(from) == false || Boolean(to) == false || start > end || (end - start) / 86400000 > 365)
  const selected = invalid == true ? [] : events.filter((t) => t >= start && t <= end)
  const buckets = []
  if (invalid == false) {
    if (period === "today") {
      for (let h = 0; h <= now.getHours(); h++)
        buckets.push({
          label: `${String(h).padStart(2, "0")}:00`,
          n: selected.filter((t) => t.getHours() === h).length,
        })
    } else {
      for (let d = new Date(start); d <= end; d.setDate(d.getDate() + 1)) {
        const same = selected.filter(
          (t) => t.toDateString() === d.toDateString(),
        )
        buckets.push({
          label: d.toLocaleDateString("tr-TR", {
            day: "numeric",
            month: "short",
          }),
          n: same.length,
        })
      }
    }
  }
  const max = Math.max(1, ...buckets.map((b) => b.n))
  const peak = buckets.reduce((a, b) => (b.n > a.n ? b : a), { n: 0, label: "—" })

  if (loading == true) {
    return (
      <div style={{ textAlign: "center", padding: "60px 20px" }}>
        <p className="muted">İstatistikler veritabanından alınıyor...</p>
      </div>
    )
  }

  if (Boolean(error) == true) {
    return (
      <div style={{ textAlign: "center", padding: "60px 20px" }}>
        <p style={{ color: "#ef4444" }}>{error}</p>
      </div>
    )
  }

  return (
    <>
      <div className="stats-toolbar">
        <div className="segmented">
          {[
            ["today", "Bugün"],
            ["7", "Son 7 gün"],
            ["30", "Son 30 gün"],
            ["custom", "Özel tarih"],
          ].map(([key, label]) => (
            <button
              className={period === key ? "active" : ""}
              key={key}
              onClick={() => setPeriod(key)}
            >
              {label}
            </button>
          ))}
        </div>
        <span className="muted small">
          Toplam {Number(stats?.totalScans || 0).toLocaleString("tr-TR")} okutma
        </span>
      </div>
      {period === "custom" && (
        <div className="custom-dates">
          <Field
            label="Başlangıç"
            type="date"
            value={from}
            onChange={(e) => setFrom(e.target.value)}
          />
          <Field
            label="Bitiş"
            type="date"
            value={to}
            onChange={(e) => setTo(e.target.value)}
          />
          {invalid == true && (
            <span role="status">
              En fazla 365 günlük geçerli bir tarih aralığı seçin.
            </span>
          )}
        </div>
      )}
      <div className="stat-cards">
        <div>
          <span>Toplam QR okutma</span>
          <strong>{selected.length.toLocaleString("tr-TR")}</strong>
          <small>Seçilen tarih aralığında</small>
        </div>
        <div>
          <span>
            {period === "today" ? "Saatlik ortalama" : "Günlük ortalama"}
          </span>
          <strong>{Math.round(selected.length / (buckets.length || 1))}</strong>
          <small>QR okutma</small>
        </div>
        <div>
          <span>{period === "today" ? "En yoğun saat" : "En yoğun gün"}</span>
          <strong className="peak">{Boolean(peak.n) == true ? peak.label : "—"}</strong>
          <small>{Boolean(peak.n) == true ? `${peak.n} okutma` : "Henüz veri yok"}</small>
        </div>
      </div>
      <section className="surface chart-panel">
        <div className="chart-heading">
          <div>
            <h2>QR okutma dağılımı</h2>
            <p>Menünüzün ziyaret edilme sıklığı</p>
          </div>
          <span className="chart-legend">
            <i />
            QR okutma
          </span>
        </div>
        {selected.length > 0 ? (
          <div className="chart">
            <div className="y-axis">
              {[
                max,
                Math.round(max * 0.75),
                Math.round(max * 0.5),
                Math.round(max * 0.25),
                0,
              ].map((n, i) => (
                <span key={i}>{n}</span>
              ))}
            </div>
            <div className="plot">
              <div className="gridlines">
                <i />
                <i />
                <i />
                <i />
                <i />
              </div>
              <div className="bars">
                {buckets.map((b, i) => (
                  <div className="bar-column" key={b.label}>
                    <button
                      style={{ height: `${Math.max(1, (b.n / max) * 100)}%` }}
                      aria-label={`${b.label}: ${b.n} okutma`}
                    >
                      <span className="chart-tooltip">
                        {b.label}
                        <strong>{b.n} okutma</strong>
                      </span>
                    </button>
                    <span className="x-label">
                      {buckets.length < 10 ||
                      i % Math.ceil(buckets.length / 7) === 0
                        ? b.label
                        : ""}
                    </span>
                  </div>
                ))}
              </div>
            </div>
          </div>
        ) : (
          <div className="empty">
            <Icon name="chart" size={36} />
            <h3>
              {invalid == true ? "Tarih aralığını seçin" : "Bu dönemde okutma yok"}
            </h3>
            <p>QR okutmalarınız olduğunda grafikte görünecek.</p>
          </div>
        )}
        <div className="chart-footnote">
          <Icon name="help" size={17} />
          <p>
            Toplam QR okutmalarını gösterir; tekil kişi sayısını göstermez. Aynı
            kişinin tekrar okutması yeni bir okutma olarak sayılır.
          </p>
        </div>
      </section>
    </>
  )
}
