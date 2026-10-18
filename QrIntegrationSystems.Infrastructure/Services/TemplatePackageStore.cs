using Microsoft.Extensions.Options;
using QrIntegrationSystems.Infrastructure.Configuration;
using SkiaSharp;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace QrIntegrationSystems.Infrastructure.Services;

public sealed record TemplateManifest(int SchemaVersion, string Id, string Name, string Version, string Entry, int MenuContractVersion);
public sealed class TemplatePackageStore
{
    public string Root { get; }
    public TemplatePackageStore(IOptions<StorageOptions> options) => Root = Path.GetFullPath(options.Value.TemplatesPath);
    public static bool ValidCode(string code) => Regex.IsMatch(code, "^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.CultureInvariant) && code.Length <= 80;
    public string DirectoryFor(string code)
    {
        if (ValidCode(code) == false) throw new ArgumentException("Geçersiz şablon kodu.");
        return Path.Combine(Root, code);
    }
    public bool IsAvailable(string code) => EntryUrl(code) != null;
    public TemplateManifest? Manifest(string code)
    {
        if (ValidCode(code) == false) return null;
        try
        {
            var path = Path.Combine(DirectoryFor(code), "template.json");
            return File.Exists(path) ? JsonSerializer.Deserialize<TemplateManifest>(File.ReadAllText(path), new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) : null;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException) { return null; }
    }
    public string? EntryUrl(string code)
    {
        var manifest = Manifest(code);
        if (manifest == null || manifest.SchemaVersion != 1 || manifest.MenuContractVersion != 1 || manifest.Id != code) return null;
        try
        {
            var entry = SafeRelative(manifest.Entry);
            var directory = DirectoryFor(code);
            if (entry.EndsWith(".html", StringComparison.OrdinalIgnoreCase) == false || File.Exists(Path.Combine(directory, entry)) == false) return null;
            var capability = File.ReadAllText(Path.Combine(directory, ".capability"));
            if (Guid.TryParseExact(capability, "N", out _) == false) return null;
            return $"/api/template/assets/{code}/{capability}/{entry}";
        }
        catch (Exception ex) when (ex is ArgumentException or IOException or UnauthorizedAccessException) { return null; }
    }
    public static string SafeRelative(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Contains('\\') || value.StartsWith('/') || value.Contains(':') || value.Contains('%') || value.Contains('?') || value.Contains('#') || value.Length > 240)
            throw new ArgumentException("Paket içinde geçersiz dosya yolu.");
        var parts = value.Split('/');
        if (parts.Any(p => p.Length == 0 || p == "." || p == ".." || p.StartsWith('.') || p.Any(c => char.IsControl(c))))
            throw new ArgumentException("Paket yolu klasör dışına çıkamaz veya gizli dosya içeremez.");
        return value;
    }

    public (string PhysicalPath, string ContentType) ResolveOptimizedAsset(string code, string asset, string originalPath, string originalMime)
    {
        var ext = Path.GetExtension(originalPath).ToLowerInvariant();
        if (ext != ".png" && ext != ".jpg" && ext != ".jpeg")
        {
            return (originalPath, originalMime);
        }

        try
        {
            var info = new FileInfo(originalPath);
            if (info.Exists == false || info.Length <= 24 * 1024)
            {
                return (originalPath, originalMime);
            }

            var cachePath = Path.Combine(Root, ".opt-cache", code, asset.Replace('/', Path.DirectorySeparatorChar) + ".webp");
            if (File.Exists(cachePath) == true)
            {
                return (cachePath, "image/webp");
            }

            using var codec = SKCodec.Create(originalPath);
            if (codec == null || codec.Info.Width <= 0 || codec.Info.Height <= 0)
            {
                return (originalPath, originalMime);
            }

            using var bitmap = new SKBitmap(codec.Info);
            if (codec.GetPixels(bitmap.Info, bitmap.GetPixels()) != SKCodecResult.Success)
            {
                return (originalPath, originalMime);
            }

            using var image = SKImage.FromBitmap(bitmap);
            using var encoded = image.Encode(SKEncodedImageFormat.Webp, 82);
            if (encoded == null || encoded.Size >= info.Length)
            {
                return (originalPath, originalMime);
            }

            var cacheDir = Path.GetDirectoryName(cachePath);
            if (string.IsNullOrWhiteSpace(cacheDir) == false)
            {
                Directory.CreateDirectory(cacheDir);
            }

            var tempPath = cachePath + "." + Guid.NewGuid().ToString("N") + ".tmp";
            File.WriteAllBytes(tempPath, encoded.ToArray());
            File.Move(tempPath, cachePath, true);

            return (cachePath, "image/webp");
        }
        catch
        {
            return (originalPath, originalMime);
        }
    }
}

