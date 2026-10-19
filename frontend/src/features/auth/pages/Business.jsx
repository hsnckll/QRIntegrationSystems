import React, { useState, useEffect, useRef } from "react"
import { Icon, Button, EmailField, Back, ErrorMessage } from "../components"
import { sendOtp, verifyOtp } from "../../../services/auth"
export default function Business({ navigate, onSuccess }) {
  const [email, setEmail] = useState(""),
    [sentEmail, setSentEmail] = useState(""),
    [step, setStep] = useState("email"),
    [code, setCode] = useState(""),
    [busy, setBusy] = useState(false),
    [error, setError] = useState(""),
    [info, setInfo] = useState(() => new URLSearchParams(location.search).get("emailUpdated") === "1" ? "E-posta adresiniz güncellendi. Yeni e-posta adresinizle yeniden giriş yapın." : ""),
    [challenge, setChallenge] = useState(null),
    [now, setNow] = useState(Date.now())
  const codeRef = useRef(null)
  useEffect(() => {
    const id = setInterval(() => setNow(Date.now()), 1000)
    return () => clearInterval(id)
  }, [])
  useEffect(() => {
    if (step === "otp") codeRef.current?.focus()
  }, [step])

  const lockoutStr = localStorage.getItem("ip_lock_until")
  const lockoutUntil = parseInt(lockoutStr || "0", 10)
  const isLocked = lockoutUntil > now
  const lockRemainingSecs = Math.max(0, Math.ceil((lockoutUntil - now) / 1000))
  const lockText = `${String(Math.floor(lockRemainingSecs / 60)).padStart(2, "0")}:${String(lockRemainingSecs % 60).padStart(2, "0")}`

  useEffect(() => {
    if (isLocked == true && step !== "email") {
      setStep("email")
    }
  }, [isLocked, step])

  const remaining = Math.max(
      0,
      Math.ceil(((challenge?.resendAt || 0) - now) / 1000),
    ),
    expires = Math.max(0, Math.ceil(((challenge?.expiresAt || 0) - now) / 1000))
  async function requestCode(resend = false) {
    if (busy == true || isLocked == true) return
    setBusy(true)
    setError("")
    setInfo("")
    try {
      const address = resend ? sentEmail : email.trim().toLowerCase()
      const result = await sendOtp(address)
      setSentEmail(address)
      setChallenge(result)
      setNow(Date.now())
      setCode("")
      setStep("otp")
      setInfo(result.message || "Doğrulama kodu işletme e-posta adresinize gönderildi.")
    } catch (err) {
      if (err.status === 429 || err.details?.code === "IP_LOCKED") {
        const lockoutSeconds = err.details?.retryAfterSeconds || 300
        localStorage.setItem("ip_lock_until", (Date.now() + lockoutSeconds * 1000).toString())
        setNow(Date.now())
      } else {
        setError(err.message)
      }
    } finally {
      setBusy(false)
    }
  }
  return (
    <section className="form-view">
      <Back
        onClick={() =>
          step === "otp"
            ? (setStep("email"), setError(""), setInfo(""))
            : navigate("choice")
        }
      >
        {step === "otp" ? "E-posta adresini değiştir" : "Giriş seçenekleri"}
      </Back>
      <div
        className="steps"
        aria-label={step === "email" ? "Adım 1: E-posta" : "Adım 2: Doğrulama"}
      >
        <span className="active">
          <i>{step === "otp" ? <Icon name="check" size={12} /> : 1}</i>E-posta
        </span>
        <b />
        <span className={step === "otp" ? "active" : ""}>
          <i>2</i>Doğrulama
        </span>
      </div>
      <span className="section-icon">
        <Icon name={step === "email" ? "store" : "mail"} size={25} />
      </span>
      <h1>{step === "email" ? "İşletme girişi" : "E-postanızı doğrulayın"}</h1>
      <p className="intro">
        {step === "email" ? (
          "Kayıtlı e-posta adresinizle devam edin. Şifreye ihtiyacınız yok."
        ) : (
          <>
            Altı haneli doğrulama kodunu girin.
            <br />
            <strong className="email-destination">{sentEmail}</strong>
          </>
        )}
      </p>
      {step === "email" ? (
        <form
          onSubmit={(e) => {
            e.preventDefault()
            if (isLocked == false) requestCode()
          }}
        >
          <EmailField value={email} onChange={setEmail} disabled={busy == true || isLocked == true} />
          {info && <p className="status" role="status">{info}</p>}
          {isLocked == true ? (
            <div
              style={{
                marginTop: "12px",
                marginBottom: "20px",
                padding: "16px",
                borderRadius: "10px",
                backgroundColor: "#fff8f8",
                border: "1px solid #fecdd3",
                display: "flex",
                flexDirection: "column",
                gap: "10px",
              }}
            >
              <div style={{ display: "flex", alignItems: "center", gap: "8px", color: "#e11d48", fontWeight: "600", fontSize: "13px" }}>
                <Icon name="shield" size={18} />
                <span>Çok fazla başarısız deneme yapıldı</span>
              </div>
              <p style={{ margin: 0, fontSize: "12px", color: "#881337", lineHeight: "1.5" }}>
                Güvenliğiniz için bu cihazdan giriş yapılması geçici olarak durduruldu. Lütfen bekleyin:
              </p>
              <div
                style={{
                  display: "inline-flex",
                  alignItems: "center",
                  alignSelf: "flex-start",
                  gap: "6px",
                  backgroundColor: "#ffffff",
                  border: "1px solid #f43f5e",
                  padding: "5px 12px",
                  borderRadius: "6px",
                  fontSize: "13px",
                  fontWeight: "700",
                  color: "#e11d48",
                  letterSpacing: "0.5px",
                }}
              >
                <span style={{ fontSize: "11px", fontWeight: "500", color: "#9f1239" }}>Kalan süre:</span>
                <span>{lockText}</span>
              </div>
            </div>
          ) : (
            <>
              <ErrorMessage message={error} />
              <Button type="submit" busy={busy == true} disabled={busy == true}>
                Doğrulama kodu al <Icon name="arrow" size={18} />
              </Button>
              <p className="field-hint">
                Giriş kodu, kayıtlı işletme e-postanıza gönderilir.
              </p>
            </>
          )}
        </form>
      ) : (
        <form
          onSubmit={async (e) => {
            e.preventDefault()
            if (busy) return
            setBusy(true)
            setError("")
            setInfo("")
            try {
              const session = await verifyOtp(sentEmail, code)
              onSuccess(session)
            } catch (err) {
              if (err.status === 429 || err.details?.code === "IP_LOCKED") {
                const lockoutSeconds = err.details?.retryAfterSeconds || 300
                localStorage.setItem("ip_lock_until", (Date.now() + lockoutSeconds * 1000).toString())
                setNow(Date.now())
                setStep("email")
              } else {
                setError(err.message)
              }
            } finally {
              setBusy(false)
            }
          }}
        >
          <label className="field">
            <span>Doğrulama kodu</span>
            <input
              ref={codeRef}
              className="otp-input"
              name="code"
              aria-describedby="otp-help"
              type="text"
              inputMode="numeric"
              autoComplete="one-time-code"
              pattern="[0-9]{6}"
              minLength={6}
              maxLength={6}
              required
              value={code}
              disabled={busy}
              onChange={(e) =>
                setCode(e.target.value.replace(/\D/g, "").slice(0, 6))
              }
              placeholder="000000"
            />
          </label>
          <div className="code-timing" id="otp-help">
            <span>
              {expires
                ? `Kodun geçerliliği: ${Math.floor(expires / 60)}:${String(expires % 60).padStart(2, "0")}`
                : "Kodun süresi doldu"}
            </span>
            <span>6 haneli kod</span>
          </div>
          <ErrorMessage message={error} />
          {info && (
            <p className="status" role="status">
              {info}
            </p>
          )}
          <Button
            type="submit"
            busy={busy == true}
            disabled={code.length !== 6 || expires === 0 || busy == true}
          >
            Doğrula ve devam et <Icon name="arrow" size={18} />
          </Button>
          <div className="resend">
            <span>Kod gelmedi mi?</span>
            <button
              type="button"
              disabled={remaining > 0 || busy}
              onClick={() => requestCode(true)}
            >
              {remaining > 0
                ? `${remaining} sn sonra yeniden gönder`
                : "Kodu yeniden gönder"}
            </button>
          </div>
        </form>
      )}
      <div className="login-help" style={{ background: "#f8fafc", borderColor: "#e2e8f0" }}>
        <strong>
          <Icon name="help" size={15} /> Bilgilendirme
        </strong>
        <p>
          6 haneli tek kullanımlık giriş kodunuz işletmenizin kayıtlı e-posta adresine gönderilir.
          Lütfen gelen kutunuzu (ve gerekiyorsa Spam klasörünüzü) kontrol edin.
        </p>
      </div>
      {step === "email" && (
        <p className="switch-method">
          Platform yöneticisi misiniz?{" "}
          <button disabled={busy} onClick={() => navigate("admin")}>
            Yönetici girişi
          </button>
        </p>
      )}
    </section>
  )
}
