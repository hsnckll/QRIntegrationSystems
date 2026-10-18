import React from "react"
import { Icon } from "../../../shared/components"

export default function TemplateCardPreview({ template: t, onPreview }) {
  const customThumbnail = t.thumbnail || t.thumbnailPath

  return (
    <div
      className="template-real-preview"
      onClick={() => {
        if (Boolean(onPreview) == true) {
          onPreview(t)
        }
      }}
      title="Canlı önizleme için tıklayın"
      style={{ position: "relative", overflow: "hidden", background: "#f8fafc" }}
    >
      <div className="template-preview-overlay">
        <span className="template-preview-overlay-btn">
          <Icon name="eye" size={14} />
          Canlı Önizle
        </span>
      </div>

      {Boolean(customThumbnail) == true ? (
        <div
          style={{
            position: "relative",
            width: "100%",
            height: "100%",
            overflow: "hidden",
            background: "#171410",
          }}
        >
          <img
            src={customThumbnail}
            alt={t.name}
            loading="lazy"
            decoding="async"
            style={{
              width: "100%",
              height: "100%",
              objectFit: "cover",
              display: "block",
            }}
          />
        </div>
      ) : (
        <div
          style={{
            width: "100%",
            height: "100%",
            position: "relative",
            overflow: "hidden",
            background: "#fff",
          }}
        >
          <iframe
            title={`${t.name} Thumbnail`}
            src={`/menu.html?previewTemplate=${encodeURIComponent(t.folder || t.id || t.name)}`}
            style={{
              width: "350%",
              height: "350%",
              transform: "scale(0.2857)",
              transformOrigin: "top left",
              border: "none",
              pointerEvents: "none",
              background: "#fff",
            }}
            tabIndex={-1}
            loading="lazy"
          />
        </div>
      )}
    </div>
  )
}
