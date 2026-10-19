import test from "node:test"
import assert from "node:assert/strict"
import { createApiClient, ApiError } from "../src/services/api/http.js"
import {
  businessUpdateDto,
  productUpdateDto,
  sessionFromApi,
} from "../src/services/api/mappers.js"
test("API JSON, bearer token and DTO body", async () => {
  let seen
  const request = createApiClient({
    baseUrl: "/api/",
    getToken: () => "test-token",
    fetchImpl: async (url, opts) => {
      seen = { url, opts }
      return new Response(JSON.stringify({ id: 7 }), { status: 200 })
    },
  })
  assert.deepEqual(
    await request("business/7", { method: "PUT", body: { name: "Yeni" } }),
    { id: 7 },
  )
  assert.equal(seen.url, "/api/business/7")
  assert.equal(seen.opts.headers.Authorization, "Bearer test-token")
  assert.equal(seen.opts.body, '{"name":"Yeni"}')
})
test("401 expires auth; validation fields survive errors", async () => {
  let expired = false
  const request = createApiClient({
    onUnauthorized: () => (expired = true),
    fetchImpl: async () =>
      new Response(
        JSON.stringify({ message: "Geçersiz", errors: { email: ["Hatalı"] } }),
        { status: 401 },
      ),
  })
  await assert.rejects(
    request("business"),
    (e) =>
      e instanceof ApiError &&
      e.status === 401 &&
      e.details.email[0] === "Hatalı",
  )
  assert.equal(expired, true)
})
test("FormData is sent without forced content type", async () => {
  let opts
  const form = new FormData()
  form.append("Folder", "products")
  const request = createApiClient({
    fetchImpl: async (_, o) => {
      opts = o
      return new Response(null, { status: 204 })
    },
  })
  assert.equal(
    await request("upload/image", { method: "POST", body: form }),
    null,
  )
  assert.equal(opts.body, form)
  assert.equal(opts.headers["Content-Type"], undefined)
})
test("aborted and network failures never fall back to mock success", async () => {
  const request = createApiClient({
    fetchImpl: async () => {
      throw Error("offline")
    },
  })
  await assert.rejects(request("business"), (e) => e.status === 0)
  const controller = new AbortController()
  controller.abort()
  const aborted = createApiClient({
    fetchImpl: async () => {
      throw Error("aborted")
    },
  })
  await assert.rejects(
    aborted("business", { signal: controller.signal }),
    (e) => e.message.includes("iptal"),
  )
})
test("business DTO includes a normalized email and excludes immutable slug", () => {
  const dto = businessUpdateDto({
    name: "Doy",
    owner: "E",
    email: "  OWNER@EXAMPLE.COM  ",
    slug: "s",
    template: 2,
    phone: "5",
    address: "M",
  })
  assert.equal(dto.email, "owner@example.com")
  assert.equal(dto.slug, undefined)
  assert.equal(dto.templateId, 2)
  assert.deepEqual(
    productUpdateDto(
      {
        category: "3",
        name: "Dürüm",
        description: "",
        price: "150",
        active: true,
      },
      1,
    ),
    {
      categoryId: 3,
      name: "Dürüm",
      description: "",
      price: 150,
      sortOrder: 1,
      isActive: true,
    },
  )
  assert.throws(() => sessionFromApi({ role: "Other" }))
})
