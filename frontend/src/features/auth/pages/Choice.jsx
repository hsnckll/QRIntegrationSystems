import React from "react"
import { Icon } from "../components"
export default function Choice({ navigate }) {
  return (
    <section className="form-view choice">
      <span className="section-icon">
        <Icon name="store" size={25} />
      </span>
      <h1>Hoş geldiniz.</h1>
      <p className="intro">
        Menünüzü yönetmeye başlamak için
        <br className="desktop-break" /> giriş türünüzü seçin.
      </p>
      <div className="choices">
        <button className="choice-card" onClick={() => navigate("business")}>
          <span className="choice-icon">
            <Icon name="store" size={25} />
          </span>
          <span>
            <strong>İşletme girişi</strong>
            <small>
              Menünüzü, ürünlerinizi ve işletme
              <br className="desktop-break" /> bilgilerinizi yönetin.
            </small>
            <em>E-posta ile doğrulama</em>
          </span>
          <Icon name="arrow" />
        </button>
        <button
          className="choice-card admin-choice"
          onClick={() => navigate("admin")}
        >
          <span className="choice-icon">
            <Icon name="shield" size={25} />
          </span>
          <span>
            <strong>Yönetici girişi</strong>
            <small>
              İşletmeleri, abonelikleri ve
              <br className="desktop-break" /> platformu yönetin.
            </small>
            <em>E-posta ve şifre</em>
          </span>
          <Icon name="arrow" />
        </button>
      </div>
      <div className="below-note">
        <Icon name="lock" size={15} />
        <span>Size ait giriş yöntemiyle devam edin.</span>
      </div>
    </section>
  )
}
