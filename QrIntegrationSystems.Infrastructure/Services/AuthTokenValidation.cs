using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using QrIntegrationSystems.Infrastructure.Data;

namespace QrIntegrationSystems.Infrastructure.Services;

public static class AuthTokenValidation
{
    public static async Task<bool> IsCurrentAsync(ClaimsPrincipal user, AppDbContext db, AuthSessionStore sessions)
    {
        var jti = user.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
        if (string.IsNullOrEmpty(jti) || sessions.IsRevoked(jti)) return false;
        if (user.IsInRole("Business"))
        {
            if (!int.TryParse(user.FindFirst("BusinessId")?.Value, out var id)) return false;
            return await db.Businesses.AsNoTracking().AnyAsync(b => b.Id == id && b.IsActive && !b.IsDeleted)
                && user.FindFirst("sessionVersion")?.Value == sessions.BusinessVersion(id);
        }
        if (user.IsInRole("SuperAdmin"))
        {
            var email = user.FindFirst(ClaimTypes.Email)?.Value;
            return await db.SuperAdmins.AsNoTracking().AnyAsync(a => a.Email == email);
        }
        return false;
    }
}
