using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QrIntegrationSystems.API.Middlewares;
using QrIntegrationSystems.Application.Interfaces;
using QrIntegrationSystems.Domain.Entities;
using QrIntegrationSystems.Infrastructure.Data;
using QrIntegrationSystems.Infrastructure.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("QrIntegrationSystems.Infrastructure")));

// Servis Kayıtları
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IBusinessService, BusinessService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<ITemplateService, TemplateService>();
builder.Services.AddScoped<IQrCodeService, QrCodeService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IEmailService, EmailService>(); // <-- E-posta Servisi Eklendi

var jwtConfig = builder.Configuration.GetSection("JwtSettings");
var secret = jwtConfig["SecretKey"] ?? "QrMenuDefaultSecretKeyMustBe32CharsLong!";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ValidateIssuer = true,
            ValidIssuer = jwtConfig["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtConfig["Audience"],
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "QR Menu API", Version = "v1" });

    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Token girin"
    };

    opt.AddSecurityDefinition("Bearer", scheme);
    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // 1. ŞABLON: Doy Doy Döner Özel Şablonu
    var doydoyTemplate = db.Templates.FirstOrDefault(t => t.FolderName == "doydoy");
    if (doydoyTemplate == null)
    {
        doydoyTemplate = new Template
        {
            Name = "Doy Doy Döner Özel",
            FolderName = "doydoy",
            CreatedAt = DateTime.UtcNow
        };
        db.Templates.Add(doydoyTemplate);
        db.SaveChanges();
    }

    // 2. SÜPER ADMİN: Sistem Yöneticisi Girişi
    var superAdmin = db.SuperAdmins.FirstOrDefault(s => s.Email == "smartresidora@gmail.com");
    if (superAdmin == null)
    {
        var legacyAdmin = db.SuperAdmins.FirstOrDefault(s => s.Email == "admin@qrmenu.com");
        if (legacyAdmin != null)
        {
            legacyAdmin.Email = "smartresidora@gmail.com";
            legacyAdmin.Name = "Sistem Yöneticisi";
            legacyAdmin.Password = BCrypt.Net.BCrypt.HashPassword("Admin123!");
        }
        else
        {
            db.SuperAdmins.Add(new SuperAdmin
            {
                Name = "Sistem Yöneticisi",
                Email = "smartresidora@gmail.com",
                Password = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                CreatedAt = DateTime.UtcNow
            });
        }
        db.SaveChanges();
    }

    // 3. İŞLETME: Doy Doy Hatay Döner (Giriş ve OTP Testi İçin)
    var business = db.Businesses.FirstOrDefault(b => b.Slug == "doy-doy" || b.Email == "smartresidora@gmail.com");
    if (business == null)
    {
        var newBusiness = new Business
        {
            Name = "Doy Doy Hatay Döner",
            Slug = "doy-doy",
            OwnerName = "İşletme Sahibi",
            Phone = "0535 106 59 27",
            Email = "smartresidora@gmail.com",
            TemplateId = doydoyTemplate.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        db.Businesses.Add(newBusiness);
        db.SaveChanges();
    }
    else
    {
        business.TemplateId = doydoyTemplate.Id;
        business.Email = "smartresidora@gmail.com";
        db.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowReactApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
