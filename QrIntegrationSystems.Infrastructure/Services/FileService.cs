using Microsoft.Extensions.Options;
using QrIntegrationSystems.Infrastructure.Configuration;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using QrIntegrationSystems.Application.Interfaces;
using QrIntegrationSystems.Infrastructure.Data;
using SkiaSharp;

namespace QrIntegrationSystems.Infrastructure.Services;

public sealed class FileService(AppDbContext db, IOptions<StorageOptions> options, AuthSessionStore sessions) : IFileService
{
    private readonly string root = Path.GetFullPath(options.Value.UploadsPath);
    public async Task<string> UploadImageAsync(Stream stream, string name, string folder, int? businessId, bool isSuperAdmin = false)
    {
        if (!isSuperAdmin && businessId is not > 0) throw new UnauthorizedAccessException();
        if (businessId.HasValue && !await db.Businesses.AnyAsync(b => b.Id == businessId && !b.IsDeleted)) throw new ValidationException("İşletme bulunamadı.");
        if (!new[] { "general", "logos", "banners", "products" }.Contains(folder)) throw new ValidationException("Geçersiz görsel klasörü.");
        if (!new[] { ".jpg", ".jpeg", ".png", ".webp" }.Contains(Path.GetExtension(name).ToLowerInvariant())) throw new ValidationException("JPEG, PNG veya WebP görsel seçin.");
        if (stream.Length == 0 || stream.Length > options.Value.MaxImageBytes) throw new ValidationException("Görsel 0–8 MB aralığında olmalı.");
        using var buffer = new MemoryStream(); await stream.CopyToAsync(buffer); buffer.Position = 0;
        using var codec = SKCodec.Create(buffer);
        if (codec == null || !new[] { SKEncodedImageFormat.Jpeg, SKEncodedImageFormat.Png, SKEncodedImageFormat.Webp }.Contains(codec.EncodedFormat)
            || codec.Info.Width <= 0 || codec.Info.Height <= 0 || codec.Info.Width > 8192 || codec.Info.Height > 8192 || (long)codec.Info.Width * codec.Info.Height > options.Value.MaxImagePixels)
            throw new ValidationException("Geçerli ve en fazla 16 megapiksel bir görsel seçin.");
        using var bitmap = new SKBitmap(codec.Info);
        if (codec.GetPixels(bitmap.Info, bitmap.GetPixels()) != SKCodecResult.Success) throw new ValidationException("Görsel okunamadı.");
        using var image = SKImage.FromBitmap(bitmap); using var encoded = image.Encode(SKEncodedImageFormat.Webp, 85);
        var owner = businessId.HasValue ? $"business-{businessId.Value}" : "admin";
        using var held = await sessions.AcquireAsync("upload:" + owner);
        var ownerDirs = new[] { "general", "logos", "banners", "products" }.Select(f => Path.Combine(root, f, owner));
        var currentBytes = ownerDirs.Where(Directory.Exists).Sum(d => Directory.EnumerateFiles(d).Sum(f => new FileInfo(f).Length));
        if (currentBytes + encoded.Size > options.Value.BusinessImageQuotaBytes) throw new ValidationException("Görsel depolama kotası doldu.");
        var directory = Path.Combine(root, folder, owner); Directory.CreateDirectory(directory);
        var file = Guid.NewGuid().ToString("N") + ".webp";
        await File.WriteAllBytesAsync(Path.Combine(directory, file), encoded.ToArray());
        return $"/uploads/{folder}/{owner}/{file}";
    }
    private string Physical(string value)
    {
        if (!value.StartsWith("/uploads/", StringComparison.Ordinal) || value.Contains('\\') || value.Contains('%') || value.Contains('?') || value.Contains('#') || value.Contains(':')) throw new ValidationException("Geçersiz görsel yolu.");
        var relative = value[9..]; var parts = relative.Split('/');
        if (parts.Any(p => string.IsNullOrWhiteSpace(p) || p.StartsWith('.') || p.Any(char.IsControl))) throw new ValidationException("Geçersiz görsel yolu.");
        var path = Path.GetFullPath(Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar)));
        if (!path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) throw new ValidationException("Geçersiz görsel yolu.");
        return path;
    }
    public async Task EnsureImageAccessAsync(string? path, int? businessId, bool isSuperAdmin = false)
    {
        if (string.IsNullOrWhiteSpace(path)) return;
        _ = Physical(path);
        if (isSuperAdmin) return;
        if (businessId is not > 0) throw new UnauthorizedAccessException();
        var parts = path[9..].Split('/');
        if (parts.Length == 3) { if (parts[1] != $"business-{businessId}") throw new UnauthorizedAccessException(); return; }
        var own = await db.Businesses.AnyAsync(b => b.Id == businessId && (b.LogoPath == path || b.BannerPath == path))
            || await db.Products.AnyAsync(p => p.BusinessId == businessId && p.ImagePath == path);
        var foreign = await db.Businesses.AnyAsync(b => b.Id != businessId && (b.LogoPath == path || b.BannerPath == path))
            || await db.Products.AnyAsync(p => p.BusinessId != businessId && p.ImagePath == path);
        if (!own || foreign) throw new UnauthorizedAccessException();
    }
    public async Task DeleteAsync(string path, int? businessId, bool isSuperAdmin = false)
    {
        if (string.IsNullOrWhiteSpace(path)) return;
        await EnsureImageAccessAsync(path, businessId, isSuperAdmin);
        var physical = Physical(path); if (File.Exists(physical)) File.Delete(physical);
    }
}
