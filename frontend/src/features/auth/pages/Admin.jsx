import React, { useState } from "react"
import { Icon, Button, EmailField, Back, ErrorMessage } from "../components"
import { adminLogin } from "../../../services/auth"
export default function Admin({ navigate, onSuccess }) {
  const [email, setEmail] = useState(""),
    [password, setPassword] = useState(""),
    [visible, setVisible] = useState(false),
    [busy, setBusy] = useState(false),
    [error, setError] = useState("")
  return (
    <section className="form-view">
      <Back onClick={() => navigate("choice")} />
      <span className="section-icon">
        <Icon name="shield" size={25} />
      </span>
      <h1>Yönetici girişi</h1>
      <p className="intro">
        Yönetim merkezine erişmek için
        <br /> hesap bilgilerinizle giriş yapın.
      </p>
      <form
        onSubmit={async (e) => {
          e.preventDefault()
          if (busy) return
          setBusy(true)
          setError("")
          try {
            const session = await adminLogin(email, password)
            onSuccess(session)
          } catch (err) {
            if (err.status === 400 || err.status === 401) {
              setPassword("")
              setVisible(false)
            }
            setError(err.message)
          } finally {
            setBusy(false)
          }
        }}
      >
        <fieldset disabled={busy}>
          <EmailField value={email} onChange={setEmail} />
          <label className="field">
            <span>Şifre</span>
            <span className="input-wrap">
              <Icon name="lock" />
              <input
                name="password"
                type={visible ? "text" : "password"}
                autoComplete="current-password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="Şifrenizi girin"
                required
              />
              <button
                type="button"
                className="eye-button"
                aria-label={visible ? "Şifreyi gizle" : "Şifreyi göster"}
                aria-pressed={visible}
                onClick={() => setVisible(!visible)}
              >
                <Icon name="eye" />
              </button>
            </span>
          </label>
        </fieldset>
        <ErrorMessage message={error} />
        <Button type="submit" busy={busy}>
          Yönetici olarak giriş yap <Icon name="arrow" size={18} />
        </Button>
      </form>
      <p className="switch-method">
        İşletme sahibi misiniz?{" "}
        <button disabled={busy} onClick={() => navigate("business")}>
          İşletme girişi
        </button>
      </p>
    </section>
  )
}
