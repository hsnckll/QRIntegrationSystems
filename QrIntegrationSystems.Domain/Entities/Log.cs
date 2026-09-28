using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QrIntegrationSystems.Domain.Entities
{
    public class Log
    {
        public int Id { get; set; }
        public int BusinessId { get; set; }
        public string ActorType { get; set; } = null!;      // "Admin" veya "Business"
        public string Action { get; set; } = null!;          // "Ürün güncellendi" gibi
        public string EntityType { get; set; } = null!;      // "Product", "Category" gibi
        public int EntityId { get; set; }                     // Hangi kaydın Id'si
        public string? OldValues { get; set; }                // Eski değerler (JSON)
        public string? NewValues { get; set; }                // Yeni değerler (JSON)
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        // İlişki: Bir log kaydı bir işletmeye aittir
        public Business Business { get; set; } = null!;
    }
}
