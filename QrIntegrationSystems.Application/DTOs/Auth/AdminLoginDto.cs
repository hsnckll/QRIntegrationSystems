using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QrIntegrationSystems.Application.DTOs.Auth
{
    // SuperAdmin giriş (email + şifre)
    public class AdminLoginDto
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
