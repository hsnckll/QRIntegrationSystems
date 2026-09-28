using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QrIntegrationSystems.Domain.Entities
{
    public class QRCode
    {
        public int Id { get; set; }
        public int BusinessId { get; set; }
        public string QRImagePath { get; set; } = null!;
        public string TargetUrl { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        // İlişki: Bir QR kod bir işletmeye aittir
        public Business Business { get; set; } = null!;
        // İlişki: Bir QR kodun birden fazla taraması olabilir
        public ICollection<QRScan> QRScans { get; set; } = new List<QRScan>();
    }
}
