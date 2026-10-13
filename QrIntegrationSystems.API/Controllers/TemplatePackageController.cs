using System.IO.Compression;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QrIntegrationSystems.Domain.Entities;
using QrIntegrationSystems.Infrastructure.Data;
using QrIntegrationSystems.Infrastructure.Services;

namespace QrIntegrationSystems.API.Controllers;

[ApiController, Route("api/template")]
public sealed class TemplatePackageController(AppDbContext db, TemplatePackageStore store) : ControllerBase
{
    const long MaxBytes = 32 * 1024 * 1024;
    static readonly HashSet<string> Extensions = new(StringComparer.OrdinalIgnoreCase) { ".html", ".js", ".css", ".json", ".png", ".jpg", ".jpeg", ".webp", ".svg", ".ico", ".woff", ".woff2" };
    static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    [HttpPost("package"), Authorize(Roles = "SuperAdmin"), RequestSizeLimit(36 * 1024 * 1024)]
    public async Task<IActionResult> Upload([FromForm] PackageUpload form, CancellationToken ct)
    {
        Directory.CreateDirectory(store.Root);
        var staging = Path.Combine(store.Root, ".staging-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(staging);
        string? installed = null;
        long total = 0;
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        async Task Save(string name, Stream input)
        {
            name = TemplatePackageStore.SafeRelative(name);
            if (!Extensions.Contains(Path.GetExtension(name)) || !seen.Add(name) || seen.Count > 1000)
                throw new ArgumentException("Paket yalnız web dosyaları içerebilir; yinelenen veya fazla dosya var.");
            var path = Path.Combine(staging, name.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await using var output = System.IO.File.Create(path);
            var buffer = new byte[81920];
            int count;
            while ((count = await input.ReadAsync(buffer, ct)) > 0)
            {
                total += count;
                if (total > MaxBytes) throw new ArgumentException("Açılmış paket 32 MB sınırını aşıyor.");
                await output.WriteAsync(buffer.AsMemory(0, count), ct);
            }
        }
        try
        {
            if (form.Archive != null)
            {
                if (form.Files.Count != 0 || !form.Archive.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) || form.Archive.Length > MaxBytes)
                    throw new ArgumentException("Tek ZIP veya klasör dosyaları seçin.");
                using var zip = new ZipArchive(form.Archive.OpenReadStream(), ZipArchiveMode.Read);
                if (zip.Entries.Count > 1200) throw new ArgumentException("Paket çok fazla dosya içeriyor.");
                foreach (var entry in zip.Entries)
                {
                    if ((entry.ExternalAttributes >> 16 & 0xF000) == 0xA000) throw new ArgumentException("Sembolik bağlantılar kabul edilmez.");
                    if (entry.FullName.EndsWith('/')) { TemplatePackageStore.SafeRelative(entry.FullName.TrimEnd('/')); continue; }
                    using var input = entry.Open();
                    await Save(entry.FullName, input);
                }
            }
            else
            {
                var paths = JsonSerializer.Deserialize<string[]>(form.Paths ?? "[]") ?? [];
                if (paths.Length != form.Files.Count || paths.Length == 0) throw new ArgumentException("Klasör dosyaları ve yolları eksik.");
                for (int i = 0; i < paths.Length; i++) { using var input = form.Files[i].OpenReadStream(); await Save(paths[i], input); }
            }
            var manifestPath = Path.Combine(staging, "template.json");
            if (!System.IO.File.Exists(manifestPath) || new FileInfo(manifestPath).Length > 8192) throw new ArgumentException("Paket kökünde geçerli template.json bulunmalı.");
            var manifest = JsonSerializer.Deserialize<TemplateManifest>(await System.IO.File.ReadAllTextAsync(manifestPath, ct), JsonOptions);
            if (manifest == null || manifest.SchemaVersion != 1 || manifest.MenuContractVersion != 1 || !TemplatePackageStore.ValidCode(manifest.Id) || string.IsNullOrWhiteSpace(manifest.Name) || manifest.Name.Length > 120 || string.IsNullOrWhiteSpace(manifest.Version) || manifest.Version.Length > 40)
                throw new ArgumentException("Şablon manifesti v1 sözleşmesine uygun değil.");
            var entryName = TemplatePackageStore.SafeRelative(manifest.Entry);
            if (!entryName.EndsWith(".html", StringComparison.OrdinalIgnoreCase) || !seen.Contains(entryName)) throw new ArgumentException("Manifest giriş HTML dosyası bulunamadı.");
            // Ready/menu integration is exercised in the preview before publishing; no server-side JavaScript execution.
            var destination = store.DirectoryFor(manifest.Id);
            if (Directory.Exists(destination) || await db.Templates.AnyAsync(t => t.FolderName == manifest.Id, ct)) throw new ArgumentException("Bu şablon kodu zaten kullanılıyor. Yeni kod ile yükleyin.");
            await System.IO.File.WriteAllTextAsync(Path.Combine(staging, ".capability"), Guid.NewGuid().ToString("N"), ct);
            Directory.Move(staging, destination);
            installed = destination;
            var template = new Template { Name = manifest.Name.Trim(), FolderName = manifest.Id, CreatedAt = DateTime.UtcNow };
            db.Templates.Add(template);
            await db.SaveChangesAsync(ct);
            installed = null;
            return Ok(new { template.Id, template.Name, template.FolderName, entryUrl = store.EntryUrl(template.FolderName), isPublished = false, version = manifest.Version });
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidDataException or JsonException or DbUpdateException)
        { return BadRequest(new { message = ex is DbUpdateException ? "Şablon kaydı oluşturulamadı." : ex.Message }); }
        finally
        {
            if (Directory.Exists(staging)) Directory.Delete(staging, true);
            if (installed != null && Directory.Exists(installed)) Directory.Delete(installed, true);
        }
    }

    [HttpGet("runtime/{code}"), AllowAnonymous]
    public async Task<IActionResult> Runtime(string code)
    {
        if (!TemplatePackageStore.ValidCode(code)) return NotFound();
        var template = await db.Templates.AsNoTracking().SingleOrDefaultAsync(t => t.FolderName == code);
        if (template == null || (!store.Published(code) && !User.IsInRole("SuperAdmin"))) return NotFound(new { message = "Şablon yayında değil." });
        return Ok(new { template.Id, template.Name, entryUrl = store.EntryUrl(code), isPublished = store.Published(code) });
    }

    [HttpPost("{id:int}/publish"), Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Publish(int id)
    {
        var template = await db.Templates.FindAsync(id);
        if (template == null || store.Manifest(template.FolderName) == null) return NotFound();
        await System.IO.File.WriteAllTextAsync(Path.Combine(store.DirectoryFor(template.FolderName), ".published"), DateTime.UtcNow.ToString("O"));
        return Ok(new { message = "Şablon kullanıma açıldı." });
    }

    // A random capability permits draft preview; no tenant data or session is stored in packages.
    [HttpGet("assets/{code}/{capability}/{**asset}"), AllowAnonymous]
    public IActionResult Asset(string code, string capability, string asset)
    {
        try
        {
            var directory = store.DirectoryFor(code);
            var key = Path.Combine(directory, ".capability");
            if (!System.IO.File.Exists(key) || System.IO.File.ReadAllText(key) != capability) return NotFound();
            asset = TemplatePackageStore.SafeRelative(asset);
            var path = Path.Combine(directory, asset.Replace('/', Path.DirectorySeparatorChar));
            if (!System.IO.File.Exists(path)) return NotFound();
            Response.Headers["Content-Security-Policy"] = "sandbox allow-scripts; default-src 'none'; script-src 'unsafe-inline' https: http:; style-src 'unsafe-inline' https: http:; img-src https: http: data:; font-src https: http: data:; connect-src 'none'; frame-src 'none'; form-action 'none'; base-uri 'none'; object-src 'none'";
            Response.Headers["Access-Control-Allow-Origin"] = "*"; // Module scripts load from an opaque sandbox origin.
            Response.Headers["X-Content-Type-Options"] = "nosniff";
            Response.Headers["Referrer-Policy"] = "no-referrer";
            Response.Headers["Cache-Control"] = "no-cache";
            var ext = Path.GetExtension(path).ToLowerInvariant();
            var mime = ext switch { ".html" => "text/html", ".js" => "text/javascript", ".css" => "text/css", ".json" => "application/json", ".svg" => "image/svg+xml", ".png" => "image/png", ".webp" => "image/webp", ".jpg" or ".jpeg" => "image/jpeg", ".woff" => "font/woff", ".woff2" => "font/woff2", ".ico" => "image/x-icon", _ => "application/octet-stream" };
            return PhysicalFile(path, mime);
        }
        catch (ArgumentException) { return NotFound(); }
    }
}
public sealed class PackageUpload
{
    public IFormFile? Archive { get; set; }
    public List<IFormFile> Files { get; set; } = [];
    public string? Paths { get; set; }
}
