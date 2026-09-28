using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QrIntegrationSystems.Domain.Entities
{
    public class QRScan
    {
        public int Id { get; set; }
        public int BusinessId { get; set; }
        public int QRCodeId { get; set; }
        public DateTime ScannedAt { get; set; } = DateTime.Now;
        // İlişki: Bir tarama bir işletmeye aittir
        public Business Business { get; set; } = null!;
        // İlişki: Bir tarama bir QR koda aittir
        public QRCode QRCode { get; set; } = null!;
    }
}
