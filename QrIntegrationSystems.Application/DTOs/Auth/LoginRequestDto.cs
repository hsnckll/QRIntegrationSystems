using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QrIntegrationSystems.Application.DTOs.Auth
{
    // İşletme sahibi email girip kod gönder butonuna bastığı zaman bu email verisini karşılayacak.
    public class LoginRequestDto
    {
        [Required, EmailAddress, StringLength(254)]
        public string Email { get; set; } = null!;
    }
}
