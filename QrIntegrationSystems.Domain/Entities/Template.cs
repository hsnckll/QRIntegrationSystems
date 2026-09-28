using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QrIntegrationSystems.Domain.Entities
{
    public class Template
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string FolderName { get; set; } = null!;
        public string? ThumbnailPath { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // İlişki: Bir template birden fazla işletme tarafından kullanılabilir
        public ICollection<Business> Businesses { get; set; } = new List<Business>();
    }
}
