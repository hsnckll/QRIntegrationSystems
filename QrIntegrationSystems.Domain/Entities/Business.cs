using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QrIntegrationSystems.Domain.Entities
{
    public class Business
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string? OwnerName { get; set; }
        public string? Phone { get; set; }
        public string Email { get; set; } = null!;
        public string? Address { get; set; }
        public string? LogoPath { get; set; }
        public string? BannerPath { get; set; }
        public int? TemplateId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        // İlişki: Bir işletme bir template'e bağlı
        public Template? Template { get; set; }
        // İlişki: Bir işletmenin birden fazla kategorisi olabilir
        public ICollection<Category> Categories { get; set; } = new List<Category>();
        // İlişki: Bir işletmenin birden fazla ürünü olabilir
        public ICollection<Product> Products { get; set; } = new List<Product>();
        // İlişki: Bir işletmenin birden fazla QR kodu olabilir
        public ICollection<QRCode> QRCodes { get; set; } = new List<QRCode>();
        // İlişki: Bir işletmenin birden fazla QR taraması olabilir
        public ICollection<QRScan> QRScans { get; set; } = new List<QRScan>();
        // İlişki: Bir işletmenin birden fazla aboneliği olabilir
        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        // İlişki: Bir işletmenin birden fazla log kaydı olabilir
        public ICollection<Log> Logs { get; set; } = new List<Log>();
    }
}
