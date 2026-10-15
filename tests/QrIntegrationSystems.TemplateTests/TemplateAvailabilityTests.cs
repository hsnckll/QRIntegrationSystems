using System.ComponentModel.DataAnnotations;
using System.IO.Compression;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QrIntegrationSystems.API.Controllers;
using QrIntegrationSystems.Application.DTOs.Business;
using QrIntegrationSystems.Application.DTOs.Template;
using QrIntegrationSystems.Domain.Entities;
using QrIntegrationSystems.Infrastructure.Configuration;
using QrIntegrationSystems.Infrastructure.Data;
using QrIntegrationSystems.Infrastructure.Services;
using Xunit;

namespace QrIntegrationSystems.TemplateTests;

// Uses an isolated SQLite in-memory DB and owned temporary files, never application settings or live storage.
public sealed class TemplateAvailabilityTests : IAsyncLifetime
{
    private readonly string testBase = Path.Combine(Path.GetTempPath(), "okutye-template-tests");
    private readonly string root;
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private AppDbContext db = null!;
    private TemplatePackageStore store = null!;
    private TemplatePackageController controller = null!;
    private BusinessService businesses = null!;

    public TemplateAvailabilityTests() => root = Path.Combine(testBase, Guid.NewGuid().ToString("N"));

    public async Task InitializeAsync()
    {
        await connection.OpenAsync();
        db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        var options = Options.Create(new StorageOptions { TemplatesPath = Path.Combine(root, "templates"), AuthPath = Path.Combine(root, "auth") });
        store = new TemplatePackageStore(options);
        controller = new TemplatePackageController(db, store);
        businesses = new BusinessService(db, store, new AuthSessionStore(options));
    }

    public async Task DisposeAsync()
    {
        await db.DisposeAsync();
        await connection.DisposeAsync();
        if (Path.GetDirectoryName(Path.GetFullPath(root)) != Path.GetFullPath(testBase)) throw new InvalidOperationException("Unexpected test directory.");
        if (Directory.Exists(root)) Directory.Delete(root, true);
    }

    private static readonly TemplateManifest Manifest = new(1, "kiyi-balikci", "Kıyı Balıkçısı", "1.0.0", "index.html", 1);

    private async Task<IActionResult> Upload(bool zip = true, TemplateManifest? manifest = null)
    {
        var contents = new Dictionary<string, string>
        {
            ["index.html"] = "<!doctype html><html lang=tr><body>Test menüsü</body></html>",
            ["template.json"] = JsonSerializer.Serialize(manifest ?? Manifest),
            ["assets/menu.js"] = "console.log('fixture');"
        };
        using var archive = new MemoryStream();
        var form = new PackageUpload();
        if (zip)
        {
            using (var writer = new ZipArchive(archive, ZipArchiveMode.Create, true))
                foreach (var (name, value) in contents)
                {
                    using var text = new StreamWriter(writer.CreateEntry(name).Open());
                    text.Write(value);
                }
            archive.Position = 0;
            form.Archive = new FormFile(archive, 0, archive.Length, "Archive", "kiyi.zip");
        }
        else
        {
            form.Paths = JsonSerializer.Serialize(contents.Keys);
            foreach (var (name, value) in contents)
            {
                var data = new MemoryStream(Encoding.UTF8.GetBytes(value));
                form.Files.Add(new FormFile(data, 0, data.Length, "Files", Path.GetFileName(name)));
            }
        }
        return await controller.Upload(form, CancellationToken.None);
    }

