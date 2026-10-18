import React from "react"
export default class ErrorBoundary extends React.Component {
  state = { failed: false }
  static getDerivedStateFromError() {
    return { failed: true }
  }
  render() {
    return this.state.failed ? (
      <div style={{ padding: 40, fontFamily: "sans-serif" }} role="alert">
        <h1>Ekran yüklenemedi</h1>
        <p>
          Sayfayı yenileyerek tekrar deneyin. Sorun devam ederse destek ekibiyle iletişime geçin.
        </p>
        <button onClick={() => location.reload()}>Yeniden dene</button>
      </div>
    ) : (
      this.props.children
    )
  }
}
