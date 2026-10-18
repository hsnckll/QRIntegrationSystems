export async function photoData(file) {
  if (!["image/jpeg", "image/png", "image/webp"].includes(file.type))
    throw new Error("JPEG, PNG veya WebP fotoğraf seçin.")
  if (file.size > 8 * 1024 * 1024)
    throw new Error("En fazla 8 MB büyüklüğünde fotoğraf seçin.")
  const bitmap = await createImageBitmap(file)
  const ratio = Math.min(1, 800 / Math.max(bitmap.width, bitmap.height))
  const canvas = document.createElement("canvas")
  canvas.width = Math.round(bitmap.width * ratio)
  canvas.height = Math.round(bitmap.height * ratio)
  canvas.getContext("2d").drawImage(bitmap, 0, 0, canvas.width, canvas.height)
  bitmap.close()
  return canvas.toDataURL("image/webp", 0.78)
}
