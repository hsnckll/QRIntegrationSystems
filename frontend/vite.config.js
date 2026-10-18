import { defineConfig, loadEnv } from "vite"
import tailwindcss from "@tailwindcss/vite"
import { resolve } from "node:path"
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), "")
  return {
    plugins: [tailwindcss()],
    esbuild: { jsx: "automatic" },
    server: {
      fs: { deny: [".env", ".env.*", "*.{crt,pem}", "**/.git/**", "**/archive/**"] },
      watch: { ignored: ["**/archive/**", "**/outputs/**"] },
      proxy: {
        "/api": {
          target: env.API_PROXY_TARGET || "https://localhost:7168",
          changeOrigin: true,
          secure: false,
        },
        "/uploads": {
          target: env.API_PROXY_TARGET || "https://localhost:7168",
          changeOrigin: true,
          secure: false,
        },
      },
    },
    build: {
      rollupOptions: {
        input: Object.fromEntries(
          ["index", "customer", "superadmin", "menu"].map((name) => [
            name,
            resolve(process.cwd(), name + ".html"),
          ]),
        ),
      },
    },
  }
})
