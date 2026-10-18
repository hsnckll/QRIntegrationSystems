using System.ComponentModel.DataAnnotations;
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
        [Required, EmailAddress, StringLength(254)]
        public string Email { get; set; } = null!;
        [Required, StringLength(72)]
        public string Password { get; set; } = null!;
    }
}
