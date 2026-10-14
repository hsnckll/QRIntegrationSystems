using Microsoft.Extensions.Options;
using QrIntegrationSystems.Infrastructure.Configuration;
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
        if (!ValidCode(code)) throw new ArgumentException("Geçersiz şablon kodu.");
        return Path.Combine(Root, code);
    }
    public bool Published(string code) => ValidCode(code) && File.Exists(Path.Combine(DirectoryFor(code), ".published"));
    public TemplateManifest? Manifest(string code)
    {
        var path = Path.Combine(DirectoryFor(code), "template.json");
        return File.Exists(path) ? JsonSerializer.Deserialize<TemplateManifest>(File.ReadAllText(path), new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) : null;
    }
    public string? EntryUrl(string code)
    {
        var manifest = Manifest(code);
        if (manifest == null) return null;
        var capability = File.ReadAllText(Path.Combine(DirectoryFor(code), ".capability"));
        return $"/api/template/assets/{code}/{capability}/{manifest.Entry}";
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
}