    private async Task<List<TemplateResponseDto>> ListAs(string role)
    {
        var list = new TemplateController(new TemplateService(db, store))
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role, role)], "test")) } }
        };
        return Assert.IsType<List<TemplateResponseDto>>(Assert.IsType<OkObjectResult>(await list.GetAll()).Value);
    }

    private async Task<Template> LegacyPackage(bool withOldMarker)
    {
        var directory = store.DirectoryFor(Manifest.Id);
        Directory.CreateDirectory(directory);
        await File.WriteAllTextAsync(Path.Combine(directory, "template.json"), JsonSerializer.Serialize(Manifest));
        await File.WriteAllTextAsync(Path.Combine(directory, "index.html"), "<html>Existing package</html>");
        await File.WriteAllTextAsync(Path.Combine(directory, ".capability"), Guid.NewGuid().ToString("N"));
        if (withOldMarker) await File.WriteAllTextAsync(Path.Combine(directory, ".published"), DateTime.UtcNow.ToString("O"));
        var template = new Template { Name = Manifest.Name, FolderName = Manifest.Id };
        db.Templates.Add(template);
        await db.SaveChangesAsync();
        return template;
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task UploadIsImmediatelyListedAssignableAndRendersPublicMenu(bool zip)
    {
        var result = Assert.IsType<OkObjectResult>(await Upload(zip));
        var response = JsonSerializer.SerializeToElement(result.Value);
        Assert.False(response.TryGetProperty("isPublished", out _));
        var template = Assert.Single(await db.Templates.ToListAsync());
        Assert.True(store.IsAvailable(template.FolderName));
        Assert.False(File.Exists(Path.Combine(store.DirectoryFor(template.FolderName), ".published")));
        Assert.NotNull(Assert.Single(await ListAs("Business")).EntryUrl);
        Assert.IsType<OkObjectResult>(await controller.Runtime(template.FolderName));

        var created = await businesses.CreateAsync(new CreateBusinessDto { Name = "Test işletmesi", Slug = "test-isletmesi", Email = "test@example.com", TemplateId = template.Id });
        Assert.Equal(template.Id, created.Template!.Id);
        Assert.Equal(template.FolderName, (await new MenuService(db, store).GetMenuBySlugAsync("test-isletmesi"))!.TemplateName);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ExistingDraftAndPreviouslyPublishedPackagesBothWorkWithoutReupload(bool withOldMarker)
    {
        var template = await LegacyPackage(withOldMarker);
        Assert.Equal(template.Id, Assert.Single(await ListAs("Business")).Id);
        Assert.IsType<OkObjectResult>(await controller.Runtime(template.FolderName));
        var created = await businesses.CreateAsync(new CreateBusinessDto { Name = "Atanmamış işletme", Slug = "atanmamis", Email = "unassigned@example.com" });
        Assert.Null(created.Template);
        Assert.Equal("", (await new MenuService(db, store).GetMenuBySlugAsync("atanmamis"))!.TemplateName);
        var updated = await businesses.UpdateAsync(created.Id, new UpdateBusinessDto { Name = created.Name, TemplateId = template.Id });
        Assert.Equal(template.Id, updated.Template!.Id);
        Assert.Equal(template.FolderName, (await new MenuService(db, store).GetMenuBySlugAsync("atanmamis"))!.TemplateName);
        await Assert.ThrowsAsync<ValidationException>(() => new TemplateService(db, store).DeleteAsync(template.Id));
    }

    [Theory]
    [InlineData("entry")]
    [InlineData("capability")]
    [InlineData("manifest")]
    public async Task BrokenPackagesAreNotSelectableEvenWithOldPublishedMarker(string missing)
    {
        var template = await LegacyPackage(true);
        var directory = store.DirectoryFor(template.FolderName);
        if (missing == "entry") File.Delete(Path.Combine(directory, "index.html"));
        if (missing == "capability") File.Delete(Path.Combine(directory, ".capability"));
        if (missing == "manifest") await File.WriteAllTextAsync(Path.Combine(directory, "template.json"), "not-json");
        Assert.False(store.IsAvailable(template.FolderName));
        Assert.Empty(await ListAs("Business"));
        Assert.Null(Assert.Single(await ListAs("SuperAdmin")).EntryUrl);
        Assert.IsType<NotFoundObjectResult>(await controller.Runtime(template.FolderName));
        await Assert.ThrowsAsync<ValidationException>(() => businesses.CreateAsync(new CreateBusinessDto { Name = "Test", Slug = "test", Email = "test@example.com", TemplateId = template.Id }));
    }

    [Fact]
    public async Task InvalidUploadDoesNotBecomeAnAvailableTemplate()
    {
        Assert.IsType<BadRequestObjectResult>(await Upload(manifest: Manifest with { MenuContractVersion = 2 }));
        Assert.Empty(await db.Templates.ToListAsync());
        Assert.False(store.IsAvailable(Manifest.Id));
        Assert.Empty(Directory.EnumerateDirectories(store.Root));
    }
}
