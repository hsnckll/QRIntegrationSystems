using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QrIntegrationSystems.Domain.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public int BusinessId { get; set; }
        public string Name { get; set; } = null!;
        public int SortOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        // İlişki: Bir kategori bir işletmeye aittir
        public Business Business { get; set; } = null!;
        // İlişki: Bir kategorinin birden fazla ürünü olabilir
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
