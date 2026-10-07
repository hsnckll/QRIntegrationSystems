using Microsoft.EntityFrameworkCore;
using QrIntegrationSystems.Domain.Entities;

namespace QrIntegrationSystems.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public string? AuditActor { get; set; }

        // Store scalar snapshots only; exclude OTPs, credentials and scan traffic.
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ChangeTracker.DetectChanges();
            var changes = ChangeTracker.Entries()
                .Where(e => AuditActor != null &&
                    (e.Entity is Business or Category or Product or Subscription or QRCode) &&
                    (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted))
                .Select(e => new { Entry = e, State = e.State,
                    Old = e.State == EntityState.Added ? null : System.Text.Json.JsonSerializer.Serialize(
                        e.OriginalValues.Properties.ToDictionary(p => p.Name, p => e.OriginalValues[p])) })
                .ToList();
            if (changes.Count == 0) return await base.SaveChangesAsync(cancellationToken);
            var ownsTransaction = Database.IsRelational() && Database.CurrentTransaction == null;
            await using var transaction = ownsTransaction ? await Database.BeginTransactionAsync(cancellationToken) : null;
            var result = await base.SaveChangesAsync(cancellationToken);
            foreach (var change in changes)
            {
                var entry = change.Entry;
                var entityId = (int)entry.Property("Id").CurrentValue!;
                var businessId = entry.Entity is Business ? entityId : (int)entry.Property("BusinessId").CurrentValue!;
                var label = entry.Entity switch { Business => "İşletme", Category => "Kategori",
                    Product => "Ürün", Subscription => "Abonelik", _ => "QR kodu" };
                var deleted = change.State == EntityState.Deleted ||
                    (entry.Metadata.FindProperty("IsDeleted") != null && (bool)entry.Property("IsDeleted").CurrentValue!);
                Logs.Add(new Log { BusinessId = businessId, EntityId = entityId,
                    EntityType = entry.Metadata.ClrType.Name, ActorType = AuditActor!,
                    Action = label + (deleted ? " silindi" : change.State == EntityState.Added ? " oluşturuldu" : " güncellendi"),
                    OldValues = change.Old, NewValues = change.State == EntityState.Deleted ? null :
                        System.Text.Json.JsonSerializer.Serialize(entry.CurrentValues.Properties.ToDictionary(p => p.Name, p => entry.CurrentValues[p])),
                    CreatedAt = DateTime.UtcNow });
            }
            await base.SaveChangesAsync(cancellationToken);
            if (transaction != null) await transaction.CommitAsync(cancellationToken);
            return result;
        }

        // Tablolarımız
        public DbSet<SuperAdmin> SuperAdmins { get; set; }
        public DbSet<Template> Templates { get; set; }
        public DbSet<Business> Businesses { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<OTPCode> OTPCodes { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<QRCode> QRCodes { get; set; }
        public DbSet<QRScan> QRScans { get; set; }
        public DbSet<Log> Logs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Supabase (PostgreSQL) şema ayarı (Gereklidir)
            modelBuilder.HasDefaultSchema("public");

            // --- İLİŞKİLER (FLUENT API) ---

            // Business -> Template (1:N)
            modelBuilder.Entity<Business>()
                .HasOne(b => b.Template)
                .WithMany(t => t.Businesses)
                .HasForeignKey(b => b.TemplateId);

            // Business -> Category (1:N)
            modelBuilder.Entity<Category>()
                .HasOne(c => c.Business)
                .WithMany(b => b.Categories)
                .HasForeignKey(c => c.BusinessId)
                .OnDelete(DeleteBehavior.Restrict); // Business silinirse kategoriler silinmesin (Soft Delete yapıyoruz zaten)

            // Business -> Product (1:N)
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Business)
                .WithMany(b => b.Products)
                .HasForeignKey(p => p.BusinessId)
                .OnDelete(DeleteBehavior.Restrict);

            // Category -> Product (1:N)
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Business -> QRCode (1:N)
            modelBuilder.Entity<QRCode>()
                .HasOne(q => q.Business)
                .WithMany(b => b.QRCodes)
                .HasForeignKey(q => q.BusinessId)
                .OnDelete(DeleteBehavior.Restrict);

            // QRCode -> QRScan (1:N)
            modelBuilder.Entity<QRScan>()
                .HasOne(qs => qs.QRCode)
                .WithMany(q => q.QRScans)
                .HasForeignKey(qs => qs.QRCodeId)
                .OnDelete(DeleteBehavior.Cascade); // QR kod silinirse okuma geçmişi silinebilir

            // Business -> QRScan (1:N)
            modelBuilder.Entity<QRScan>()
                .HasOne(qs => qs.Business)
                .WithMany(b => b.QRScans)
                .HasForeignKey(qs => qs.BusinessId)
                .OnDelete(DeleteBehavior.Restrict);

            // Business -> Subscription (1:N)
            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.Business)
                .WithMany(b => b.Subscriptions)
                .HasForeignKey(s => s.BusinessId)
                .OnDelete(DeleteBehavior.Restrict);

            // Business -> Log (1:N)
            modelBuilder.Entity<Log>()
                .HasOne(l => l.Business)
                .WithMany(b => b.Logs)
                .HasForeignKey(l => l.BusinessId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}