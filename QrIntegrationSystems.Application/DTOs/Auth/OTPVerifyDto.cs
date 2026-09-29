using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QrIntegrationSystems.Application.DTOs.Auth
{
    
    // İşletme sahibi emailine gelen 6 haneli kodu yazıp "Giriş Yap" butonuna basıyor.
    // Frontend → API'ye şunu gönderiyor:
    // { "email": "cafe@istanbul.com", "code": "482951" }
    // Bu veriyi bu kısım karşılıyor ve doğrulanıyor. Kodu doğrulama kısımı burada.
    
    public class OTPVerifyDto
    {
        public string Email { get; set; } = null!;
        public string Code { get; set; } = null!;
    }
}
