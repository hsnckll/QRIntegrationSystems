using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QrIntegrationSystems.Application.DTOs.Auth
{
    // Giriş başarılı olunca dönen cevap
    public class TokenResponseDto
    {
        public string Token { get; set; } = null!; // JWT kimlik kartı
        public string Role { get; set; } = null!; // Superadmin veya işletme sahibi
        public DateTime ExpiresAt { get; set; } // Ne zamana kadar geçerli
    }
}
