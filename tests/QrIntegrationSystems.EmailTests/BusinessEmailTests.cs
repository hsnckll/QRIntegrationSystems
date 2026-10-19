using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QrIntegrationSystems.API;
using QrIntegrationSystems.API.Controllers;
using QrIntegrationSystems.Application.DTOs.Auth;
using QrIntegrationSystems.Application.DTOs.Business;
using QrIntegrationSystems.Application.Interfaces;
using QrIntegrationSystems.Domain.Entities;
using QrIntegrationSystems.Infrastructure.Configuration;
using QrIntegrationSystems.Infrastructure.Data;
using QrIntegrationSystems.Infrastructure.Services;
using Xunit;

namespace QrIntegrationSystems.EmailTests;

// All data is isolated: in-memory SQLite, owned temporary session files, fake SMTP.
public sealed class BusinessEmailTests : IAsyncLifetime
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private readonly string testBase = Path.Combine(Path.GetTempPath(), "okutye-business-email-tests");
    private readonly string root;
    private AppDbContext db = null!;
    private BusinessService service = null!;
    private AuthSessionStore sessions = null!;
    private AuthService auth = null!;
    private readonly RecordingEmail email = new();

    public BusinessEmailTests() => root = Path.Combine(testBase, Guid.NewGuid().ToString("N"));

    public async Task InitializeAsync()
    {
        await connection.OpenAsync();
        db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        var storage = Options.Create(new StorageOptions { AuthPath = Path.Combine(root, "auth"), TemplatesPath = Path.Combine(root, "templates") });
        sessions = new AuthSessionStore(storage);
        service = new BusinessService(db, new TemplatePackageStore(storage), sessions);
        auth = new AuthService(db, Options.Create(new JwtOptions { SecretKey = "isolated-business-email-test-secret-key", Issuer = "test", Audience = "test" }), email, sessions);
        db.Businesses.AddRange(
            new Business { Id = 1, Name = "First business", Slug = "first", Email = "old@example.com" },
            new Business { Id = 2, Name = "Other business", Slug = "other", Email = "taken@example.com" },
            new Business { Id = 3, Name = "Deleted business", Slug = "deleted", Email = "previous@example.com", IsDeleted = true });
        db.SuperAdmins.Add(new SuperAdmin { Name = "Admin", Email = "admin@example.com", Password = "unused-test-hash" });
        await db.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await db.DisposeAsync(); await connection.DisposeAsync();
        if (Path.GetDirectoryName(Path.GetFullPath(root)) != Path.GetFullPath(testBase)) throw new InvalidOperationException("Unexpected test root.");
        if (Directory.Exists(root)) Directory.Delete(root, true);
    }

    private static UpdateBusinessDto Update(string? address) => new() { Name = "First business", Email = address };
    private ClaimsPrincipal User(string role, int id = 1) => new(new ClaimsIdentity([
        new Claim(ClaimTypes.Role, role), new Claim("BusinessId", id.ToString()),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
        new Claim("sessionVersion", sessions.BusinessVersion(id))
    ], "test"));

    private BusinessController Controller(string role, int ownId = 1)
    {
        var context = new DefaultHttpContext { User = User(role, ownId) };
        var scope = new BusinessScopeResolver(new HttpContextAccessor { HttpContext = context });
        return new BusinessController(service, null!, null!, scope) { ControllerContext = new ControllerContext { HttpContext = context } };
    }

    [Theory]
    [InlineData("SuperAdmin")]
    [InlineData("Business")]
    public async Task BothPanelsCanPersistNormalizedEmailAndAuditChange(string role)
    {
        db.AuditActor = role;
        var result = Assert.IsType<OkObjectResult>(await Controller(role).Update(1, Update(" NEW@EXAMPLE.COM ")));
        Assert.Equal("new@example.com", Assert.IsType<BusinessDetailDto>(result.Value).Email);
        db.ChangeTracker.Clear();
        Assert.Equal("new@example.com", (await service.GetByIdAsync(1)).Email);
        Assert.Equal("first", (await service.GetByIdAsync(1)).Slug);
        var log = await db.Logs.SingleAsync();
        Assert.Contains("old@example.com", log.OldValues!);
        Assert.Contains("new@example.com", log.NewValues!);
    }

    [Theory]
    [InlineData("taken@example.com")]
    [InlineData("ADMIN@EXAMPLE.COM")]
    [InlineData("")]
    [InlineData("invalid-address")]
    public async Task InvalidConflictingAndAdminEmailDoNotChangeProfileOrSessions(string address)
    {
        var version = sessions.BusinessVersion(1);
        await Assert.ThrowsAsync<ValidationException>(() => service.UpdateAsync(1, Update(address)));
        Assert.Equal("old@example.com", (await service.GetByIdAsync(1)).Email);
        Assert.Equal(version, sessions.BusinessVersion(1));
    }

    [Fact]
    public async Task InactiveBusinessEmailStillCannotBeTaken()
    {
        (await db.Businesses.FindAsync(2))!.IsActive = false;
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<ValidationException>(() => service.UpdateAsync(1, Update("taken@example.com")));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(" OLD@EXAMPLE.COM ")]
    public async Task MissingOrSemanticallyUnchangedEmailPreservesSessionAndPendingCodes(string? address)
    {
        var version = sessions.BusinessVersion(1);
        db.OTPCodes.Add(new OTPCode { Email = "old@example.com", Code = "isolated-digest", ExpiresAt = DateTime.UtcNow.AddMinutes(5) });
        await db.SaveChangesAsync();
        await service.UpdateAsync(1, Update(address));
        Assert.Equal("old@example.com", (await service.GetByIdAsync(1)).Email);
        Assert.Equal(version, sessions.BusinessVersion(1));
        Assert.False((await db.OTPCodes.SingleAsync()).IsUsed);
    }

    [Fact]
    public async Task EmailChangeInvalidatesOldSessionsAndCodesAndNewAddressCanLogin()
    {
        await auth.SendOtpAsync(new LoginRequestDto { Email = "old@example.com" }, "test-ip");
        var oldUser = User("Business");
        var otherVersion = sessions.BusinessVersion(2);
        Assert.True(await AuthTokenValidation.IsCurrentAsync(oldUser, db, sessions));
        await service.UpdateAsync(1, Update("new@example.com"));
        Assert.True((await db.OTPCodes.SingleAsync()).IsUsed);
        Assert.False(await AuthTokenValidation.IsCurrentAsync(oldUser, db, sessions));
        Assert.Equal(otherVersion, sessions.BusinessVersion(2));
        await Assert.ThrowsAsync<ValidationException>(() => auth.SendOtpAsync(new LoginRequestDto { Email = "old@example.com" }, "test-ip"));
        await auth.SendOtpAsync(new LoginRequestDto { Email = "new@example.com" }, "test-ip");
        Assert.Equal("new@example.com", email.Recipient);
        var result = await auth.VerifyOtpAsync(new OTPVerifyDto { Email = "new@example.com", Code = email.Code }, "test-ip");
        Assert.Equal("Business", result.Role);
    }

    [Fact]
    public async Task ReusedDeletedAccountAddressCannotReuseItsPreviousOtp()
    {
        db.OTPCodes.Add(new OTPCode { Email = "previous@example.com", Code = "old-challenge", ExpiresAt = DateTime.UtcNow.AddMinutes(5) });
        await db.SaveChangesAsync();
        await service.UpdateAsync(1, Update("previous@example.com"));
        Assert.True((await db.OTPCodes.SingleAsync()).IsUsed);
    }

    [Fact]
    public async Task BusinessUserCannotChangeAnotherBusinessEmail()
    {
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Controller("Business", ownId: 2).Update(1, Update("new@example.com")));
        Assert.Equal("old@example.com", (await service.GetByIdAsync(1)).Email);
    }

    [Fact]
    public void UpdateDtoRejectsMalformedEmail()
    {
        var dto = Update("bad-address");
        Assert.False(Validator.TryValidateObject(dto, new ValidationContext(dto), [], true));
    }

    private sealed class RecordingEmail : IEmailService
    {
        public string Recipient { get; private set; } = "";
        public string Code { get; private set; } = "";
        public Task SendAsync(string toEmail, string subject, string body, string? textBody = null)
        {
            Recipient = toEmail; Code = Regex.Match(textBody ?? body, @"(?<!\d)\d{6}(?!\d)").Value;
            return Task.CompletedTask;
        }
    }
}
