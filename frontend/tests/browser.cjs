const {
  chromium,
} = require("C:/Users/ebk90/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/playwright")
const assert = require("node:assert/strict")
const path = require("node:path")
const fs = require("node:fs")
;(async () => {
  const browser = await chromium.launch({
    executablePath: "C:/Program Files/Google/Chrome/Application/chrome.exe",
    headless: true,
  })
  try {
    const context = await browser.newContext({
      viewport: { width: 1440, height: 1000 },
      reducedMotion: "reduce",
    })
    const p = await context.newPage()
    const errors = []
    p.on("pageerror", (e) => errors.push(e.message))
    const base = process.env.PREVIEW_URL || "http://127.0.0.1:5177"
    const output = path.resolve("outputs")
    fs.mkdirSync(output, { recursive: true })
    await p.goto(base + "/superadmin.html")
    await p.waitForURL("**/index.html")
    async function admin() {
      await p.goto(base + "/index.html#admin")
      await p.getByRole("button", { name: "Demo bilgilerini doldur" }).click()
      await p.getByRole("button", { name: "Yönetici olarak giriş yap" }).click()
      await p.waitForURL("**/superadmin.html")
      await p
        .getByRole("heading", { name: "Her işletme, tek bir merkezde." })
        .waitFor()
      assert.equal(await p.getByText("Hazırsınız.", { exact: true }).count(), 0)
    }
    await admin()
    await p.screenshot({ path: path.join(output, "superadmin.png") })
    await p.goto(base + "/customer.html")
    await p.waitForURL("**/superadmin.html")
    await p.locator("nav").getByRole("button", { name: "İşletmeler" }).click()
    await p.getByRole("button", { name: "Yönet", exact: true }).first().click()
    await p.getByRole("button", { name: "Menü düzenleme", exact: true }).click()
    await p.getByRole("button", { name: "Ürün ekle", exact: true }).click()
    await p.getByLabel("Ürün adı", { exact: true }).fill("Ortak veri testi")
    await p.getByLabel("Fiyat (TL)").fill("175")
    await p.getByRole("button", { name: "Ürünü kaydet" }).click()
    await p.getByText("Ortak veri testi", { exact: true }).waitFor()
    await p.getByRole("button", { name: "Çıkış yap", exact: true }).click()
    await p.waitForURL("**/index.html")
    await p.goto(base + "/index.html#business")
    await p.getByLabel("E-posta adresi").fill("demo@isletme.com")
    await p.getByRole("button", { name: "Doğrulama kodu al" }).click()
    await p.getByLabel("Doğrulama kodu", { exact: true }).fill("123456")
    await p.getByRole("button", { name: "Doğrula ve devam et" }).click()
    await p.waitForURL("**/customer.html")
    await p.getByText("Ortak veri testi", { exact: true }).waitFor()
    await p.getByTitle("Ortak veri testi düzenle", { exact: true }).click()
    await p.getByLabel("Fiyat (TL)").fill("190")
    await p.getByRole("button", { name: "Ürünü kaydet" }).click()
    await p.reload()
    await p.getByText("Ortak veri testi", { exact: true }).waitFor()
    await p.screenshot({ path: path.join(output, "customer.png") })
    for (const w of [320, 390, 430]) {
      await p.setViewportSize({ width: w, height: 844 })
      assert.equal(
        await p.evaluate(
          () => document.documentElement.scrollWidth > innerWidth,
        ),
        false,
        `customer ${w}`,
      )
    }
    await p.setViewportSize({ width: 1440, height: 1000 })
    await p.getByRole("button", { name: "Çıkış yap", exact: true }).click()
    await p.waitForURL("**/index.html")
    await p.screenshot({ path: path.join(output, "login.png") })
    await p.goto(base + "/customer.html")
    await p.waitForURL("**/index.html")
    await admin()
    await p.locator("nav").getByRole("button", { name: "İşletmeler" }).click()
    await p.getByRole("button", { name: "Yönet", exact: true }).first().click()
    await p.getByRole("button", { name: "Menü düzenleme", exact: true }).click()
    const row = p
      .locator(".managed-product")
      .filter({ hasText: "Ortak veri testi" })
    await row.waitFor()
    assert.ok((await row.innerText()).includes("190"))
    await p.getByRole("button", { name: "Geçmiş", exact: true }).click()
    await p
      .getByText("İşletme paneli güncellendi", { exact: true })
      .first()
      .waitFor()
    await p
      .getByRole("button", { name: "QR kodları", exact: true })
      .last()
      .click()
    await p.getByRole("button", { name: "QR kodu oluştur" }).click()
    await p.getByRole("button", { name: "Oluştur ve işletmeye ata" }).click()
    await p.locator(".qr-url").waitFor()
    const url = await p.locator(".qr-url").getAttribute("href")
    assert.ok(url.includes("/menu.html?qr="))
    const menu = await context.newPage()
    await menu.goto(url)
    await menu.getByRole("heading", { name: "Doy Doy Hatay Döner" }).waitFor()
    await menu.getByText("Ortak veri testi", { exact: true }).waitFor()
    await menu.close()
    await p.goto(base + "/doydoy.html")
    await p
      .getByRole("heading", { name: /Doy Doy/ })
      .first()
      .waitFor()
    await p.screenshot({ path: path.join(output, "doydoy.png") })
    assert.deepEqual(errors, [])
    console.log(
      "PASS: direct redirects; logout; role guards; persistence; shared admin/customer/menu data; audit logs; QR unified route; original DoyDoy; responsive widths; no page errors.",
    )
  } finally {
    await browser.close()
  }
})().catch((e) => {
  console.error(e)
  process.exit(1)
})
