using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QrIntegrationSystems.Application.DTOs.Auth;
using QrIntegrationSystems.Application.Interfaces;
using QrIntegrationSystems.Domain.Entities;
using QrIntegrationSystems.Infrastructure.Configuration;
using QrIntegrationSystems.Infrastructure.Data;
using QrIntegrationSystems.Infrastructure.Services;
using Xunit;

namespace QrIntegrationSystems.TemplateTests;

// Authentication regression tests use only an isolated DB and a fake mail sender.
public sealed class AuthServiceTests : IAsyncLifetime
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private readonly string testBase = Path.Combine(Path.GetTempPath(), "okutye-auth-tests");
    private readonly string root;
    private readonly RecordingEmail sender = new();
    private AppDbContext db = null!;
    private AuthService auth = null!;
    private const string AdminAddress = "yonetimokutye@gmail.com";

    public AuthServiceTests() => root = Path.Combine(testBase, Guid.NewGuid().ToString("N"));

    public async Task InitializeAsync()
    {
        await connection.OpenAsync();
        db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        auth = new AuthService(db,
            Options.Create(new JwtOptions { SecretKey = "isolated-test-secret-with-at-least-32-bytes", Issuer = "test", Audience = "test" }),
            sender, new AuthSessionStore(Options.Create(new StorageOptions { AuthPath = root })));
    }

    public async Task DisposeAsync()
    {
        await db.DisposeAsync();
        await connection.DisposeAsync();
        if (Path.GetDirectoryName(Path.GetFullPath(root)) != Path.GetFullPath(testBase)) throw new InvalidOperationException("Unexpected test directory.");
        if (Directory.Exists(root)) Directory.Delete(root, true);
    }

    private async Task AddAdmin(string address = AdminAddress, string password = "unused-test-hash")
    {
        db.SuperAdmins.Add(new SuperAdmin { Name = "Test yönetici", Email = address, Password = password });
        await db.SaveChangesAsync();
    }

    private async Task AddBusiness(string address, bool active = true, bool deleted = false)
    {
        db.Businesses.Add(new Business { Name = "Test işletmesi", Slug = "test-isletmesi", Email = address, IsActive = active, IsDeleted = deleted });
        await db.SaveChangesAsync();
    }

    [Theory]
    [InlineData("yonetimokutye@gmail.com")]
    [InlineData("  YONETIMOKUTYE@GMAIL.COM  ")]
    public async Task AdminAddressCannotRequestBusinessOtp(string input)
    {
        await AddAdmin();
        var error = await Assert.ThrowsAsync<ValidationException>(() => auth.SendOtpAsync(new LoginRequestDto { Email = input }, "test-ip"));
        Assert.Contains("işletme girişi yapılamıyor", error.Message);
        Assert.Empty(sender.Messages);
        Assert.Empty(await db.OTPCodes.ToListAsync());
    }

    [Theory]
    [InlineData("unknown")]
    [InlineData("inactive")]
    [InlineData("deleted")]
    public async Task NonBusinessOrUnavailableBusinessDoesNotReportSuccessOrCreateOtp(string state)
    {
        if (state != "unknown") await AddBusiness("owner@example.com", active: state != "inactive", deleted: state == "deleted");
        await Assert.ThrowsAsync<ValidationException>(() => auth.SendOtpAsync(new LoginRequestDto { Email = "owner@example.com" }, "test-ip"));
        Assert.Empty(sender.Messages);
        Assert.Empty(await db.OTPCodes.ToListAsync());
    }

    [Fact]
    public async Task AdminDuplicatedInActiveBusinessesStillCannotReceiveBusinessOtp()
    {
        await AddAdmin("YONETIMOKUTYE@GMAIL.COM");
        await AddBusiness(AdminAddress);
        await Assert.ThrowsAsync<ValidationException>(() => auth.SendOtpAsync(new LoginRequestDto { Email = AdminAddress }, "test-ip"));
        Assert.Empty(sender.Messages);
        Assert.Empty(await db.OTPCodes.ToListAsync());
    }

    [Fact]
    public async Task PreviouslyIssuedOtpCannotCreateBusinessSessionAfterAddressBecomesAdmin()
    {
        await AddBusiness(AdminAddress);
        await auth.SendOtpAsync(new LoginRequestDto { Email = AdminAddress }, "test-ip");
        var code = sender.Code;
        await AddAdmin();
        await Assert.ThrowsAsync<ValidationException>(() => auth.VerifyOtpAsync(new OTPVerifyDto { Email = AdminAddress, Code = code }, "test-ip"));
        Assert.False((await db.OTPCodes.SingleAsync()).IsUsed);
    }

    [Fact]
    public async Task ActiveBusinessStillReceivesOtpAndCanLoginWithNormalizedAddress()
    {
        await AddAdmin();
        await AddBusiness("OWNER@EXAMPLE.COM");
        await auth.SendOtpAsync(new LoginRequestDto { Email = "  Owner@Example.com  " }, "test-ip");
        Assert.Equal("owner@example.com", Assert.Single(sender.Messages).Recipient);
        var result = await auth.VerifyOtpAsync(new OTPVerifyDto { Email = " OWNER@EXAMPLE.COM ", Code = sender.Code }, "test-ip");
        Assert.Equal("Business", result.Role);
        Assert.NotEmpty(result.Token);
        Assert.True((await db.OTPCodes.SingleAsync()).IsUsed);
    }

    [Fact]
    public async Task AdministratorPasswordLoginIsUnaffected()
    {
        const string password = "isolated-test-password";
        await AddAdmin(password: BCrypt.Net.BCrypt.HashPassword(password));
        var result = await auth.AdminLoginAsync(new AdminLoginDto { Email = AdminAddress, Password = password }, "test-ip");
        Assert.Equal("SuperAdmin", result.Role);
        Assert.Empty(sender.Messages);
        Assert.Empty(await db.OTPCodes.ToListAsync());
    }

    private sealed class RecordingEmail : IEmailService
    {
        public List<(string Recipient, string Text)> Messages { get; } = [];
        public string Code => Regex.Match(Messages.Last().Text, @"(?<!\d)\d{6}(?!\d)").Value;
        public Task SendAsync(string toEmail, string subject, string body, string? textBody = null)
        {
            Messages.Add((toEmail, textBody ?? body));
            return Task.CompletedTask;
        }
    }
}
